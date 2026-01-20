// @file 2026.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Core.Math;

namespace RetroEngine.SceneView;

public abstract partial class SceneObject : ITransformSync, IDisposable
{
    protected SceneObject(uint id)
    {
        Id = id;
        Scale = Vector2F.One;
    }

    public uint Id { get; }

    protected bool Disposed { get; private set; }

    public Vector2F Position
    {
        get;
        set
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            field = value;
            MarkAsDirty();
        }
    }

    public float Rotation
    {
        get;
        set
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            field = value;
            MarkAsDirty();
        }
    }

    public Vector2F Scale
    {
        get;
        set
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            field = value;
            MarkAsDirty();
        }
    }

    protected void MarkAsDirty()
    {
        Scene.MarkAsDirty(this);
    }

    public void Dispose()
    {
        if (Disposed)
            return;

        NativeDispose(Id);
        Disposed = true;
        GC.SuppressFinalize(this);
    }

    private const string LibraryName = "retro_runtime";

    [LibraryImport(LibraryName, EntryPoint = "retro_entity_dispose")]
    private static partial void NativeDispose(uint id);
}
