using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DirtTownMenu;

internal static class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        if(args.Length>0)
        {
            try
            {
                if(args[0]=="--memory-test-host")
                {
                    nint buffer=Native.VirtualAlloc(0,4096,0x3000,0x04);
                    if(buffer==0) throw new InvalidOperationException("VirtualAlloc failed");
                    Marshal.Copy(Convert.FromHexString("C5FA10B624D40000"),0,buffer,8);
                    if(!Native.VirtualProtect(buffer,4096,0x20,out _)) throw new InvalidOperationException("VirtualProtect failed");
                    Console.WriteLine(buffer.ToInt64());
                    Console.Out.Flush();
                    Console.ReadLine();
                    return 0;
                }
                if(args[0]=="--verify")
                {
                    BuildProfile.ValidateFile(args.Length>1?args[1]:BuildProfile.DefaultGame);
                    Console.WriteLine("PASS: SHA256, all original patch sites and 3 distance constants match build 26690600.");
                    return 0;
                }
                if(args[0]=="--self-test") return SelfTest.Run();
                if(args[0]=="--coop-self-test"){CoopSyncTests.Run().GetAwaiter().GetResult();return 0;}
                if(args[0]=="--loader-self-test")return LoaderTests.Run();
                if(args[0]=="--live-snapshot")
                {
                    var games=Process.GetProcessesByName("dirttown");
                    if(games.Length!=1)throw new IOException("Se requiere una partida abierta.");
                    Console.WriteLine("Backup: "+ModSession.CreateBackup(Path.Combine(AppContext.BaseDirectory,"backups")));
                    using var bridge=new LiveBridge(games[0]);
                    using var snapshot=bridge.Request(1).GetAwaiter().GetResult();
                    Console.WriteLine(snapshot.RootElement.GetRawText());
                    return 0;
                }
                if(args[0]=="--live-fill" || args[0]=="--live-unlock")
                {
                    var games=Process.GetProcessesByName("dirttown");if(games.Length!=1)throw new IOException("Abre una partida.");
                    using var bridge=new LiveBridge(games[0]);
                    using var result=args[0]=="--live-unlock"?bridge.Request(4).GetAwaiter().GetResult():bridge.Request(2,int.Parse(args[1]),int.Parse(args[2]),float.Parse(args[3],System.Globalization.CultureInfo.InvariantCulture)).GetAwaiter().GetResult();
                    Console.WriteLine(result.RootElement.GetRawText());return 0;
                }
                if(args[0]=="--live-action")
                {
                    using var input=System.Text.Json.JsonDocument.Parse(File.ReadAllText(args[1]));var v=input.RootElement;
                    var games=Process.GetProcessesByName("dirttown");if(games.Length!=1)throw new IOException("Abre una partida.");
                    using var bridge=new LiveBridge(games[0]);
                    float Number(string name,float fallback)=>v.TryGetProperty(name,out var p)?p.GetSingle():fallback;
                    using var result=bridge.Request(v.GetProperty("command").GetUInt32(),material:(int)Number("material",1),amount:Number("amount",1),x:Number("x",0),z:Number("z",0),model:v.TryGetProperty("model",out var model)?model.GetString()!:"",height:Number("height",1)).GetAwaiter().GetResult();
                    Console.WriteLine(result.RootElement.GetRawText());return 0;
                }
                throw new ArgumentException("Unknown option.");
            }
            catch(Exception ex) { Console.Error.WriteLine(ex.ToString()); return 1; }
        }
        using var singleton=new Mutex(true,@"Local\DirtTownMenu.SingleInstance",out bool first);
        if(!first) {MessageBox.Show("El menú ya está abierto. Pulsa F6 para mostrarlo.","Dirt Town Menu"); return 0;}
        ApplicationConfiguration.Initialize();
        Application.Run(new IntegratedHostForm());
        return 0;
    }
}

internal static class SelfTest
{
    public static int Run()
    {
        string sample="hydraulic_speed = 0.0 # automatic\r\nmouse = 1.0\r\n\r\n[camera_distance]\r\n\"loaders\" = 0.9\r\n[joysticks]\r\n'controller' = 'off'\r\n";
        string changed=SettingsEditor.SetRoot(sample,"hydraulic_speed","2.5");
        changed=SettingsEditor.SetRoot(changed,"follow_bucket","false");
        if(SettingsEditor.Read(changed,"hydraulic_speed","")!="2.5" || SettingsEditor.Read(changed,"follow_bucket","")!="false")throw new Exception("Settings update failed");
        if(!changed.Contains("# automatic") || changed[changed.IndexOf("[camera_distance]",StringComparison.Ordinal)..]!=sample[sample.IndexOf("[camera_distance]",StringComparison.Ordinal)..])throw new Exception("Settings comments or controller tables changed");
        if(changed.IndexOf("follow_bucket",StringComparison.Ordinal)>changed.IndexOf("[camera_distance]",StringComparison.Ordinal))throw new Exception("New root setting inserted into a TOML table");
        Console.WriteLine("PASS: Settings edits preserve comments, camera/controller tables and insert missing keys at the root.");
        BuildProfile.ValidateFile(BuildProfile.DefaultGame);
        Console.WriteLine("PASS: Installed executable, branch guards and all range constants verified.");
        var start=new ProcessStartInfo(Environment.ProcessPath!,"--memory-test-host")
        {UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardInput=true,RedirectStandardError=true};
        using var child=Process.Start(start) ?? throw new IOException("Test child did not start.");
        try
        {
            string line=child.StandardOutput.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult() ?? "";
            long address=long.Parse(line);
            using var memory=new RemoteMemory(child);
            byte[] original=Convert.FromHexString("C5FA10B624D40000");
            if(!memory.Read(address,8).SequenceEqual(original)) throw new Exception("Initial memory mismatch");
            using(memory.Freeze())
            {
                memory.Write(address,BuildProfile.Range(25)[0].Enabled);
                if(!memory.Read(address,8).SequenceEqual(BuildProfile.Range(25)[0].Enabled)) throw new Exception("Remote patch mismatch");
                memory.Write(address,original);
            }
            if(!memory.Read(address,8).SequenceEqual(original)) throw new Exception("Restore mismatch");
            Console.WriteLine("PASS: Native remote read/write on an RX page, instruction-cache flush and restoration.");
            child.StandardInput.WriteLine("exit");
            if(!child.WaitForExit(10000)) throw new Exception("Test child stayed suspended");
            if(child.ExitCode!=0) throw new Exception("Test child failed");
            Console.WriteLine("PASS: All suspended threads resumed; test child exited normally. No game process modified.");
            return 0;
        }
        finally {if(!child.HasExited) child.Kill();}
    }
}
