// @file 2026.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using RetroEngine.Core.Math;
using RetroEngine.Interop;

namespace RetroEngine.World;

[NativeMarshalling(typeof(SceneObjectMarshaller))]
public abstract partial class SceneObject : IDisposable
{
    private readonly List<SceneObject> _children = [];

    protected SceneObject(Scene scene, SceneObject? parent, Func<Scene, SceneObject?, IntPtr> nativePtrFactory)
    {
        parent?.ThrowIfDisposed();
        scene.ThrowIfDisposed();
        Scene = scene;
        NativeObject = nativePtrFactory(scene, parent);
        Scene.AddObject(this);
        Parent = parent;
        Scale = Vector2F.One;
    }

    public Scene Scene { get; }

    public IntPtr NativeObject { get; }

    public bool Disposed
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
            if (value is not null)
            {
                NativeAttachToParent(this, field);
            }
            else
            {
                NativeDetachFromParent(this);
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
            NativeSetTransform(this, value);
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
            field = NativeSetZOrder(this, value);
        }
    }

    private void AddChild(SceneObject child)
    {
        _children.Add(child);
    }

    private void RemoveChild(SceneObject child)
    {
        for (var i = 0; i < _children.Count; i++)
        {
            if (!ReferenceEquals(_children[i], child))
                continue;
            _children.RemoveAt(i);
            break;
        }
    }

    protected void ThrowIfDisposed()
    {
        Scene.ThrowIfDisposed();
        ObjectDisposedException.ThrowIf(Disposed, this);
    }

    public void Dispose()
    {
        if (Disposed)
            return;

        NativeDispose(Scene, this);
        Disposed = true;
        GC.SuppressFinalize(this);
    }

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_node_set_transform")]
    private static partial void NativeSetTransform(SceneObject obj, in Transform transform);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_node_set_z_order")]
    private static partial int NativeSetZOrder(SceneObject obj, int zOrder);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_node_attach_to_parent")]
    private static partial void NativeAttachToParent(SceneObject obj, SceneObject? parent);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_node_detach_from_parent")]
    private static partial void NativeDetachFromParent(SceneObject obj);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_node_dispose")]
    private static partial void NativeDispose(Scene scene, SceneObject obj);
}

[CustomMarshaller(typeof(SceneObject), MarshalMode.ManagedToUnmanagedIn, typeof(SceneObjectMarshaller))]
public static class SceneObjectMarshaller
{
    public static IntPtr ConvertToUnmanaged(SceneObject? scene) => scene?.NativeObject ?? IntPtr.Zero;
}
