using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using HanumanInstitute.MvvmDialogs.Avalonia.MessageBox;
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
