// // @file EngineHost.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using RetroEngine.Assets;
using RetroEngine.Core.Async;
using RetroEngine.Logging;
using RetroEngine.Platform;
using RetroEngine.Portable.Interop;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Tickables;
using ZLinq;

namespace RetroEngine;

public sealed partial class Engine : IDisposable, IAsyncDisposable
{
    public IServiceProvider Services { get; }
    private readonly IntPtr _nativeEngine;
    private bool _disposed;
    private Thread? _gameThread;
    private ulong _windowId;
    private readonly HashSet<ITickable> _tickables = [];
    public ulong FrameCount { get; private set; }

    private static Engine? _instance;

    public static Engine Instance =>
        _instance ?? throw new InvalidOperationException("Engine has not been initialized.");

    internal Engine(IntPtr nativeEngine, IServiceProvider services)
    {
        _nativeEngine = nativeEngine;
        Services = services;
    }

    [MemberNotNull(nameof(_gameThread))]
    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_instance is not null)
            throw new InvalidOperationException("The engine is already running.");
        Interlocked.Exchange(ref _instance, this);

        _ = CultureManager.Instance;
        AssetRegistry.RegisterDefaultAssetFactories();

        _gameThread = new Thread(RunGameThread);
        _gameThread.Start();

        _windowId = CreateMainWindow("Retro Engine", 1280, 720, WindowFlags.Resizable | WindowFlags.Vulkan);

        OnWindowRemoved += windowId =>
        {
            if (windowId == _windowId)
            {
                RequestShutdown();
            }
        };

        return Task.CompletedTask;
    }

    private ulong CreateMainWindow(string title, int width, int height, WindowFlags flags)
    {
        Span<byte> errorMessage = stackalloc byte[256];
        var windowId = NativeCreateMainWindow(
            _nativeEngine,
            title,
            width,
            height,
            flags,
            errorMessage,
            errorMessage.Length
        );
        return windowId != 0
            ? windowId
            : throw new PlatformNotSupportedException(Encoding.UTF8.GetString(errorMessage));
    }

    private record NativeCallbackPayload(
        Engine Engine,
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

    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);

        var exitCode = NativePollPlatformEvents(_nativeEngine);

        _gameThread.Join();

        return exitCode;
    }

    [UnmanagedCallersOnly]
    private static void StopCallback(IntPtr userData)
    {
        var payload = (NativeCallbackPayload)GCHandle.FromIntPtr(userData).Target!;
        payload.Session?.Terminate();
    }

    public void RegisterTickable(ITickable tickable)
    {
        _tickables.Add(tickable);
    }

    public void UnregisterTickable(ITickable tickable)
    {
        _tickables.Remove(tickable);
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

    public void RequestShutdown(int exitCode = 0)
    {
        NativeRequestShutdown(_nativeEngine, exitCode);
    }

    public event Action<ulong>? OnWindowRemoved
    {
        add
        {
            unsafe
            {
                var handle = GCHandle.Alloc(value);
                NativeOnWindowRemovedAdd(
                    _nativeEngine,
                    GCHandle.ToIntPtr(handle),
                    &InvokeWindowRemoved,
                    &DisposeWindowRemoved,
                    &EqualsWindowRemoved
                );
            }
        }
        remove
        {
            unsafe
            {
                var handle = GCHandle.Alloc(value);
                NativeOnWindowRemovedRemove(
                    _nativeEngine,
                    GCHandle.ToIntPtr(handle),
                    &InvokeWindowRemoved,
                    &DisposeWindowRemoved,
                    &EqualsWindowRemoved
                );
            }
        }
    }

    [UnmanagedCallersOnly]
    private static void InvokeWindowRemoved(IntPtr userData, ulong windowId)
    {
        var handle = GCHandle.FromIntPtr(userData);
        var action = (Action<ulong>?)handle.Target;
        action?.Invoke(windowId);
    }

    [UnmanagedCallersOnly]
    private static void DisposeWindowRemoved(IntPtr userData)
    {
        var handle = GCHandle.FromIntPtr(userData);
        handle.Free();
    }

    [UnmanagedCallersOnly]
    private static byte EqualsWindowRemoved(IntPtr lhs, IntPtr rhs)
    {
        var lhsHandle = GCHandle.FromIntPtr(lhs);
        var rhsHandle = GCHandle.FromIntPtr(rhs);
        return Equals(lhsHandle.Target, rhsHandle.Target) ? (byte)1 : (byte)0;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        if (ReferenceEquals(this, _instance))
        {
            Interlocked.Exchange(ref _instance, null);
        }

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

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_destroy_engine")]
    internal static partial void NativeDestroy(IntPtr engine);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_engine_run")]
    private static unsafe partial void NativeRun(
        IntPtr engine,
        IntPtr userData,
        delegate* unmanaged<IntPtr, int> startCallback,
        delegate* unmanaged<IntPtr, float, void> updateCallback,
        delegate* unmanaged<IntPtr, void> stopCallback
    );

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_engine_poll_platform_events")]
    private static partial int NativePollPlatformEvents(IntPtr engine);

    [LibraryImport(
        NativeLibraries.RetroEngine,
        EntryPoint = "retro_engine_create_main_window",
        StringMarshalling = StringMarshalling.Utf8
    )]
    private static partial ulong NativeCreateMainWindow(
        IntPtr engine,
        string title,
        int width,
        int height,
        WindowFlags flags,
        Span<byte> errorMessage,
        int errorMessageLength
    );

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_engine_request_shutdown")]
    private static partial void NativeRequestShutdown(IntPtr engine, int exitCode);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_engine_on_window_removed_add")]
    private static unsafe partial void NativeOnWindowRemovedAdd(
        IntPtr engine,
        IntPtr callback,
        delegate* unmanaged<IntPtr, ulong, void> invoke,
        delegate* unmanaged<IntPtr, void> dispose,
        delegate* unmanaged<IntPtr, IntPtr, byte> equals
    );

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_engine_on_window_removed_remove")]
    private static unsafe partial void NativeOnWindowRemovedRemove(
        IntPtr engine,
        IntPtr callback,
        delegate* unmanaged<IntPtr, ulong, void> invoke,
        delegate* unmanaged<IntPtr, void> dispose,
        delegate* unmanaged<IntPtr, IntPtr, byte> equals
    );
}
