// @file 2026.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Core.Math;

namespace RetroEngine.World;

public abstract partial class SceneObject : IDisposable
{
    protected SceneObject(Scene scene, SceneObject? parent, IntPtr id)
    {
        Scene = scene;
        NativeObject = id;
        Parent = parent;
        Scale = Vector2F.One;
    }

    public Scene Scene { get; }

    public IntPtr NativeObject { get; }

    protected bool Disposed
    {
        get => field || Scene.Disposed;
        private set;
    }

    public SceneObject? Parent
    {
        get;
        set
        {
            ThrowIfDisposed();
            field = value;
            if (field is not null)
            {
                NativeAttachToParent(NativeObject, field.NativeObject);
            }
            else
            {
                NativeDetachFromParent(NativeObject);
            }
        }
    }

    public Transform Transform
    {
        get;
        set
        {
            ThrowIfDisposed();
            field = value;
            NativeSetTransform(NativeObject, value);
        }
    }

    public Vector2F Position
    {
        get => Transform.Position;
        set => Transform = Transform with { Position = value };
    }

    public float Rotation
    {
        get => Transform.Rotation;
        set => Transform = Transform with { Rotation = value };
    }

    public Vector2F Scale
    {
        get => Transform.Scale;
        set => Transform = Transform with { Scale = value };
    }

    public int ZOrder
    {
        get;
        set
        {
            ThrowIfDisposed();
            field = NativeSetZOrder(NativeObject, value);
        }
    }

    protected void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(Scene.Disposed, Scene);
        ObjectDisposedException.ThrowIf(Disposed, this);
    }

    public void Dispose()
    {
        if (Disposed)
            return;

        NativeDispose(NativeObject);
        Disposed = true;
        GC.SuppressFinalize(this);
    }

    private const string LibraryName = "retro_runtime";

    [LibraryImport(LibraryName, EntryPoint = "retro_node_set_transform")]
    private static partial void NativeSetTransform(IntPtr obj, in Transform transform);

    [LibraryImport(LibraryName, EntryPoint = "retro_node_set_z_order")]
    private static partial int NativeSetZOrder(IntPtr obj, int zOrder);

    [LibraryImport(LibraryName, EntryPoint = "retro_node_attach_to_parent")]
    private static partial void NativeAttachToParent(IntPtr obj, IntPtr parent);

    [LibraryImport(LibraryName, EntryPoint = "retro_node_detach_from_parent")]
    private static partial void NativeDetachFromParent(IntPtr obj);

    [LibraryImport(LibraryName, EntryPoint = "retro_node_dispose")]
    private static partial void NativeDispose(IntPtr obj);
}
