// @file FontService.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using RetroEngine.Async;
using RetroEngine.Interop;

namespace RetroEngine.Rendering.Text;

[NativeMarshalling(typeof(FontServiceMarshaller))]
[RegisterSingleton]
public sealed partial class FontService : IDisposable
{
    internal IntPtr NativeHandle { get; private set; }

    public FontService(RenderBackend renderBackend)
    {
        NativeHandle = NativeCreate(renderBackend, out var error);
        error.ThrowIfError();
    }

    ~FontService()
    {
        Dispose();
    }

    public Font LoadFont(scoped ReadOnlySpan<byte> bytes)
    {
        return LoadFontAsync(bytes).GetAwaiter().GetResult();
    }

    public Task<Font> LoadFontAsync(scoped ReadOnlySpan<byte> bytes, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        var tcs = new TaskCompletionSource<Font>();
        using var nativeSource = NativeCancellationSource.FromCancellationToken(cancellationToken);

        var gcHandle = GCHandle.Alloc(tcs);
        tcs.Task.ContinueWith(_ => gcHandle.Free(), TaskContinuationOptions.ExecuteSynchronously);
        unsafe
        {
            NativeLoadFont(
                this,
                bytes,
                bytes.Length,
                &OnLoaded,
                &OnError,
                &OnCancelled,
                GCHandle.ToIntPtr(gcHandle),
                nativeSource
            );
        }
        return tcs.Task;
    }

    [UnmanagedCallersOnly]
    private static void OnLoaded(IntPtr userData, IntPtr nativeHandle)
    {
        var tcs = (TaskCompletionSource<Font>)GCHandle.FromIntPtr(userData).Target!;
        tcs.TrySetResult(new Font(nativeHandle));
    }

    [UnmanagedCallersOnly]
    private static void OnError(IntPtr userData, NativeInteropError error)
    {
        var tcs = (TaskCompletionSource<Font>)GCHandle.FromIntPtr(userData).Target!;
        tcs.TrySetException(
            InteropErrorMarshaller.ConvertToManaged(error).ToException()
                ?? new InvalidOperationException("Unknown error")
        );
    }

    [UnmanagedCallersOnly]
    private static void OnCancelled(IntPtr userData)
    {
        var tcs = (TaskCompletionSource<Font>)GCHandle.FromIntPtr(userData).Target!;
        tcs.TrySetCanceled();
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(NativeHandle == IntPtr.Zero, this);
    }

    public void Dispose()
    {
        if (NativeHandle == IntPtr.Zero)
            return;

        NativeDestroy(this);
        NativeHandle = IntPtr.Zero;
        GC.SuppressFinalize(this);
    }

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_font_service_create")]
    private static partial IntPtr NativeCreate(RenderBackend renderBackend, out InteropError error);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_font_service_destroy")]
    private static partial void NativeDestroy(FontService service);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_font_service_load_font")]
    private static unsafe partial IntPtr NativeLoadFont(
        FontService service,
        ReadOnlySpan<byte> bytes,
        int length,
        delegate* unmanaged<IntPtr, IntPtr, void> onLoaded,
        delegate* unmanaged<IntPtr, NativeInteropError, void> onError,
        delegate* unmanaged<IntPtr, void> onCancelled,
        IntPtr userData,
        NativeCancellationSource? cancellationSource
    );
}

[CustomMarshaller(typeof(FontService), MarshalMode.ManagedToUnmanagedIn, typeof(FontServiceMarshaller))]
public static class FontServiceMarshaller
{
    public static IntPtr ConvertToUnmanaged(FontService? service) => service?.NativeHandle ?? IntPtr.Zero;
}
