using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Injectio.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetroEngine.Editor.Core.Data;
using RetroEngine.Editor.Core.ViewModels;
using RetroEngine.Editor.Views;

namespace RetroEngine.Editor;

public class App(Engine engine) : Application
{
    private readonly IMainWindowViewModel _mainWindowViewModel =
        engine.Services.GetRequiredService<IMainWindowViewModel>();

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

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.Exit += OnExit;

            desktop.MainWindow = new MainWindow { DataContext = _mainWindowViewModel };
        }

        _mainWindowViewModel.ShowProjectOpen();

        base.OnFrameworkInitializationCompleted();
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        engine.RequestShutdown();
        engine.WaitForGameThread();
        engine.Dispose();
    }

    private static void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove = BindingPlugins
            .DataValidators.OfType<DataAnnotationsValidationPlugin>()
            .ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    [RegisterServices]
    internal static void RegisterDialogService(IServiceCollection services)
    {
        services
            .AddSingleton<IDialogManager, DialogManager>()
            .AddSingleton<IDialogFactory, DialogFactory>(_ => new DialogFactory())
            .AddSingleton<IDialogService, DialogService>()
            .AddSingleton(
                IDialogService (provider) =>
                    new DialogService(
                        provider.GetRequiredService<IDialogManager>(),
                        viewModelFactory: provider.GetRequiredService
                    )
            );
    }
}
