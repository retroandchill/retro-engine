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
    private static readonly Name Type = new("Quad");

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
        UpdateNativeData(Id, new QuadNativeUpdate(Size, Color));
    }

    [StructLayout(LayoutKind.Sequential)]
    private readonly record struct QuadNativeUpdate(Vector2F Size, Color Color);

    [LibraryImport("retro_renderer", EntryPoint = "retro_quad_update_data")]
    private static partial void UpdateNativeData(RenderObjectId renderObjectId, in QuadNativeUpdate data);
}
