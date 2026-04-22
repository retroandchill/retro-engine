// // @file RenderManager.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using RetroEngine.Interop;
using RetroEngine.Platform;
using RetroEngine.World;
using ZLinq;

namespace RetroEngine.Rendering;

[RegisterSingleton]
public sealed partial class RenderManager : IDisposable
{
    private IntPtr _nativeHandle;

    public RenderManager(
        PlatformBackend platformBackend,
        RenderBackend renderBackend,
        ViewportManager viewportManager,
        IEnumerable<RenderPipeline> pipelines
    )
    {
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
    }

    public Window CreateWindow(ReadOnlySpan<char> title, int width, int height, WindowFlags flags)
    {
        var window = NativeCreateWindow(_nativeHandle, title, title.Length, width, height, flags, out var error);
        error.ThrowIfError();
        return new Window(window, true);
    }

    public Task<Window> CreateWindowAsync(
        string title,
        int width,
        int height,
        WindowFlags flags,
        CancellationToken cancellationToken = default
    )
    {
        return CreateWindowAsync(title.AsMemory(), width, height, flags, cancellationToken);
    }

    public async Task<Window> CreateWindowAsync(
        ReadOnlyMemory<char> title,
        int width,
        int height,
        WindowFlags flags,
        CancellationToken cancellationToken = default
    )
    {
        var tcs = new TaskCompletionSource<Window>(cancellationToken);
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

    [UnmanagedCallersOnly]
    private static void OnWindowCreated(IntPtr userData, IntPtr windowId)
    {
        var tcs = (TaskCompletionSource<Window>)GCHandle.FromIntPtr(userData).Target!;
        tcs.TrySetResult(new Window(windowId, true));
    }

    [UnmanagedCallersOnly]
    private static unsafe void OnWindowError(IntPtr userData, byte* errorMessage)
    {
        var tcs = (TaskCompletionSource<Window>)GCHandle.FromIntPtr(userData).Target!;
        tcs.TrySetException(new PlatformNotSupportedException(Utf8StringMarshaller.ConvertToManaged(errorMessage)));
    }

    public void Dispose()
    {
        if (_nativeHandle == IntPtr.Zero)
            return;

        NativeDestroy(_nativeHandle);
        _nativeHandle = IntPtr.Zero;
    }

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_renderer_manager_create")]
    private static partial IntPtr NativeCreate(
        PlatformBackend platformBackend,
        RenderBackend renderBackend,
        ViewportManager viewportManager,
        ReadOnlySpan<IntPtr> pipelines,
        int pipelineCount,
        out InteropError error
    );

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_renderer_manager_destroy")]
    private static partial void NativeDestroy(IntPtr ptr);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_renderer_manager_create_window")]
    private static unsafe partial IntPtr NativeCreateWindow(
        IntPtr engine,
        ReadOnlySpan<char> title,
        int tileLength,
        int width,
        int height,
        WindowFlags flags,
        out InteropError error
    );

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_renderer_manager_create_window_async")]
    private static unsafe partial void NativeCreateWindowAsync(
        IntPtr engine,
        ReadOnlySpan<char> title,
        int tileLength,
        int width,
        int height,
        WindowFlags flags,
        IntPtr userData,
        delegate* unmanaged<IntPtr, IntPtr, void> onWindowCreated,
        delegate* unmanaged<IntPtr, byte*, void> onError
    );
}
