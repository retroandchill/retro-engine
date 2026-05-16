using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Injectio.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetroEngine.Editor.Core.Data;
using RetroEngine.Editor.Core.Services;
using RetroEngine.Editor.Views;

namespace RetroEngine.Editor;

public class App(Engine engine) : Application
{
    private readonly INavigationService _navigationService = engine.Services.GetRequiredService<INavigationService>();
    private DispatcherTimer? _sdlEventPumpTimer;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var contextFactory = engine.Services.GetRequiredService<IDbContextFactory<CachedDbContext>>();
        using (var context = contextFactory.CreateDbContext())
        {
            context.Database.Migrate();
        }

        engine.Start();

        _sdlEventPumpTimer = new DispatcherTimer(
            TimeSpan.FromMilliseconds(1),
            DispatcherPriority.Input,
            (_, _) =>
            {
                engine.PollPlatformEvents();
            }
        );

        _sdlEventPumpTimer.Start();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Exit += OnExit;

            desktop.MainWindow = new MainWindow { DataContext = _navigationService.MainWindow };

            desktop.MainWindow.Closing += (_, _) =>
            {
                if (_navigationService.MainWindow.Content is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            };
        }

        _navigationService.ShowProjectOpen();

        base.OnFrameworkInitializationCompleted();
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        _sdlEventPumpTimer?.Stop();
        _sdlEventPumpTimer = null;

        engine.RequestShutdown();
        engine.WaitForGameThread();
        engine.DisposeAsync().AsTask().Wait(TimeSpan.FromSeconds(10));
    }

    [RegisterServices]
    internal static void RegisterDialogService(IServiceCollection services)
    {
        services
            .AddSingleton<IDialogManager, DialogManager>()
            .AddSingleton<IDialogFactory>(_ => new DialogFactory().AddMessageBox())
            .AddSingleton(
                IDialogService (provider) =>
                {
                    var viewModelProvider = provider.GetRequiredService<ViewModelProvider>();
                    return new DialogService(
                        provider.GetRequiredService<IDialogManager>(),
                        viewModelFactory: viewModelProvider.CreateViewModel
                    );
                }
            );
    }
}
