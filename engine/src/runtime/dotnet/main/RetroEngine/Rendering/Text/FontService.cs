// @file FontService.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
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
        ThrowIfDisposed();
        var nativeHandle = NativeLoadFont(this, bytes, bytes.Length, out var error);
        error.ThrowIfError();
        return new Font(nativeHandle);
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
    private static partial IntPtr NativeLoadFont(
        FontService service,
        ReadOnlySpan<byte> bytes,
        int length,
        out InteropError error
    );
}

[CustomMarshaller(typeof(FontService), MarshalMode.ManagedToUnmanagedIn, typeof(FontServiceMarshaller))]
public static class FontServiceMarshaller
{
    public static IntPtr ConvertToUnmanaged(FontService? service) => service?.NativeHandle ?? IntPtr.Zero;
}
