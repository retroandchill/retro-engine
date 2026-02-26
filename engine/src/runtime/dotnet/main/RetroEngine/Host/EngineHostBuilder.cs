// // @file EngineHostBuilder.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RetroEngine.Interop;
using RetroEngine.Platform;
using RetroEngine.Portable.Localization.Cultures;

namespace RetroEngine.Host;

public sealed partial class EngineHostBuilder : IHostApplicationBuilder
{
    private readonly List<Action<NativeConfigureContext>> _configureActions = [];

    public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();
    public IConfigurationManager Configuration { get; } = new ConfigurationManager();
    public IHostEnvironment Environment { get; } = new HostEnvironment();

    private readonly LoggingBuilder _loggingBuilder;
    private readonly MetricsBuilder _metricsBuilder;

    ILoggingBuilder IHostApplicationBuilder.Logging => _loggingBuilder;
    IMetricsBuilder IHostApplicationBuilder.Metrics => _metricsBuilder;

    private Func<IServiceCollection, IServiceProvider> _serviceProviderFactory = services =>
        services.BuildServiceProvider();
    public IServiceCollection Services { get; } = new ServiceCollection();

    private readonly EngineHostLifetime _engineHostLifetime = new();

    public EngineHostBuilder()
    {
        Services.AddSingleton<IHostApplicationLifetime>(_engineHostLifetime);
        _loggingBuilder = new LoggingBuilder(Services);
        _metricsBuilder = new MetricsBuilder(Services);
        ConfigureNative(ctx =>
        {
            NativeAddRenderingServices(ctx, WindowBackend.SDL3, RenderBackend.Vulkan);
        });
    }

    [PublicAPI]
    public EngineHostBuilder ConfigureNative(Action<NativeConfigureContext> configure)
    {
        _configureActions.Add(configure);
        return this;
    }

    public void ConfigureContainer<TContainerBuilder>(
        IServiceProviderFactory<TContainerBuilder> factory,
        Action<TContainerBuilder>? configure = null
    )
        where TContainerBuilder : notnull
    {
        _serviceProviderFactory = services =>
        {
            var builder = factory.CreateBuilder(services);
            configure?.Invoke(builder);
            return factory.CreateServiceProvider(builder);
        };
    }

    public EngineHost Build()
    {
        _ = CultureManager.Instance;
        var serviceProvider = _serviceProviderFactory.Invoke(Services);

        var configureListHandle = GCHandle.Alloc(_configureActions);
        try
        {
            unsafe
            {
                var nativeEngine = CreateNativeEngine(
                    new PlatformBackendInfo(PlatformBackendKind.SDL3, PlatformInitFlags.Video),
                    &PerformNativeConfiguration,
                    GCHandle.ToIntPtr(configureListHandle)
                );
                try
                {
                    return new EngineHost(nativeEngine, serviceProvider, _engineHostLifetime);
                }
                catch
                {
                    EngineHost.NativeDestroy(nativeEngine);
                    throw;
                }
            }
        }
        finally
        {
            configureListHandle.Free();
        }
    }

    [UnmanagedCallersOnly]
    private static void PerformNativeConfiguration(IntPtr ptr, IntPtr userData)
    {
        var userDataHandle = GCHandle.FromIntPtr(userData);
        var configurers = (List<Action<NativeConfigureContext>>)userDataHandle.Target!;
        var nativeContext = new NativeConfigureContext(ptr);
        foreach (var configurer in configurers)
        {
            configurer(nativeContext);
        }
    }

    private sealed class HostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;
        public string ApplicationName { get; set; } = "RetroEngine";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new PhysicalFileProvider(AppContext.BaseDirectory);
    }

    private sealed class LoggingBuilder(IServiceCollection services) : ILoggingBuilder
    {
        public IServiceCollection Services { get; } = services;
    }

    private sealed class MetricsBuilder(IServiceCollection services) : IMetricsBuilder
    {
        public IServiceCollection Services { get; } = services;
    }

    [LibraryImport("retro_runtime", EntryPoint = "retro_create_engine")]
    private static unsafe partial IntPtr CreateNativeEngine(
        PlatformBackendInfo platformInfo,
        delegate* unmanaged<IntPtr, IntPtr, void> configureCallback,
        IntPtr userData
    );

    [LibraryImport("retro_renderer", EntryPoint = "retro_add_rendering_services")]
    private static partial void NativeAddRenderingServices(
        NativeConfigureContext ctx,
        WindowBackend windowBackend,
        RenderBackend renderBackend
    );
}
