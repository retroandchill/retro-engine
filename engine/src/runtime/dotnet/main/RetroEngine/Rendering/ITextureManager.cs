// // @file ITextureManager.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Interop;

namespace RetroEngine.Rendering;

public sealed partial class TextureRenderData : IDisposable
{
    private IntPtr _nativeHandle;

    public int Width { get; }

    public int Height { get; }

    internal TextureRenderData(IntPtr nativeHandle, int width, int height)
    {
        _nativeHandle = nativeHandle;
        Width = width;
        Height = height;
    }

    public void Dispose()
    {
        if (_nativeHandle == IntPtr.Zero)
            return;

        NativeDestroy(_nativeHandle);
        _nativeHandle = IntPtr.Zero;
    }

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_texture_destroy")]
    private static partial void NativeDestroy(IntPtr ptr);
}
