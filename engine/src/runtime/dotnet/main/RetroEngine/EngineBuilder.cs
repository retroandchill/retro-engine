// // @file EngineHostBuilder.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RetroEngine.Interop;
using RetroEngine.Platform;
using RetroEngine.Portable.Interop;
using RetroEngine.Portable.Localization.Cultures;
using Serilog;

namespace RetroEngine;

public sealed partial class EngineBuilder
{
    private readonly List<Action<NativeConfigureContext>> _configureActions = [];

    public IConfigurationManager Configuration { get; } = new ConfigurationManager();

    private Func<IServiceCollection, IServiceProvider> _serviceProviderFactory = services =>
        services.BuildServiceProvider();
    public IServiceCollection Services { get; } = new ServiceCollection();

    public EngineBuilder()
    {
        ConfigureNative(ctx =>
        {
            NativeAddRenderingServices(ctx, WindowBackend.SDL3, RenderBackend.Vulkan);
        });

        Services.AddLogging(builder =>
        {
            builder.AddSerilog();
        });
    }

    [PublicAPI]
    public EngineBuilder ConfigureNative(Action<NativeConfigureContext> configure)
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

    public Engine Build()
    {
        _ = CultureManager.Instance;
        var serviceProvider = _serviceProviderFactory.Invoke(Services);

        var configureListHandle = GCHandle.Alloc(_configureActions);
        try
        {
            unsafe
            {
                Span<byte> errorMessage = stackalloc byte[256];
                var nativeEngine = CreateNativeEngine(
                    new PlatformBackendInfo(PlatformBackendKind.SDL3, PlatformInitFlags.Video),
                    &PerformNativeConfiguration,
                    GCHandle.ToIntPtr(configureListHandle),
                    errorMessage,
                    errorMessage.Length
                );
                if (nativeEngine == IntPtr.Zero)
                {
                    throw new ApplicationException($"Failed to create engine: {Encoding.UTF8.GetString(errorMessage)}");
                }

                try
                {
                    return new Engine(nativeEngine, serviceProvider);
                }
                catch
                {
                    Engine.NativeDestroy(nativeEngine);
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

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_create_engine")]
    private static unsafe partial IntPtr CreateNativeEngine(
        PlatformBackendInfo platformInfo,
        delegate* unmanaged<IntPtr, IntPtr, void> configureCallback,
        IntPtr userData,
        Span<byte> errorMessage,
        int errorMessageLength
    );

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_add_rendering_services")]
    private static partial void NativeAddRenderingServices(
        NativeConfigureContext ctx,
        WindowBackend windowBackend,
        RenderBackend renderBackend
    );
}
