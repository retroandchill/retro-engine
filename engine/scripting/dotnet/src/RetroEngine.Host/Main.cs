using System.Runtime.InteropServices;
using RetroEngine.Core;

namespace RetroEngine.Host;

public static class Main
{
    [UnmanagedCallersOnly]
    public static unsafe NativeBool InitializeScriptEngine(char* workingDirectoryPath, int workingDirectoryPathLength)
    {
        try
        {
            AppDomain.CurrentDomain.SetData("APP_CONTEXT_BASE_DIRECTORY",
                new ReadOnlySpan<char>(workingDirectoryPath, workingDirectoryPathLength).ToString());

            Console.WriteLine("Script engine initialized successfully.");
            return NativeBool.True;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return NativeBool.False;
        }
    }
}