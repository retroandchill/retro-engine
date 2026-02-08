// @file 2026.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Core.Math;

namespace RetroEngine.World;

public abstract partial class SceneObject : IDisposable
{
    protected SceneObject(Scene scene, IntPtr id)
    {
        Scene = scene;
        NativeObject = id;
        Scale = Vector2F.One;
    }

    public Scene Scene { get; }

    public IntPtr NativeObject { get; }

    protected bool Disposed
    {
        get => field || Scene.Disposed;
        private set;
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

    [LibraryImport(LibraryName, EntryPoint = "retro_node_dispose")]
    private static partial void NativeDispose(IntPtr obj);
}
