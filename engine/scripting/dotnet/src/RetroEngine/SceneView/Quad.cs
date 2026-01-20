// @file Quad.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Core.Drawing;
using RetroEngine.Core.Math;
using RetroEngine.Strings;

namespace RetroEngine.SceneView;

public sealed partial class Quad(SceneObject parent) : SceneObject(NativeCreate(parent.Id)), IGeometrySync
{
    private static readonly Name Type = new("geometry");

    public Vector2F Size
    {
        get;
        set
        {
            field = value;
            MarkAsDirty();
        }
    }

    public Color Color
    {
        get;
        set
        {
            field = value;
            MarkAsDirty();
        }
    }

    public void SyncGeometry(Action<uint, ReadOnlySpan<Vertex>, ReadOnlySpan<uint>> syncCallback)
    {
        syncCallback(
            Id,
            [
                new Vertex(new Vector2F(0, 0), new Vector2F(0, 0), Color),
                new Vertex(Size with { Y = 0 }, new Vector2F(1, 0), Color),
                new Vertex(new Vector2F(Size.X, Size.Y), new Vector2F(1, 1), Color),
                new Vertex(Size with { X = 0 }, new Vector2F(0, 1), Color),
            ],
            [0, 2, 1, 2, 0, 3]
        );
    }

    [LibraryImport("retro_runtime", EntryPoint = "retro_geometry_create")]
    private static unsafe partial uint NativeCreate(uint id);
}
