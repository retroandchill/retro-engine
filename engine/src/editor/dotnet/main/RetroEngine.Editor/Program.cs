using System;
using Avalonia;
using RetroEngine.Logging;
using Serilog;

namespace RetroEngine.Editor;

internal static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration().WithEngineLog().CreateLogger();

        var engineBuilder = new EngineBuilder();
        using var engine = engineBuilder.Build();
        _ = engine.InitializeAsync();

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            engine.RequestShutdown();
            engine.WaitForGameThread();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>().UsePlatformDetect().WithInterFont().LogToTrace();
    }
}
