// // @file $FILE$
// //
// // @copyright Copyright (c) $[InvalidReference] Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Core.Drawing;
using RetroEngine.Core.Math;
using RetroEngine.SceneView;

namespace RetroEngine.Interop;

internal static partial class QuadExporter
{
    private const string LibraryName = "retro_renderer";

    [LibraryImport(LibraryName, EntryPoint = "retro_quad_set_size")]
    public static partial void SetQuadSize(RenderObjectId renderObjectId, Vector2F size);

    [LibraryImport(LibraryName, EntryPoint = "retro_quad_set_color")]
    public static partial void SetQuadColor(RenderObjectId renderObjectId, Color color);
}
