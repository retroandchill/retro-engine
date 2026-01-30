// @file Quad.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Core.Drawing;
using RetroEngine.Core.Math;
using RetroEngine.Strings;

namespace RetroEngine.SceneView;

public enum GeometryType : byte
{
    None,
    Rectangle,
    Triangle,
    Custom,
}

public sealed partial class Quad : SceneObject
{
    public Quad(SceneObject parent)
        : base(NativeCreate(parent.NativeObject))
    {
        NativeSetRenderData(NativeObject, GeometryType.Rectangle);
        Size = new Vector2F(100, 100);
        Color = new Color(1, 1, 1);
    }

    public Vector2F Size
    {
        get;
        set
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            field = value;
            NativeSetSize(NativeObject, value);
        }
    }

    public Color Color
    {
        get;
        set
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            field = value;
            NativeSetColor(NativeObject, value);
        }
    }

    public Vector2F Pivot
    {
        get;
        set
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            field = value;
            NativeSetPivot(NativeObject, value);
        }
    }

    [LibraryImport("retro_runtime", EntryPoint = "retro_geometry_create")]
    private static partial IntPtr NativeCreate(IntPtr id);

    [LibraryImport("retro_runtime", EntryPoint = "retro_geometry_set_type")]
    private static partial void NativeSetRenderData(IntPtr id, GeometryType type);

    [LibraryImport("retro_runtime", EntryPoint = "retro_geometry_set_render_data")]
    private static partial void NativeSetRenderData(
        IntPtr id,
        ReadOnlySpan<Vertex> vertices,
        int vertexCount,
        ReadOnlySpan<uint> indices,
        int indexCount
    );

    private static void NativeSetRenderData(IntPtr id, ReadOnlySpan<Vertex> vertices, ReadOnlySpan<uint> indices) =>
        NativeSetRenderData(id, vertices, vertices.Length, indices, indices.Length);

    [LibraryImport("retro_runtime", EntryPoint = "retro_geometry_set_color")]
    private static partial void NativeSetColor(IntPtr id, Color color);

    [LibraryImport("retro_runtime", EntryPoint = "retro_geometry_set_pivot")]
    private static partial void NativeSetPivot(IntPtr id, Vector2F pivot);

    [LibraryImport("retro_runtime", EntryPoint = "retro_geometry_set_size")]
    private static partial void NativeSetSize(IntPtr id, Vector2F size);
}
