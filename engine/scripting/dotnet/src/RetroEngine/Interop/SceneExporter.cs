// // @file $[InvalidReference].cs
// //
// // @copyright Copyright (c) $[InvalidReference] Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Core.Math;
using RetroEngine.SceneView;
using RetroEngine.Strings;

namespace RetroEngine.Interop;

internal static partial class SceneExporter
{
    private const string LibraryName = "retro_runtime";

    [LibraryImport(LibraryName, EntryPoint = "retro_viewport_create")]
    public static partial ViewportId CreateViewport();

    [LibraryImport(LibraryName, EntryPoint = "retro_viewport_dispose")]
    public static partial void DisposeViewport(ViewportId viewportId);

    [LibraryImport(LibraryName, EntryPoint = "retro_render_object_create")]
    public static partial RenderObjectId CreateRenderObject(Name type, ViewportId viewportId);

    [LibraryImport(LibraryName, EntryPoint = "retro_render_object_dispose")]
    public static partial void DisposeRenderObject(RenderObjectId renderObjectId);

    [LibraryImport(LibraryName, EntryPoint = "retro_render_object_set_transform")]
    public static partial void SetRenderObjectTransform(RenderObjectId renderObjectId, in Transform transform);
}
