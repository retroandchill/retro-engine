using System;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using RetroEngine.Logging;
using Serilog;

namespace RetroEngine.Editor;

internal static class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
    {
        Log.Logger = new LoggerConfiguration().WithEngineLog().CreateLogger();

        var engineBuilder = new EngineBuilder();
        engineBuilder
            .Services.Configure<JsonSerializerOptions>(options =>
            {
                options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.WriteIndented = true;
            })
            .AddRetroEngineEditorCore()
            .AddRetroEngineEditor();

        return AppBuilder
            .Configure(() => new App(engineBuilder.Build()))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}
