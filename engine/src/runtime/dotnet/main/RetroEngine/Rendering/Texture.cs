// // @file ITextureManager.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using RetroEngine.Interop;

namespace RetroEngine.Rendering;

public enum TextureFormat : byte
{
    Rgba8,
    Rgba16F,
}

[NativeMarshalling(typeof(TextureMarshaller))]
public sealed partial class Texture : IDisposable
{
    internal IntPtr NativeHandle { get; private set; }

    public int Width { get; }

    public int Height { get; }

    public TextureFormat Format { get; }

    internal Texture(IntPtr nativeHandle, int width, int height, TextureFormat format)
    {
        NativeHandle = nativeHandle;
        Width = width;
        Height = height;
        Format = format;
    }

    ~Texture()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (NativeHandle == IntPtr.Zero)
            return;

        NativeDestroy(NativeHandle);
        NativeHandle = IntPtr.Zero;
        GC.SuppressFinalize(this);
    }

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_texture_destroy")]
    private static partial void NativeDestroy(IntPtr ptr);
}

[CustomMarshaller(typeof(Texture), MarshalMode.ManagedToUnmanagedIn, typeof(TextureMarshaller))]
public static class TextureMarshaller
{
    public static IntPtr ConvertToUnmanaged(Texture? backend) => backend?.NativeHandle ?? IntPtr.Zero;
}
