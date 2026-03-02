using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
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
        BuildAvaloniaApp(engineBuilder).StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp(EngineBuilder engineBuilder)
    {
        return AppBuilder
            .Configure(() => new App(engineBuilder.Build()))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}
