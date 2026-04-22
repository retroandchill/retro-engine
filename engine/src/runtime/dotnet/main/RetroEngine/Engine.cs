// // @file EngineHost.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RetroEngine.Interop;
using RetroEngine.Platform;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Rendering;
using RetroEngine.Tickables;
using Serilog;
using Zomp.SyncMethodGenerator;

namespace RetroEngine;

public sealed partial class Engine : IDisposable, IAsyncDisposable
{
    private readonly PlatformBackend _platformBackend;
    private readonly IntPtr _nativeEngine;
    private bool _disposed;
    private Thread? _gameThread;
    private Thread? _renderThread;
    private readonly EngineLifetime _lifetime = new();
    private readonly EngineHost _host;

    public IServiceProvider Services => _host.Services;

    internal Engine(
        PlatformBackend platformBackend,
        IntPtr nativeEngine,
        IServiceCollection serviceCollection,
        Func<IServiceCollection, IServiceProvider> serviceProviderFactory
    )
    {
        _platformBackend = platformBackend;
        _nativeEngine = nativeEngine;
        serviceCollection.AddSingleton(_platformBackend);
        serviceCollection.AddSingleton<IHostApplicationLifetime>(_lifetime);
        serviceCollection.AddSingleton(serviceProvider =>
        {
            var b = serviceProvider.GetRequiredService<PlatformBackend>();
            return new RenderBackend(b, RenderBackendType.Vulkan);
        });
        _host = new EngineHost(this, serviceProviderFactory(serviceCollection), _lifetime);
    }

    [MemberNotNull(nameof(_gameThread))]
    public void Start()
    {
        _ = CultureManager.Instance;

        _gameThread = new Thread(RunGameThread) { Name = "Game Thread" };
        _renderThread = new Thread(RunRenderThread) { Name = "Render Thread" };

        _renderThread.Start();
        _gameThread.Start();

        unsafe
        {
            var handle = GCHandle.Alloc((Action?)RequestShutdown);
            NativeOnShutdownRequestedAdd(
                _nativeEngine,
                GCHandle.ToIntPtr(handle),
                &InvokeOnShutdownRequested,
                &DisposeDelegate,
                &EqualsDelegates
            );
        }
    }

    private void WaitUntilStopped()
    {
        _gameThread?.Join();
        _renderThread?.Join();
    }

    public void Run()
    {
        Start();

        while (!_lifetime.ApplicationStopped.IsCancellationRequested)
        {
            NativeWaitEvents(_nativeEngine);
        }

        WaitUntilStopped();
    }

    private void RunGameThread()
    {
        var tickManager = _host.Services.GetRequiredService<TickManager>();
        var renderManager = _host.Services.GetRequiredService<RenderManager>();
        try
        {
            using var threadSyncScope = tickManager.BindSynchronizationContext();
            LocalizationManager.Instance.ThreadSync = tickManager.ThreadSync;

            var stopWatch = new Stopwatch();

            _ = _host.StartAsync();
            while (!_lifetime.ApplicationStopped.IsCancellationRequested)
            {
                try
                {
                    var deltaTime = (float)stopWatch.Elapsed.TotalSeconds;
                    stopWatch.Restart();

                    tickManager.Tick(deltaTime);
                    renderManager.SyncRenderState();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Exception during game thread loop.");
                    break;
                }
            }
        }
        finally
        {
            renderManager.OnEngineShutdown();
        }
    }

    private void RunRenderThread()
    {
        var renderManager = _host.Services.GetRequiredService<RenderManager>();
        while (!_lifetime.ApplicationStopped.IsCancellationRequested)
        {
            renderManager.Render();
        }
    }

    public void PollPlatformEvents()
    {
        NativePollPlatformEvents(_nativeEngine);
    }

    public void RequestShutdown()
    {
        _lifetime.StopApplication();
    }

    [UnmanagedCallersOnly]
    private static void InvokeOnShutdownRequested(IntPtr userData)
    {
        var handle = GCHandle.FromIntPtr(userData);
        var action = (Action?)handle.Target;
        action?.Invoke();
    }

    [UnmanagedCallersOnly]
    private static void InvokeWindowRemoved(IntPtr userData, ulong windowId)
    {
        var handle = GCHandle.FromIntPtr(userData);
        var action = (Action<ulong>?)handle.Target;
        action?.Invoke(windowId);
    }

    [UnmanagedCallersOnly]
    private static void DisposeDelegate(IntPtr userData)
    {
        var handle = GCHandle.FromIntPtr(userData);
        handle.Free();
    }

    [UnmanagedCallersOnly]
    private static byte EqualsDelegates(IntPtr lhs, IntPtr rhs)
    {
        var lhsHandle = GCHandle.FromIntPtr(lhs);
        var rhsHandle = GCHandle.FromIntPtr(rhs);
        return Equals(lhsHandle.Target, rhsHandle.Target) ? (byte)1 : (byte)0;
    }

    public void WaitForGameThread()
    {
        _gameThread?.Join();
    }

    [CreateSyncVersion]
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;
        _platformBackend.Dispose();
        await _host.DisposeAsync();

        CultureManager.Instance.Dispose();
        NativeDestroy(_nativeEngine);
    }

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_destroy_engine")]
    internal static partial void NativeDestroy(IntPtr engine);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_engine_wait_platform_events")]
    private static partial void NativeWaitEvents(IntPtr engine, long timeout = 10);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_engine_poll_platform_events")]
    private static partial void NativePollPlatformEvents(IntPtr engine);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_engine_on_shutdown_requested_add")]
    private static unsafe partial void NativeOnShutdownRequestedAdd(
        IntPtr engine,
        IntPtr callback,
        delegate* unmanaged<IntPtr, void> invoke,
        delegate* unmanaged<IntPtr, void> dispose,
        delegate* unmanaged<IntPtr, IntPtr, byte> equals
    );
}
