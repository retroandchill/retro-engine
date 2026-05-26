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
    internal static ViewportManager? Manager { get; set; }
    public IntPtr NativeHandle { get; private set; }

    public bool Disposed => NativeHandle == IntPtr.Zero || Manager is null;

    public Scene? Scene
    {
        get;
        set
        {
            ThrowIfDisposed();
            if (ReferenceEquals(field, value))
                return;

            field = value;
            NativeSetScene(this, field);
        }
    }

    public AnchorData ScreenLayout
    {
        get;
        set
        {
            ThrowIfDisposed();
            if (field == value)
                return;

            field = value;
            NativeSetScreenLayout(this, field);
        }
    }

    public RectI ScreenRect
    {
        get
        {
            ThrowIfDisposed();
            return NativeGetScreenRect(this);
        }
    }

    public int ZOrder
    {
        get;
        set
        {
            ThrowIfDisposed();
            if (field == value)
                return;

            field = value;
            NativeSetZOrder(this, field);
        }
    }

    public CameraLayout CameraLayout
    {
        get;
        set
        {
            ThrowIfDisposed();
            if (field == value)
                return;

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

    public event Action<RectI>? ScreenRectChanged
    {
        add
        {
            ThrowIfDisposed();
            if (value is null)
                return;

            unsafe
            {
                var handle = GCHandle.Alloc(value);
                NativeScreenRectChangedAdd(
                    this,
                    GCHandle.ToIntPtr(handle),
                    &ScreenRectChangedCallback,
                    &DisposeDelegate,
                    &EqualsDelegates
                );
            }
        }
        remove
        {
            ThrowIfDisposed();
            if (value is null)
                return;

            unsafe
            {
                var handle = GCHandle.Alloc(value);
                NativeScreenRectChangedRemove(
                    this,
                    GCHandle.ToIntPtr(handle),
                    &ScreenRectChangedCallback,
                    &DisposeDelegate,
                    &EqualsDelegates
                );
            }
        }
    }

    public Viewport()
    {
        if (Manager is null)
            throw new InvalidOperationException("Viewport manager is not initialized.");

        NativeHandle = NativeCreate(Manager, out var error);
        error.ThrowIfError();
        Manager.AddViewport(this);
        CameraZoom = 1.0f;
        ScreenLayout = new AnchorData
        {
            Anchors = new Anchors { Minimum = new Vector2F(0, 0), Maximum = new Vector2F(1, 1) },
        };
    }

    [UnmanagedCallersOnly]
    private static void ScreenRectChangedCallback(IntPtr viewportPtr, RectI rect)
    {
        var action = (Action<RectI>?)GCHandle.FromIntPtr(viewportPtr).Target;
        action?.Invoke(rect);
    }

    [UnmanagedCallersOnly]
    private static void DisposeDelegate(IntPtr userData)
    {
        var handle = GCHandle.FromIntPtr(userData);
        handle.Free();
    }

    [UnmanagedCallersOnly]
    private static byte EqualsDelegates(IntPtr lhs, IntPtr rhs)
    {
        var lhsHandle = GCHandle.FromIntPtr(lhs);
        var rhsHandle = GCHandle.FromIntPtr(rhs);
        return Equals(lhsHandle.Target, rhsHandle.Target) ? (byte)1 : (byte)0;
    }

    internal void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
    }

    public void Dispose()
    {
        if (Disposed)
            return;

        if (Manager is not null)
        {
            NativeDestroy(Manager, this);
            Manager.RemoveViewport(this);
        }
        NativeHandle = IntPtr.Zero;
    }

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_viewport_create")]
    private static partial IntPtr NativeCreate(ViewportManager manager, out InteropError errorMessage);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_viewport_destroy")]
    private static partial void NativeDestroy(ViewportManager manager, Viewport ptr);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_viewport_set_scene")]
    private static partial void NativeSetScene(Viewport viewport, Scene? scene);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_viewport_set_screen_layout")]
    private static partial void NativeSetScreenLayout(Viewport viewport, in AnchorData layout);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_viewport_set_z_order")]
    private static partial void NativeSetZOrder(Viewport viewport, int zOrder);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_viewport_set_camera_layout")]
    private static partial void NativeSetCameraLayout(Viewport viewport, in CameraLayout layout);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_viewport_get_screen_rect")]
    private static partial RectI NativeGetScreenRect(Viewport viewport);

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_viewport_screen_rect_changed_add")]
    private static unsafe partial void NativeScreenRectChangedAdd(
        Viewport viewport,
        IntPtr userData,
        delegate* unmanaged<IntPtr, RectI, void> invoke,
        delegate* unmanaged<IntPtr, void> dispose,
        delegate* unmanaged<IntPtr, IntPtr, byte> equals
    );

    [LibraryImport(NativeLibraries.RetroRuntime, EntryPoint = "retro_viewport_screen_rect_changed_remove")]
    private static unsafe partial void NativeScreenRectChangedRemove(
        Viewport viewport,
        IntPtr userData,
        delegate* unmanaged<IntPtr, RectI, void> invoke,
        delegate* unmanaged<IntPtr, void> dispose,
        delegate* unmanaged<IntPtr, IntPtr, byte> equals
    );
}

[CustomMarshaller(typeof(Viewport), MarshalMode.ManagedToUnmanagedIn, typeof(ViewportMarshaller))]
public static class ViewportMarshaller
{
    public static IntPtr ConvertToUnmanaged(Viewport? viewport) => viewport?.NativeHandle ?? IntPtr.Zero;
}
