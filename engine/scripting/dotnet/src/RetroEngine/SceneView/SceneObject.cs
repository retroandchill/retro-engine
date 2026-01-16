// // @file 2026.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Core.Math;
using RetroEngine.Core.State;
using RetroEngine.Strings;

namespace RetroEngine.SceneView;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct RenderObjectId(uint Index, uint Generation);

public abstract partial class SceneObject : INativeSynchronizable, IDisposable
{
    protected SceneObject(Name type, Viewport viewport)
    {
        Viewport = viewport;
        Id = NativeCreate(type, viewport.Id);
        Viewport.AddRenderObject(this);
    }

    public RenderObjectId Id { get; }
    protected bool Disposed { get; private set; }

    public Viewport Viewport { get; }

    public Transform Transform
    {
        get;
        set
        {
            field = value;
            MarkDirty();
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

    public virtual void SyncToNative()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        NativeSetTransform(Id, Transform);
    }

    public void Dispose()
    {
        if (Disposed)
            return;

        NativeDispose(Id);
        Disposed = true;
        Viewport.RemoveRenderObject(Id);
        GC.SuppressFinalize(this);
    }

    protected void MarkDirty()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
        this.AddToSyncList();
    }

    private const string LibraryName = "retro_runtime";

    [LibraryImport(LibraryName, EntryPoint = "retro_render_object_create")]
    private static partial RenderObjectId NativeCreate(Name type, ViewportId viewportId);

    [LibraryImport(LibraryName, EntryPoint = "retro_render_object_dispose")]
    private static partial void NativeDispose(RenderObjectId renderObjectId);

    [LibraryImport(LibraryName, EntryPoint = "retro_render_object_set_transform")]
    private static partial void NativeSetTransform(RenderObjectId renderObjectId, in Transform transform);
}
