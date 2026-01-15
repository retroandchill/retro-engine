using RetroEngine.Interop;

namespace RetroEngine;

public static class Engine
{
    private const string RetroRuntime = "retro_runtime";

    public static void RequestShutdown(int exitCode = 0)
    {
        EngineExporter.RequestShutdown(exitCode);
    }
}
