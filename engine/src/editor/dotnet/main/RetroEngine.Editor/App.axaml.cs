using System.IO.Abstractions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Injectio.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RetroEngine.Editor.Core.Data;
using RetroEngine.Editor.Core.Extensions;
using RetroEngine.Editor.Core.ViewModels;
using RetroEngine.Editor.Core.Views;

namespace RetroEngine.Editor;

public sealed class App : Application, IDisposable
{
    private Engine? _engine;
    private MainWindowViewModel? _mainWindow;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (!Design.IsDesignMode)
        {
            var engineBuilder = new EngineBuilder();
            engineBuilder.Services.AddRetroEngineEditorCore().AddRetroEngineEditor();

            _engine = engineBuilder.Build();
            DesignResolve.ServiceProvider = _engine.Services;
            _mainWindow = _engine.Services.GetRequiredService<MainWindowViewModel>();

            var contextFactory = _engine.Services.GetRequiredService<IDbContextFactory<CachedDbContext>>();
            using (var context = contextFactory.CreateDbContext())
            {
                context.Database.Migrate();
            }

            _engine.Start();
        }
        else
        {
            DesignResolve.ServiceProvider = CreateDesignTimeServices();
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.Exit += OnExit;

            desktop.MainWindow = new MainWindow { DataContext = _mainWindow };
        }

        _mainWindow?.ShowProjectOpen();

        base.OnFrameworkInitializationCompleted();
    }

    private static ServiceProvider CreateDesignTimeServices()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection
            .AddLogging()
            .AddSingleton<IFileSystem, FileSystem>()
            .AddRetroEngineEditorCore()
            .AddRetroEngineEditor();
        return serviceCollection.BuildServiceProvider();
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        if (_engine is null)
            return;

        _engine.RequestShutdown();
        _engine.WaitForGameThread();
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

    public void Dispose()
    {
        _engine?.Dispose();
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
