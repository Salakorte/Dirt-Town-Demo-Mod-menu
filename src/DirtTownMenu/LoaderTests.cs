using System.Runtime.InteropServices;
namespace DirtTownMenu;

internal static class LoaderTests
{
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]delegate uint Timer();
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]delegate int PreviewTest();
    [UnmanagedFunctionPointer(CallingConvention.Winapi)]delegate uint Period(uint value);
    [UnmanagedFunctionPointer(CallingConvention.Winapi,CharSet=CharSet.Unicode)]delegate int Verify([MarshalAs(UnmanagedType.LPWStr)]string filename);
    public static int Run()
    {
        nint proxy=NativeLibrary.Load(Path.Combine(AppContext.BaseDirectory,"winmm.dll"));
        var getTime=Marshal.GetDelegateForFunctionPointer<Timer>(NativeLibrary.GetExport(proxy,"timeGetTime"));
        var begin=Marshal.GetDelegateForFunctionPointer<Period>(NativeLibrary.GetExport(proxy,"timeBeginPeriod"));
        var end=Marshal.GetDelegateForFunctionPointer<Period>(NativeLibrary.GetExport(proxy,"timeEndPeriod"));
        uint first=getTime();Thread.Sleep(30);uint second=getTime();
        if(second-first<10||second-first>10000)throw new Exception("Forwarded timer did not advance");
        if(begin(1)!=0)throw new Exception("Forwarded timeBeginPeriod failed");
        if(end(1)!=0)throw new Exception("Forwarded timeEndPeriod failed");
        var verify=Marshal.GetDelegateForFunctionPointer<Verify>(NativeLibrary.GetExport(proxy,"DirtTownMenuVerifyBuild"));
        var preview=Marshal.GetDelegateForFunctionPointer<PreviewTest>(NativeLibrary.GetExport(proxy,"DirtTownMenuPreviewSelfTest"));
        int previewResult=preview();if(previewResult!=0)throw new Exception("Preview clipping failed case "+previewResult);
        var camera=Marshal.GetDelegateForFunctionPointer<PreviewTest>(NativeLibrary.GetExport(proxy,"DirtTownMenuCameraSelfTest"));
        int cameraResult=camera();if(cameraResult!=0)throw new Exception("Free camera movement failed case "+cameraResult);
        if(verify(BuildProfile.DefaultGame)!=1)throw new Exception("Native standalone build verification failed");
        if(verify(Environment.ProcessPath!)!=0)throw new Exception("Native standalone validation accepted another executable");
        Console.WriteLine("PASS: WinMM forwarding loads the real system library; timer advances and period calls succeed.");
        Console.WriteLine("PASS: Native autoloader SHA256 accepts the installed game build and rejects another executable.");
        Console.WriteLine("PASS: Preview clips offscreen and extreme coordinates, rejects NaN/infinity and stays within the viewport.");
        Console.WriteLine("PASS: Free camera basis, diagonal speed, delta-time limit, invalid input, world bounds and pitch limit.");
        // Keep the module mapped until process exit: its startup worker is asynchronous.
        return 0;
    }
}
