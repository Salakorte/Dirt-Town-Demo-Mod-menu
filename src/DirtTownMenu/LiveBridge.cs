using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

namespace DirtTownMenu;

internal sealed class LiveBridge:IDisposable
{
    readonly MemoryMappedFile mapping;
    readonly MemoryMappedViewAccessor view;
    readonly SemaphoreSlim gate=new(1);
    CoopSync? coop;
    int disposed;
    public string CoopStatus=>coop?.Status??"Sincronización cooperativa sin activar";
    public async Task EnableCoop()
    {
        if(coop is not null)return;
        var candidate=new CoopSync(new NativeCoopBackend(this));
        try{await candidate.Start().ConfigureAwait(false);coop=candidate;}catch{candidate.Dispose();throw;}
    }
    public LiveBridge(Process game)
    {
        BuildProfile.ValidateFile(game.MainModule!.FileName);
        string name=$"Local\\DirtTownMenuBridge8-{game.Id}";
        try{mapping=MemoryMappedFile.OpenExisting(name);}
        catch(FileNotFoundException)
        {
            Inject(game,Path.Combine(AppContext.BaseDirectory,"DirtTownBridge8.dll"));
            MemoryMappedFile? found=null;
            for(int i=0;i<100;i++){try{found=MemoryMappedFile.OpenExisting(name);break;}catch(FileNotFoundException){Thread.Sleep(50);}}
            mapping=found??throw new IOException("No se ha iniciado la conexión en tiempo real.");
        }
        view=mapping.CreateViewAccessor(0,8304);
        for(int i=0;i<100&&view.ReadUInt32(44)!=1;i++)Thread.Sleep(50);
        if(view.ReadUInt32(0)!=0x44544D33||view.ReadUInt32(44)!=1)throw new IOException("El puente no coincide con esta versión del juego.");
    }
    public async Task<JsonDocument> Request(uint command,int vehicle=0,int material=0,float amount=1,float x=0,float z=0,string model="",float height=1)
    {
        if(command==3)
        {
            if(coop is not null)
            {
                int id=await coop.Spawn(model,x,z).ConfigureAwait(false);
                return JsonDocument.Parse($"{{\"ok\":true,\"vehicle\":{id},\"synchronized\":true}}");
            }
            using var state=await RequestRaw(6).ConfigureAwait(false);
            if(state.RootElement.GetProperty("network").GetBoolean())throw new IOException("Activa la sincronización cooperativa en ambos equipos antes de crear vehículos.");
        }
        return await RequestRaw(command,vehicle,material,amount,x,z,model,height).ConfigureAwait(false);
    }
    internal async Task<JsonDocument> RequestRaw(uint command,int vehicle=0,int material=0,float amount=1,float x=0,float z=0,string model="",float height=1,string? payload=null)
    {
        await gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if(view.ReadUInt32(4)!=view.ReadUInt32(8))throw new IOException("Hay una acción pendiente. Vuelve al juego para que termine antes de enviar otra.");
            uint seq=view.ReadUInt32(4)+1;
            view.Write(16,vehicle);view.Write(20,material);view.Write(24,amount);view.Write(28,x);view.Write(32,z);
            view.Write(36,height);
            if(payload is not null)
            {
                byte[] input=Encoding.UTF8.GetBytes(payload);if(input.Length>=3072)throw new IOException("Mensaje de sincronización demasiado grande.");
                byte[] buffer=new byte[8192];input.CopyTo(buffer,0);view.WriteArray(112,buffer,0,buffer.Length);
            }
            byte[] bytes=new byte[64];Encoding.ASCII.GetBytes(model).CopyTo(bytes,0);view.WriteArray(48,bytes,0,64);
            view.Write(12,command);Thread.MemoryBarrier();view.Write(4,seq);
            for(int i=0;i<200;i++)
            {
                await Task.Delay(50).ConfigureAwait(false);
                if(view.ReadUInt32(8)==seq)
                {
                    byte[] response=new byte[8192];view.ReadArray(112,response,0,response.Length);
                    int end=Array.IndexOf(response,(byte)0);var doc=JsonDocument.Parse(Encoding.UTF8.GetString(response,0,end<0?response.Length:end));
                    if(!doc.RootElement.GetProperty("ok").GetBoolean())throw new IOException(doc.RootElement.GetProperty("error").GetString());
                    return doc;
                }
            }
            throw new TimeoutException("La acción sigue pendiente. Vuelve al juego y reanuda la partida; se ejecutará cuando avance la simulación. No la envíes otra vez.");
        }
        finally{gate.Release();}
    }
    static void Inject(Process game,string dll)
    {
        if(!File.Exists(dll))throw new FileNotFoundException("Falta el puente nativo junto al menú. Vuelve a compilar con Build.ps1.");
        nint h=Native.OpenProcess(0x43A,false,game.Id);
        if(h==0)throw new IOException("No se pudo abrir el juego para conectar el puente.");
        nint remote=0,thread=0;
        try
        {
            byte[] data=Encoding.Unicode.GetBytes(Path.GetFullPath(dll)+"\0");
            remote=VirtualAllocEx(h,0,(nuint)data.Length,0x3000,4);
            if(remote==0||!Native.WriteProcessMemory(h,remote,data,(nuint)data.Length,out var count)||count!=(nuint)data.Length)throw new IOException("No se pudo preparar la conexión.");
            nint loader=GetProcAddress(GetModuleHandle("kernel32.dll"),"LoadLibraryW");
            thread=CreateRemoteThread(h,0,0,loader,remote,0,0);
            if(thread==0)throw new IOException("No se pudo cargar el puente.");
            if(WaitForSingleObject(thread,30000)!=0){remote=0;throw new IOException("La carga sigue pendiente. Reinicia el juego antes de reintentar.");}
        }
        finally{if(thread!=0)Native.CloseHandle(thread);if(remote!=0)VirtualFreeEx(h,remote,0,0x8000);Native.CloseHandle(h);}
    }
    public void Dispose()
    {
        if(Interlocked.Exchange(ref disposed,1)!=0)return;
        _=Task.Run(async()=>
        {
            if(coop is not null)await coop.Stop().ConfigureAwait(false);
            await gate.WaitAsync().ConfigureAwait(false);
            try{view.Dispose();mapping.Dispose();}finally{gate.Release();}
        });
    }
    [DllImport("kernel32.dll",SetLastError=true)]static extern nint VirtualAllocEx(nint h,nint address,nuint size,uint type,uint protect);
    [DllImport("kernel32.dll")]static extern bool VirtualFreeEx(nint h,nint address,nuint size,uint type);
    [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]static extern nint GetModuleHandle(string name);
    [DllImport("kernel32.dll",CharSet=CharSet.Ansi)]static extern nint GetProcAddress(nint module,string name);
    [DllImport("kernel32.dll",SetLastError=true)]static extern nint CreateRemoteThread(nint h,nint attributes,nuint stack,nint start,nint param,uint flags,nint id);
    [DllImport("kernel32.dll")]static extern uint WaitForSingleObject(nint h,uint milliseconds);
}
