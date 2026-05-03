// // @file RenderManager.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Microsoft.Extensions.Hosting;
using RetroEngine.Interop;
using RetroEngine.Platform;
using RetroEngine.World;
using ZLinq;
using Zomp.SyncMethodGenerator;

namespace RetroEngine.Rendering;

[RegisterSingleton]
public sealed partial class RenderManager : IDisposable
{
    private IntPtr _nativeHandle;
    private readonly PlatformBackend _platformBackend;
    private readonly IHostApplicationLifetime _lifetime;

    public bool Disposed => _nativeHandle == IntPtr.Zero;

    public RenderManager(
        PlatformBackend platformBackend,
        RenderBackend renderBackend,
        ViewportManager viewportManager,
        IEnumerable<RenderPipeline> pipelines,
        IHostApplicationLifetime lifetime
    )
    {
        _platformBackend = platformBackend;
        var pipelineHandles = pipelines.AsValueEnumerable().Select(x => x.NativeHandle).ToArray();
        _nativeHandle = NativeCreate(
            platformBackend,
            renderBackend,
            viewportManager,
            pipelineHandles,
            pipelineHandles.Length,
            out var error
        );
        error.ThrowIfError();
        _lifetime = lifetime;
    }

    public event Action<ulong>? OnWindowRemoved
    {
        add
        {
            unsafe
            {
                var handle = GCHandle.Alloc(value);
                NativeOnWindowRemovedAdd(
                    _nativeHandle,
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
                    _nativeHandle,
                    GCHandle.ToIntPtr(handle),
                    &InvokeWindowRemoved,
                    &DisposeDelegate,
                    &EqualsDelegates
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

    public ValueTask CreateMainWindowAsync(
        string title,
        int width,
        int height,
        WindowFlags flags,
        CancellationToken cancellationToken = default
    )
    {
        return CreateMainWindowAsync(title.AsMemory(), width, height, flags, cancellationToken);
    }

    [CreateSyncVersion]
    public async ValueTask CreateMainWindowAsync(
        ReadOnlyMemory<char> title,
        int width,
        int height,
        WindowFlags flags,
        CancellationToken cancellationToken = default
    )
    {
        var windowId = await CreateWindowAsync(title, width, height, flags, cancellationToken);

        OnWindowRemoved += id =>
        {
            if (id != windowId)
                return;

            _lifetime.StopApplication();
        };
    }

    public ulong CreateWindow(ReadOnlySpan<char> title, int width, int height, WindowFlags flags)
    {
        var window = NativeCreateWindow(_nativeHandle, title, title.Length, width, height, flags, out var error);
        error.ThrowIfError();
        return window;
    }

    public Task<ulong> CreateWindowAsync(
        string title,
        int width,
        int height,
        WindowFlags flags,
        CancellationToken cancellationToken = default
    )
    {
        return CreateWindowAsync(title.AsMemory(), width, height, flags, cancellationToken);
    }

    public async Task<ulong> CreateWindowAsync(
        ReadOnlyMemory<char> title,
        int width,
        int height,
        WindowFlags flags,
        CancellationToken cancellationToken = default
    )
    {
        var tcs = new TaskCompletionSource<ulong>();
        await using var registration = cancellationToken.Register(() => tcs.TrySetCanceled());
        var tcsHandle = GCHandle.Alloc(tcs);
        try
        {
            unsafe
            {
                NativeCreateWindowAsync(
                    _nativeHandle,
                    title.Span,
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

    public ulong CreateWindowFromNative(NativeWindowHandle handle)
    {
        var window = NativeCreateWindow(_nativeHandle, handle, out var error);
        error.ThrowIfError();
        return window;
    }

    public async Task<ulong> CreateWindowFromNativeAsync(
        NativeWindowHandle handle,
        CancellationToken cancellationToken = default
    )
    {
        var tcs = new TaskCompletionSource<ulong>();
        await using var registration = cancellationToken.Register(() => tcs.TrySetCanceled());
        var tcsHandle = GCHandle.Alloc(tcs);
        try
        {
            unsafe
            {
                NativeCreateWindowAsync(
                    _nativeHandle,
                    handle,
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

    public void RemoveWindow(ulong windowId)
    {
        NativeRemoveWindow(_nativeHandle, windowId, out var error);
        error.ThrowIfError();
    }

    public PlatformWindowHandle GetWindowById(ulong id)
    {
        var windowHandle = NativeGetWindowById(_nativeHandle, id);
        return windowHandle != IntPtr.Zero
            ? new PlatformWindowHandle(windowHandle, _platformBackend.WindowBackend)
            : throw new InvalidOperationException("Window not found");
    }

    public void BindViewportToWindow(Viewport viewport, ulong windowId)
    {
        if (!NativeSetViewportWindow(_nativeHandle, viewport, windowId))
        {
            throw new InvalidOperationException("Failed to bind viewport to window");
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

    internal void SyncRenderState()
    {
        NativeSyncRenderState(_nativeHandle, out var error);
        error.ThrowIfError();
    }

    private void Render()
    {
        NativeRender(_nativeHandle, out var error);
        error.ThrowIfError();
    }

    internal void RenderLoop()
    {
        while (!_lifetime.ApplicationStopped.IsCancellationRequested)
        {
            Render();
        }
    }

    internal void OnEngineShutdown()
    {
        NativeOnEngineShutdown(_nativeHandle);
    }

    public void Dispose()
    {
        if (_nativeHandle == IntPtr.Zero)
            return;

        NativeDestroy(_nativeHandle);
        _nativeHandle = IntPtr.Zero;
    }

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_render_manager_create")]
    private static partial IntPtr NativeCreate(
        PlatformBackend platformBackend,
        RenderBackend renderBackend,
        ViewportManager viewportManager,
        ReadOnlySpan<IntPtr> pipelines,
        int pipelineCount,
        out InteropError error
    );

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_render_manager_destroy")]
    private static partial void NativeDestroy(IntPtr ptr);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_render_manager_create_window")]
    private static unsafe partial ulong NativeCreateWindow(
        IntPtr engine,
        ReadOnlySpan<char> title,
        int tileLength,
        int width,
        int height,
        WindowFlags flags,
        out InteropError error
    );

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_render_create_window_from_handle")]
    private static unsafe partial ulong NativeCreateWindow(
        IntPtr engine,
        NativeWindowHandle handle,
        out InteropError error
    );

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_render_manager_create_window_async")]
    private static unsafe partial void NativeCreateWindowAsync(
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

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_render_manager_create_window_from_handle_async")]
    private static unsafe partial void NativeCreateWindowAsync(
        IntPtr engine,
        NativeWindowHandle handle,
        IntPtr userData,
        delegate* unmanaged<IntPtr, ulong, void> onWindowCreated,
        delegate* unmanaged<IntPtr, byte*, void> onError
    );

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_render_manager_remove_window")]
    private static unsafe partial void NativeRemoveWindow(IntPtr engine, ulong windowId, out InteropError error);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_render_manager_get_window_by_id")]
    private static unsafe partial IntPtr NativeGetWindowById(IntPtr engine, ulong id);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_render_manager_set_viewport_window")]
    [return: MarshalAs(UnmanagedType.U1)]
    private static unsafe partial bool NativeSetViewportWindow(IntPtr engine, Viewport viewport, ulong windowId);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_render_manager_sync_render_state")]
    private static unsafe partial void NativeSyncRenderState(IntPtr engine, out InteropError error);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_render_manager_render")]
    private static unsafe partial void NativeRender(IntPtr engine, out InteropError error);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_render_manager_on_window_removed_add")]
    private static unsafe partial void NativeOnWindowRemovedAdd(
        IntPtr engine,
        IntPtr callback,
        delegate* unmanaged<IntPtr, ulong, void> invoke,
        delegate* unmanaged<IntPtr, void> dispose,
        delegate* unmanaged<IntPtr, IntPtr, byte> equals
    );

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_render_manager_on_window_removed_remove")]
    private static unsafe partial void NativeOnWindowRemovedRemove(
        IntPtr engine,
        IntPtr callback,
        delegate* unmanaged<IntPtr, ulong, void> invoke,
        delegate* unmanaged<IntPtr, void> dispose,
        delegate* unmanaged<IntPtr, IntPtr, byte> equals
    );

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_render_manager_on_engine_shutdown")]
    private static unsafe partial void NativeOnEngineShutdown(IntPtr engine);
}
