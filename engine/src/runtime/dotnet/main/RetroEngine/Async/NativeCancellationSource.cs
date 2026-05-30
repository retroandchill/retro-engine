// @file NativeCancellationSource.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using RetroEngine.Interop;

namespace RetroEngine.Async;

[NativeMarshalling(typeof(NativeCancellationSourceMarshaller))]
public sealed partial class NativeCancellationSource : IDisposable
{
    internal IntPtr NativeHandle { get; private set; } = NativeCreate();

    ~NativeCancellationSource()
    {
        Dispose();
    }

    public void RequestCancellation()
    {
        ThrowIfDisposed();
        NativeRequestCancellation(NativeHandle);
    }

    public static NativeCancellationSource? FromCancellationToken(CancellationToken cancellationToken)
    {
        return cancellationToken.CanBeCanceled ? new NativeCancellationSource() : null;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(NativeHandle == IntPtr.Zero, this);
    }

    public void Dispose()
    {
        if (NativeHandle == IntPtr.Zero)
            return;

        NativeDispose(NativeHandle);
        NativeHandle = IntPtr.Zero;
        GC.SuppressFinalize(this);
    }

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_stop_source_create")]
    private static partial IntPtr NativeCreate();

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_stop_source_destroy")]
    private static partial void NativeDispose(IntPtr handle);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_stop_source_request_stop")]
    private static partial void NativeRequestCancellation(IntPtr handle);
}

[CustomMarshaller(
    typeof(NativeCancellationSource),
    MarshalMode.ManagedToUnmanagedIn,
    typeof(NativeCancellationSourceMarshaller)
)]
public static class NativeCancellationSourceMarshaller
{
    public static IntPtr ConvertToUnmanaged(NativeCancellationSource? cancellationSource) =>
        cancellationSource?.NativeHandle ?? IntPtr.Zero;
}
