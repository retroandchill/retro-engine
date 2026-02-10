// // @file Viewport.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Core.Drawing;
using RetroEngine.Core.Math;

namespace RetroEngine.World;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct CameraLayout(Vector2F Position, Vector2F Pivot, float Rotation, float Zoom);

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

    public ScreenLayout ScreenLayout
    {
        get;
        set
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            field = value;
            NativeSetScreenLayout(NativeHandle, field);
        }
    }

    public CameraLayout CameraLayout
    {
        get;
        set
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            field = value;
            NativeSetCameraLayout(NativeHandle, field);
        }
    }

    public Vector2F CameraPosition
    {
        get => CameraLayout.Position;
        set => CameraLayout = CameraLayout with { Position = value };
    }

    public Vector2F CameraPivot
    {
        get => CameraLayout.Pivot;
        set => CameraLayout = CameraLayout with { Pivot = value };
    }

    public float CameraRotation
    {
        get => CameraLayout.Rotation;
        set => CameraLayout = CameraLayout with { Rotation = value };
    }

    public float CameraZoom
    {
        get => CameraLayout.Zoom;
        set => CameraLayout = CameraLayout with { Zoom = value };
    }

    public Viewport()
    {
        CameraZoom = 1.0f;
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

    [LibraryImport("retro_runtime", EntryPoint = "retro_viewport_set_screen_layout")]
    private static partial void NativeSetScreenLayout(IntPtr viewport, in ScreenLayout layout);

    [LibraryImport("retro_runtime", EntryPoint = "retro_viewport_set_camera_layout")]
    private static partial void NativeSetCameraLayout(IntPtr viewport, in CameraLayout layout);
}
