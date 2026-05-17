// @file FontFace.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using RetroEngine.Interop;

namespace RetroEngine.Rendering.Text;

[NativeMarshalling(typeof(FontFaceMarshaller))]
public sealed partial class FontFace : IDisposable
{
    public const uint NullGlyph = 0;

    internal IntPtr NativeHandle { get; private set; }

    public string FamilyName
    {
        get
        {
            ThrowIfDisposed();
            var nativeName = NativeGetFamilyName(this, out var length);
            return length > 0 ? Marshal.PtrToStringUTF8(nativeName, length) : "";
        }
    }

    public string StyleName
    {
        get
        {
            ThrowIfDisposed();
            var nativeName = NativeGetStyleName(this, out var length);
            return length > 0 ? Marshal.PtrToStringUTF8(nativeName, length) : "";
        }
    }

    internal FontFace(IntPtr nativeHandle)
    {
        NativeHandle = nativeHandle;
    }

    ~FontFace()
    {
        Dispose();
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

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_font_face_destroy")]
    private static partial void NativeDestroy(FontFace service);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_font_face_get_family_name")]
    private static partial IntPtr NativeGetFamilyName(FontFace service, out int length);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_font_face_get_style_name")]
    private static partial IntPtr NativeGetStyleName(FontFace service, out int length);
}

[CustomMarshaller(typeof(FontFace), MarshalMode.ManagedToUnmanagedIn, typeof(FontFaceMarshaller))]
public static class FontFaceMarshaller
{
    public static IntPtr ConvertToUnmanaged(FontFace? service) => service?.NativeHandle ?? IntPtr.Zero;
}
