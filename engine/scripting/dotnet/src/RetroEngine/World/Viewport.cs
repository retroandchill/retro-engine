// // @file Viewport.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace RetroEngine.World;

public sealed partial class Viewport : IDisposable
{
    public IntPtr NativeHandle { get; } = NativeCreate();

    public bool Disposed { get; private set; }

    public Scene? Scene
    {
        get;
        set
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            field = value;
            NativeSetScene(NativeHandle, field?.NativeHandle ?? IntPtr.Zero);
        }
    }

    public void Dispose()
    {
        if (Disposed)
            return;

        NativeDestroy(NativeHandle);
        Disposed = true;
    }

    [LibraryImport("retro_runtime", EntryPoint = "retro_viewport_create")]
    private static partial IntPtr NativeCreate();

    [LibraryImport("retro_runtime", EntryPoint = "retro_viewport_destroy")]
    private static partial void NativeDestroy(IntPtr ptr);

    [LibraryImport("retro_runtime", EntryPoint = "retro_viewport_set_scene")]
    private static partial void NativeSetScene(IntPtr viewport, IntPtr scene);
}
