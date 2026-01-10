// // @file $[InvalidReference].cs
// //
// // @copyright Copyright (c) $[InvalidReference] Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Core.Math;
using RetroEngine.Interop;
using RetroEngine.Strings;

namespace RetroEngine.SceneView;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct RenderObjectId(uint Index, uint Generation);

public abstract class SceneObject : IDisposable
{
    protected SceneObject(Name type, Viewport viewport, ReadOnlySpan<byte> payload)
    {
        Viewport = viewport;
        unsafe
        {
            fixed (byte* payloadPtr = payload)
            {
                Id = SceneExporter.CreateRenderObject(type, viewport.Id, (IntPtr)payloadPtr, payload.Length);
            }
        }
        Viewport.AddRenderObject(this);
    }

    public RenderObjectId Id { get; }
    private bool _disposed;

    public Viewport Viewport { get; }

    public Transform Transform
    {
        get;
        set
        {
            field = value;
            SceneExporter.SetRenderObjectTransform(Id, in field);
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

    public void Dispose()
    {
        if (_disposed)
            return;

        SceneExporter.DisposeRenderObject(Id);
        _disposed = true;
        Viewport.RemoveRenderObject(Id);
        GC.SuppressFinalize(this);
    }
}
