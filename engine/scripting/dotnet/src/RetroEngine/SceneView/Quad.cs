// // @file Quad.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Core.Drawing;
using RetroEngine.Core.Math;
using RetroEngine.Strings;

namespace RetroEngine.SceneView;

public sealed partial class Quad(Viewport viewport) : SceneObject(Type, viewport)
{
    private static readonly Name Type = new("geometry");

    public Vector2F Size
    {
        get;
        set
        {
            field = value;
            MarkDirty();
        }
    }

    public Color Color
    {
        get;
        set
        {
            field = value;
            MarkDirty();
        }
    }

    public override void SyncToNative()
    {
        base.SyncToNative();

        Span<Vertex> vertices = stackalloc Vertex[4];
        vertices[0] = new Vertex(new Vector2F(0, 0), new Vector2F(0, 0), Color);
        vertices[1] = new Vertex(Size with { Y = 0 }, new Vector2F(1, 0), Color);
        vertices[2] = new Vertex(new Vector2F(Size.X, Size.Y), new Vector2F(1, 1), Color);
        vertices[3] = new Vertex(Size with { X = 0 }, new Vector2F(0, 1), Color);
        ReadOnlySpan<uint> indices = [0, 2, 1, 2, 0, 3];

        unsafe
        {
            fixed (Vertex* pVertices = vertices)
            fixed (uint* pIndices = indices)
            {
                UpdateNativeData(Id, pVertices, vertices.Length, pIndices, indices.Length);
            }
        }
    }

    [LibraryImport("retro_runtime", EntryPoint = "retro_geometry_set_render_data")]
    private static unsafe partial void UpdateNativeData(
        RenderObjectId renderObjectId,
        Vertex* vertices,
        int vertexCount,
        uint* indices,
        int indexCount
    );
}
