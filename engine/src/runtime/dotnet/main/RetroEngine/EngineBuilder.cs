// // @file EngineHostBuilder.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RetroEngine.Config;
using RetroEngine.Interop;
using RetroEngine.Platform;
using RetroEngine.Portable.Localization.Cultures;
using Serilog;

namespace RetroEngine;

public sealed partial class EngineBuilder : IHostApplicationBuilder
{
    public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();
    public IConfigurationManager Configuration { get; }
    public IHostEnvironment Environment { get; } = new HostEnvironment();
    public ILoggingBuilder Logging { get; }
    public IMetricsBuilder Metrics { get; }

    private Func<IServiceCollection, IServiceProvider> _serviceProviderFactory = services =>
        services.BuildServiceProvider();
    public IServiceCollection Services { get; } = new ServiceCollection();

    public EngineBuilder()
    {
        Logging = new LoggingBuilder(Services);
        Metrics = new MetricsBuilder(Services);

        var configurationManager = new ConfigurationManager();
        Services.AddSingleton<IConfiguration>(configurationManager);
        Services.AddSingleton(configurationManager);
        Services.AddSingleton<IConfigurationRoot>(configurationManager);
        Configuration = configurationManager;

        Services.Configure<RenderingSettings>(Configuration.GetSection("Rendering"));

        Services.AddLogging(builder => builder.AddSerilog()).AddRetroEngine();
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

    public Engine Build()
    {
        _ = CultureManager.Instance;

        var platformBackend = new PlatformBackend(PlatformBackendKind.SDL3, PlatformInitFlags.Video);
        try
        {
            var nativeEngine = CreateNativeEngine(platformBackend);

            try
            {
                return new Engine(platformBackend, nativeEngine, Services, _serviceProviderFactory);
            }
            catch
            {
                Engine.NativeDestroy(nativeEngine);
                throw;
            }
        }
        catch
        {
            platformBackend.Dispose();
            throw;
        }
    }

    private sealed class HostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = AppDomain.CurrentDomain.FriendlyName;
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; }

        public HostEnvironment()
        {
            ContentRootFileProvider = new PhysicalFileProvider(ContentRootPath);
        }
    }

    private sealed class LoggingBuilder(IServiceCollection services) : ILoggingBuilder
    {
        public IServiceCollection Services { get; } = services;
    }

    private sealed class MetricsBuilder(IServiceCollection services) : IMetricsBuilder
    {
        public IServiceCollection Services { get; } = services;
    }

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_create_engine")]
    private static unsafe partial IntPtr CreateNativeEngine(PlatformBackend platformBackend);
}
