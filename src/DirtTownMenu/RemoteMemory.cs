using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DirtTownMenu;

internal sealed class RemoteMemory : IDisposable
{
    readonly nint handle;
    readonly Process process;
    public bool HasExited => process.HasExited;
    public RemoteMemory(Process target)
    {
        process = target;
        handle = Native.OpenProcess(0x38 | 0x1000, false, target.Id);
        if (handle == 0) throw new Win32Exception(Marshal.GetLastWin32Error(), "No se puede acceder al juego. Ejecuta el juego y el menú con el mismo nivel de permisos.");
    }
    public byte[] Read(long address, int count)
    {
        byte[] bytes = new byte[count];
        if (!Native.ReadProcessMemory(handle,(nint)address,bytes,(nuint)count,out var read) || read != (nuint)count)
            throw new Win32Exception(Marshal.GetLastWin32Error(), "No se pudo leer la memoria del juego.");
        return bytes;
    }
    public void Write(long address, byte[] bytes)
    {
        if (!Native.VirtualProtectEx(handle,(nint)address,(nuint)bytes.Length,0x40,out uint old))
            throw new Win32Exception(Marshal.GetLastWin32Error(),"No se pudo preparar la memoria.");
        Exception? failure = null;
        try
        {
            if (!Native.WriteProcessMemory(handle,(nint)address,bytes,(nuint)bytes.Length,out var written) || written != (nuint)bytes.Length)
                throw new Win32Exception(Marshal.GetLastWin32Error(),"No se pudo escribir la memoria.");
            if (!Native.FlushInstructionCache(handle,(nint)address,(nuint)bytes.Length))
                throw new Win32Exception(Marshal.GetLastWin32Error(),"No se pudo actualizar la caché de instrucciones.");
            if (!Read(address,bytes.Length).SequenceEqual(bytes)) throw new IOException("La comprobación de escritura falló.");
        }
        catch (Exception ex) { failure=ex; }
        finally
        {
            if (!Native.VirtualProtectEx(handle,(nint)address,(nuint)bytes.Length,old,out _))
                failure = new IOException("No se pudo restaurar la protección de memoria. Cierra el juego.",failure);
        }
        if (failure is not null) throw failure;
    }

    public IDisposable Freeze() => new FrozenThreads(process);
    public void Dispose() => Native.CloseHandle(handle);

    sealed class FrozenThreads : IDisposable
    {
        readonly List<nint> threads = [];
        public FrozenThreads(Process target)
        {
            if (target.Id == Environment.ProcessId) throw new InvalidOperationException("No se puede suspender el propio menú.");
            try
            {
                target.Refresh();
                foreach (ProcessThread thread in target.Threads)
                {
                    nint h=Native.OpenThread(0x0002, false, (uint)thread.Id);
                    if (h == 0) throw new Win32Exception(Marshal.GetLastWin32Error(),"No se pudieron pausar todos los hilos del juego.");
                    if (Native.SuspendThread(h) == uint.MaxValue)
                    {
                        Native.CloseHandle(h);
                        throw new Win32Exception(Marshal.GetLastWin32Error(),"No se pudo pausar un hilo del juego.");
                    }
                    threads.Add(h);
                }
                if (threads.Count == 0) throw new InvalidOperationException("El juego ya no está activo.");
            }
            catch { Dispose(); throw; }
        }
        public void Dispose()
        {
            foreach (nint thread in threads.AsEnumerable().Reverse())
            {
                Native.ResumeThread(thread);
                Native.CloseHandle(thread);
            }
            threads.Clear();
        }
    }
}

internal static class Native
{
    [DllImport("kernel32.dll",SetLastError=true)] internal static extern nint OpenProcess(uint access,bool inherit,int id);
    [DllImport("kernel32.dll",SetLastError=true)] internal static extern bool CloseHandle(nint handle);
    [DllImport("kernel32.dll",SetLastError=true)] internal static extern bool ReadProcessMemory(nint h,nint at,byte[] bytes,nuint count,out nuint read);
    [DllImport("kernel32.dll",SetLastError=true)] internal static extern bool WriteProcessMemory(nint h,nint at,byte[] bytes,nuint count,out nuint written);
    [DllImport("kernel32.dll",SetLastError=true)] internal static extern bool VirtualProtectEx(nint h,nint at,nuint size,uint protect,out uint old);
    [DllImport("kernel32.dll",SetLastError=true)] internal static extern bool FlushInstructionCache(nint h,nint at,nuint size);
    [DllImport("kernel32.dll",SetLastError=true)] internal static extern nint OpenThread(uint access,bool inherit,uint id);
    [DllImport("kernel32.dll",SetLastError=true)] internal static extern uint SuspendThread(nint h);
    [DllImport("kernel32.dll",SetLastError=true)] internal static extern uint ResumeThread(nint h);
    [DllImport("kernel32.dll",SetLastError=true)] internal static extern nint VirtualAlloc(nint address,nuint size,uint type,uint protect);
    [DllImport("kernel32.dll",SetLastError=true)] internal static extern bool VirtualProtect(nint address,nuint size,uint protect,out uint old);
    [DllImport("user32.dll",SetLastError=true)] internal static extern bool RegisterHotKey(nint h,int id,uint modifiers,uint key);
    [DllImport("user32.dll")] internal static extern bool UnregisterHotKey(nint h,int id);
    [DllImport("user32.dll")] internal static extern bool IsWindowVisible(nint h);
    [DllImport("user32.dll")] internal static extern bool ShowWindow(nint h,int command);
}
