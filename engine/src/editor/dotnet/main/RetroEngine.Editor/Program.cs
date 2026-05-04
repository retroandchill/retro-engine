using System.Text.Json;
using Avalonia;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RetroEngine.Config;
using RetroEngine.Logging;
using Serilog;
using Serilog.Events;

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
        Log.Logger = new LoggerConfiguration()
            .WithEngineLog()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
            .CreateLogger();

        var engineBuilder = new EngineBuilder();

        const string autoAssignSettingKey = $"Rendering:{nameof(RenderingSettings.AutoAssignViewports)}";
        engineBuilder.Configuration.AddInMemoryCollection(
            new Dictionary<string, string?> { [autoAssignSettingKey] = "false" }
        );

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
