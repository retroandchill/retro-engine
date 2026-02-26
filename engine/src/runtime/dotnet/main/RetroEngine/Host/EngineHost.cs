// // @file EngineHost.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RetroEngine.Core.Async;
using RetroEngine.Logging;
using RetroEngine.Platform;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Tickables;
using ZLinq;

namespace RetroEngine.Host;

public sealed partial class EngineHost : IHost, IAsyncDisposable
{
    public IServiceProvider Services { get; }
    private readonly IntPtr _nativeEngine;
    private readonly EngineHostLifetime _lifetime;
    private bool _disposed;
    private Thread? _gameThread;
    private ulong _windowId;
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

        _gameThread = new Thread(RunGameThread);
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

    private record NativeCallbackPayload(
        EngineHost Engine,
        GameThreadSynchronizationContext SynchronizationContext,
        IGameSession? Session
    );

    private unsafe void RunGameThread()
    {
        using var synchronizationContext = new GameThreadSynchronizationContext();
        synchronizationContext.UnhandledException += ex => Logger.Error(ex.ToString());
        SynchronizationContext.SetSynchronizationContext(synchronizationContext);

        LocalizationManager.Instance.ThreadSync = synchronizationContext;

        var session = Services.GetService<IGameSession>();

        var payload = new NativeCallbackPayload(this, synchronizationContext, session);
        var handle = GCHandle.Alloc(payload);
        try
        {
            NativeRun(_nativeEngine, GCHandle.ToIntPtr(handle), &StartCallback, &Tick, &StopCallback);
        }
        finally
        {
            handle.Free();
        }

        SynchronizationContext.SetSynchronizationContext(null);
    }

    [UnmanagedCallersOnly]
    private static int StartCallback(IntPtr userData)
    {
        var payload = (NativeCallbackPayload)GCHandle.FromIntPtr(userData).Target!;
        try
        {
            payload.Session?.Start();
            return 0;
        }
        catch (Exception ex)
        {
            Logger.Error(ex.ToString());
            return -1;
        }
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        await StartAsync(cancellationToken);

        NativePollPlatformEvents(_nativeEngine);

        _gameThread.Join();

        await StopAsync(cancellationToken);
    }

    [UnmanagedCallersOnly]
    private static void StopCallback(IntPtr userData)
    {
        var payload = (NativeCallbackPayload)GCHandle.FromIntPtr(userData).Target!;
        payload.Session?.Terminate();
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_instance is null)
            throw new InvalidOperationException("The engine is not running.");

        _lifetime.NotifyStopped();
        Interlocked.Exchange(ref _instance, null);
        return Task.CompletedTask;
    }

    [UnmanagedCallersOnly]
    private static void Tick(IntPtr userData, float deltaTime)
    {
        var payload = (NativeCallbackPayload)GCHandle.FromIntPtr(userData).Target!;
        payload.Engine.Tick(deltaTime, payload.SynchronizationContext);
    }

    private void Tick(float deltaTime, GameThreadSynchronizationContext synchronizationContext)
    {
        foreach (var tickable in _tickables.AsValueEnumerable().Where(t => t.TickEnabled))
        {
            tickable.Tick(deltaTime);
        }
        synchronizationContext.Pump();
        FrameCount++;
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

        CultureManager.Instance.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    [LibraryImport("retro_runtime", EntryPoint = "retro_destroy_engine")]
    internal static partial void NativeDestroy(IntPtr engine);

    [LibraryImport("retro_runtime", EntryPoint = "retro_engine_run")]
    private static unsafe partial void NativeRun(
        IntPtr engine,
        IntPtr userData,
        delegate* unmanaged<IntPtr, int> startCallback,
        delegate* unmanaged<IntPtr, float, void> updateCallback,
        delegate* unmanaged<IntPtr, void> stopCallback
    );

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
