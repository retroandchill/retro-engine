// // @file EngineHost.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Hosting;
using RetroEngine.Core.Async;
using RetroEngine.Logging;
using RetroEngine.Platform;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Tickables;

namespace RetroEngine.Host;

public sealed partial class EngineHost : IHost, IAsyncDisposable
{
    public IServiceProvider Services { get; }
    private readonly IntPtr _nativeEngine;
    private readonly EngineHostLifetime _lifetime;
    private bool _disposed;
    private Thread? _gameThread;
    private ulong _windowId;
    private readonly GameThreadSynchronizationContext _synchronizationContext = new();
    private readonly HashSet<ITickable> _tickables = [];
    public ulong FrameCount { get; private set; }

    private static EngineHost? _instance;

    public static EngineHost Instance =>
        _instance ?? throw new InvalidOperationException("Engine has not been initialized.");

    internal EngineHost(IntPtr nativeEngine, IServiceProvider services, EngineHostLifetime lifetime)
    {
        _nativeEngine = nativeEngine;
        Services = services;
        _lifetime = lifetime;
    }

    [MemberNotNull(nameof(_gameThread))]
    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_instance is not null)
            throw new InvalidOperationException("The engine is already running.");
        Interlocked.Exchange(ref _instance, this);

        _ = CultureManager.Instance;

        _gameThread = new Thread(() =>
        {
            _synchronizationContext.UnhandledException += ex => Logger.Error(ex.ToString());
            SynchronizationContext.SetSynchronizationContext(_synchronizationContext);

            LocalizationManager.Instance.ThreadSync = _synchronizationContext;

            NativeRun(_nativeEngine);

            SynchronizationContext.SetSynchronizationContext(null);
        });
        _gameThread.Start();

        _windowId = NativeCreateMainWindow(
            _nativeEngine,
            "Retro Engine",
            1280,
            720,
            WindowFlags.Resizable | WindowFlags.Vulkan
        );

        _lifetime.NotifyStarted();
        return Task.CompletedTask;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        await StartAsync(cancellationToken);

        NativePollPlatformEvents(_nativeEngine);

        _gameThread.Join();

        await StopAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_instance is null)
            throw new InvalidOperationException("The engine is not running.");

        _lifetime.NotifyStopped();
        Interlocked.Exchange(ref _instance, null);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        NativeDestroy(_nativeEngine);
        if (Services is IDisposable disposable)
        {
            disposable.Dispose();
        }
        _synchronizationContext.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    [LibraryImport("retro_runtime", EntryPoint = "retro_destroy_engine")]
    internal static partial void NativeDestroy(IntPtr engine);

    [LibraryImport("retro_runtime", EntryPoint = "retro_engine_run")]
    private static partial void NativeRun(IntPtr engine);

    [LibraryImport("retro_runtime", EntryPoint = "retro_engine_poll_platform_events")]
    private static partial void NativePollPlatformEvents(IntPtr engine);

    [LibraryImport(
        "retro_runtime",
        EntryPoint = "retro_engine_create_main_window",
        StringMarshalling = StringMarshalling.Utf8
    )]
    private static partial ulong NativeCreateMainWindow(
        IntPtr engine,
        string title,
        int width,
        int height,
        WindowFlags flags
    );
}
