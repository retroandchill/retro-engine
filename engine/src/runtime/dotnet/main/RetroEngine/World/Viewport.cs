// // @file Viewport.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using RetroEngine.Core.Drawing;
using RetroEngine.Core.Math;
using RetroEngine.Interop;

namespace RetroEngine.World;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct CameraLayout(Vector2F Position, Vector2F Pivot, float Rotation, float Zoom);

[NativeMarshalling(typeof(ViewportMarshaller))]
public sealed partial class Viewport : IDisposable
{
    private readonly ViewportManager _manager;
    public IntPtr NativeHandle { get; private set; }

    public bool Disposed => NativeHandle == IntPtr.Zero || _manager.Disposed;

    public Scene? Scene
    {
        get;
        set
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            field = value;
            NativeSetScene(this, field);
        }
    }

    public ScreenLayout ScreenLayout
    {
        get;
        set
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            field = value;
            NativeSetScreenLayout(this, field);
        }
    }

    public int ZOrder
    {
        get;
        set
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            field = value;
            NativeSetZOrder(this, field);
        }
    }

    public CameraLayout CameraLayout
    {
        get;
        set
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            field = value;
            NativeSetCameraLayout(this, field);
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

    public Viewport(ViewportManager manager)
    {
        _manager = manager;
        NativeHandle = NativeCreate(manager, out var error);
        error.ThrowIfError();
        _manager.AddViewport(this);
        CameraZoom = 1.0f;
    }

    internal void ThrowIfDisposed()
    {
        _manager.ThrowIfDisposed();
        ObjectDisposedException.ThrowIf(Disposed, this);
    }

    public void Dispose()
    {
        if (Disposed)
            return;

        NativeDestroy(_manager, this);
        NativeHandle = IntPtr.Zero;
        _manager.RemoveViewport(this);
    }

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_viewport_create")]
    private static partial IntPtr NativeCreate(ViewportManager manager, out InteropError errorMessage);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_viewport_destroy")]
    private static partial void NativeDestroy(ViewportManager manager, Viewport ptr);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_viewport_set_scene")]
    private static partial void NativeSetScene(Viewport viewport, Scene? scene);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_viewport_set_screen_layout")]
    private static partial void NativeSetScreenLayout(Viewport viewport, in ScreenLayout layout);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_viewport_set_z_order")]
    private static partial void NativeSetZOrder(Viewport viewport, int zOrder);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_viewport_set_camera_layout")]
    private static partial void NativeSetCameraLayout(Viewport viewport, in CameraLayout layout);
}

[CustomMarshaller(typeof(Viewport), MarshalMode.ManagedToUnmanagedIn, typeof(ViewportMarshaller))]
public static class ViewportMarshaller
{
    public static IntPtr ConvertToUnmanaged(Viewport? viewport) => viewport?.NativeHandle ?? IntPtr.Zero;
}
