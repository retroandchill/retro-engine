// // @file EngineHost.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using RetroEngine.Assets;
using RetroEngine.Core.Async;
using RetroEngine.Interop;
using RetroEngine.Platform;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Tickables;
using Serilog;
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
    private readonly TaskCompletionSource _gameThreadStarted = new();

    private static Engine? _instance;

    public static Engine Instance =>
        _instance ?? throw new InvalidOperationException("Engine has not been initialized.");

    public static bool IsInitialized => _instance is not null;

    internal Engine(IntPtr nativeEngine, IServiceProvider services)
    {
        _nativeEngine = nativeEngine;
        Services = services;
    }

    [MemberNotNull(nameof(_gameThread))]
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_instance is not null)
            throw new InvalidOperationException("The engine is already running.");
        Interlocked.Exchange(ref _instance, this);

        _ = CultureManager.Instance;
        AssetRegistry.RegisterDefaultAssetFactories();

        _gameThread = new Thread(RunGameThread);
        _gameThread.Start();

        cancellationToken.Register(() => _gameThreadStarted.TrySetCanceled());
        await _gameThreadStarted.Task.ConfigureAwait(false);
    }

    public async Task CreateMainWindowAsync(
        string title,
        int width,
        int height,
        WindowFlags flags,
        CancellationToken cancellationToken = default
    )
    {
        _windowId = await CreateWindowAsync(title, width, height, flags, cancellationToken);

        OnWindowRemoved += windowId =>
        {
            if (windowId != _windowId)
                return;

            _windowId = 0;
            RequestShutdown();
        };
    }

    public async Task<ulong> CreateWindowAsync(
        string title,
        int width,
        int height,
        WindowFlags flags,
        CancellationToken cancellationToken = default
    )
    {
        var tcs = new TaskCompletionSource<ulong>(cancellationToken);
        var tcsHandle = GCHandle.Alloc(tcs);
        try
        {
            unsafe
            {
                NativeCreateMainWindow(
                    _nativeEngine,
                    title,
                    title.Length,
                    width,
                    height,
                    flags,
                    GCHandle.ToIntPtr(tcsHandle),
                    &OnWindowCreated,
                    &OnWindowError
                );
            }

            return await tcs.Task;
        }
        finally
        {
            tcsHandle.Free();
        }
    }

    [UnmanagedCallersOnly]
    private static void OnWindowCreated(IntPtr userData, ulong windowId)
    {
        var tcs = (TaskCompletionSource<ulong>)GCHandle.FromIntPtr(userData).Target!;
        tcs.TrySetResult(windowId);
    }

    [UnmanagedCallersOnly]
    private static unsafe void OnWindowError(IntPtr userData, byte* errorMessage)
    {
        var tcs = (TaskCompletionSource<ulong>)GCHandle.FromIntPtr(userData).Target!;
        tcs.TrySetException(new PlatformNotSupportedException(Utf8StringMarshaller.ConvertToManaged(errorMessage)));
    }

    private record NativeCallbackPayload(
        Engine Engine,
        GameThreadSynchronizationContext SynchronizationContext,
        IGameSession? Session
    );

    private unsafe void RunGameThread()
    {
        using var synchronizationContext = new GameThreadSynchronizationContext();
        synchronizationContext.UnhandledException += ex => Log.Error(ex, "Unhandled exception in game thread.");
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
            payload.Engine._gameThreadStarted.SetResult();
            payload.Session?.Start();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception during session start");
            return -1;
        }
    }

    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        await InitializeAsync(cancellationToken);

        var exitCode = NativeRunPlatformEventLoop(_nativeEngine);

        _gameThread.Join();

        return exitCode;
    }

    public void PollPlatformEvents()
    {
        NativePollPlatformEvents(_nativeEngine);
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

    public void WaitForGameThread()
    {
        _gameThread?.Join();
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

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_engine_run_platform_event_loop")]
    private static partial int NativeRunPlatformEventLoop(IntPtr engine);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_engine_poll_platform_events")]
    private static partial void NativePollPlatformEvents(IntPtr engine);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_engine_create_main_window")]
    private static unsafe partial void NativeCreateMainWindow(
        IntPtr engine,
        ReadOnlySpan<char> title,
        int tileLength,
        int width,
        int height,
        WindowFlags flags,
        IntPtr userData,
        delegate* unmanaged<IntPtr, ulong, void> onWindowCreated,
        delegate* unmanaged<IntPtr, byte*, void> onError
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
