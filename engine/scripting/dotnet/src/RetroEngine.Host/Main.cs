using System.Runtime.InteropServices;
using RetroEngine.Binds;
using RetroEngine.Core;

namespace RetroEngine.Host;

public static class Main
{
    [UnmanagedCallersOnly]
    public static unsafe NativeBool InitializeScriptEngine(
        char* workingDirectoryPath,
        int workingDirectoryPathLength,
        IntPtr bindsCallbacks
    )
    {
        try
        {
            AppDomain.CurrentDomain.SetData(
                "APP_CONTEXT_BASE_DIRECTORY",
                new ReadOnlySpan<char>(workingDirectoryPath, workingDirectoryPathLength).ToString()
            );

            Console.WriteLine("Script engine initialized successfully.");
            BindsManager.Initialize(bindsCallbacks);
            return NativeBool.True;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return NativeBool.False;
        }
    }
}
