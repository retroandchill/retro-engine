// // @file Scene.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Portable.Interop;

namespace RetroEngine.World;

public sealed partial class Scene : IDisposable
{
    public IntPtr NativeHandle { get; } = NativeCreate();

    public bool Disposed { get; private set; }

    public void Dispose()
    {
        if (Disposed)
            return;

        NativeDestroy(NativeHandle);
        Disposed = true;
    }

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_scene_create")]
    private static partial IntPtr NativeCreate();

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_scene_destroy")]
    private static partial void NativeDestroy(IntPtr ptr);
}
