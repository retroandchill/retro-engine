using System.Runtime.InteropServices;

namespace RetroEngine;

public static partial class Engine
{
    public static void RequestShutdown(int exitCode = 0)
    {
        NativeRequestShutdown(exitCode);
    }

    private const string LibraryName = "retro_runtime";

    [LibraryImport(LibraryName, EntryPoint = "retro_engine_request_shutdown")]
    private static partial void NativeRequestShutdown(int exitCode);
}
