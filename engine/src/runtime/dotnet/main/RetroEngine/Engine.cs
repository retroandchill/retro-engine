// // @file EngineHost.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RetroEngine.Assets;
using RetroEngine.Async;
using RetroEngine.Interop;
using RetroEngine.Platform;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Tickables;
using Serilog;
using ZLinq;
using Zomp.SyncMethodGenerator;

namespace RetroEngine;

public sealed partial class Engine : IDisposable, IAsyncDisposable
{
    private readonly IntPtr _nativeEngine;
    private bool _disposed;
    private Thread? _gameThread;
    private Thread? _renderThread;
    private ulong _windowId;
    private readonly EngineLifetime _lifetime = new();
    private readonly EngineHost _host;

    public IServiceProvider Services => _host.Services;

    private static Engine? _instance;

    public static Engine Instance =>
        _instance ?? throw new InvalidOperationException("Engine has not been initialized.");

    internal Engine(
        IntPtr nativeEngine,
        IServiceCollection serviceCollection,
        Func<IServiceCollection, IServiceProvider> serviceProviderFactory
    )
    {
        _nativeEngine = nativeEngine;
        serviceCollection.AddSingleton<IHostApplicationLifetime>(_lifetime);
        _host = new EngineHost(this, serviceProviderFactory(serviceCollection), _lifetime);
    }

    [MemberNotNull(nameof(_gameThread))]
    public void Start()
    {
        if (_instance is not null)
            throw new InvalidOperationException("The engine is already running.");
        Interlocked.Exchange(ref _instance, this);

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

    private void RunGameThread()
    {
        var tickManager = _host.Services.GetRequiredService<TickManager>();
        tickManager.BindSynchronizationContext();
        LocalizationManager.Instance.ThreadSync = tickManager.ThreadSync;

        var stopWatch = new Stopwatch();

        _ = _host.StartAsync();
        while (!_lifetime.ApplicationStopped.IsCancellationRequested)
        {
            try
            {
                var deltaTime = (float)stopWatch.Elapsed.TotalSeconds;
                stopWatch.Restart();

                _ = NativePumpTasks(_nativeEngine, -1, out var errorMessage);
                errorMessage.ThrowIfError();
                tickManager.Tick(deltaTime);

                _ = NativeSyncRenderState(_nativeEngine, out errorMessage);
                errorMessage.ThrowIfError();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception during game thread loop.");
                break;
            }
        }

        NativeOnLoopExit(_nativeEngine);
    }

    private void RunRenderThread()
    {
        while (!_lifetime.ApplicationStopped.IsCancellationRequested)
        {
            _ = NativeRender(_nativeEngine, out var errorMessage);
            errorMessage.ThrowIfError();
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
                    &DisposeDelegate,
                    &EqualsDelegates
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
                    &DisposeDelegate,
                    &EqualsDelegates
                );
            }
        }
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

        if (ReferenceEquals(this, _instance))
        {
            Interlocked.Exchange(ref _instance, null);
        }

        _disposed = true;
        await _host.DisposeAsync();

        CultureManager.Instance.Dispose();
        NativeDestroy(_nativeEngine);
    }

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_destroy_engine")]
    internal static partial void NativeDestroy(IntPtr engine);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_engine_pump_tasks")]
    [return: MarshalAs(UnmanagedType.I1)]
    [MustUseReturnValue]
    private static partial bool NativePumpTasks(IntPtr engine, int maxTasks, out InteropError errorMessage);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_engine_render")]
    [return: MarshalAs(UnmanagedType.I1)]
    [MustUseReturnValue]
    private static partial bool NativeSyncRenderState(IntPtr engine, out InteropError errorMessage);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_engine_sync_render_state")]
    [return: MarshalAs(UnmanagedType.I1)]
    [MustUseReturnValue]
    private static partial bool NativeRender(IntPtr engine, out InteropError errorMessage);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_engine_on_loop_exit")]
    private static partial void NativeOnLoopExit(IntPtr engine);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_engine_wait_platform_events")]
    private static partial void NativeWaitEvents(IntPtr engine, long timeout = 10);

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

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_engine_on_shutdown_requested_add")]
    private static unsafe partial void NativeOnShutdownRequestedAdd(
        IntPtr engine,
        IntPtr callback,
        delegate* unmanaged<IntPtr, void> invoke,
        delegate* unmanaged<IntPtr, void> dispose,
        delegate* unmanaged<IntPtr, IntPtr, byte> equals
    );

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
