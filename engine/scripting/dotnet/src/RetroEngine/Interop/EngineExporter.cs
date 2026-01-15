using System.Runtime.InteropServices;

namespace RetroEngine.Interop;

internal static partial class EngineExporter
{
    private const string LibraryName = "retro_runtime";

    [LibraryImport(LibraryName, EntryPoint = "retro_engine_request_shutdown")]
    public static partial void RequestShutdown(int exitCode);
}
