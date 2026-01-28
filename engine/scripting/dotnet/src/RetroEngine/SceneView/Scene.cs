// // @file Scene.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Core.Drawing;

namespace RetroEngine.SceneView;

public static partial class Scene
{
    private static readonly HashSet<ITransformSync> DirtyTransforms = [];
    private static readonly HashSet<IViewSync> DirtyViews = [];
    private static readonly HashSet<IGeometrySync> DirtyGeometry = [];

    public static void MarkAsDirty(INativeSynchronizable syncable)
    {
        if (syncable is ITransformSync transformSync)
            DirtyTransforms.Add(transformSync);
        if (syncable is IViewSync viewSync)
            DirtyViews.Add(viewSync);
        if (syncable is IGeometrySync geometrySync)
            DirtyGeometry.Add(geometrySync);
    }

    public static void Sync()
    {
        SyncTransforms();
        SyncViews();
        SyncGeometry();
    }

    private static void SyncTransforms()
    {
        if (DirtyTransforms.Count == 0)
            return;

        var buffer = new TransformUpdate[DirtyTransforms.Count];
        var i = 0;
        foreach (var item in DirtyTransforms)
        {
            buffer[i++] = new TransformUpdate
            {
                NativeObject = item.NativeObject,
                Position = item.Position,
                Rotation = item.Rotation,
                Scale = item.Scale,
            };
        }
        NativeSyncTransforms(buffer, buffer.Length);
        DirtyTransforms.Clear();
    }

    private static void SyncViews()
    {
        if (DirtyViews.Count == 0)
            return;

        var buffer = new ViewUpdate[DirtyViews.Count];
        var i = 0;
        foreach (var item in DirtyViews)
        {
            buffer[i++] = new ViewUpdate { NativeObject = item.NativeObject, Size = item.Size };
        }
        NativeSetViewportSizes(buffer, buffer.Length);
        DirtyTransforms.Clear();
    }

    private static void SyncGeometry()
    {
        foreach (var geometry in DirtyGeometry)
        {
            geometry.SyncGeometry(
                (nativeObject, vertices, indices) =>
                    NativeSetDrawGeometry(nativeObject, vertices, vertices.Length, indices, indices.Length)
            );
        }
        DirtyGeometry.Clear();
    }

    private const string LibraryName = "retro_runtime";

    [LibraryImport(LibraryName, EntryPoint = "retro_scene_update_transforms")]
    private static partial void NativeSyncTransforms([In] TransformUpdate[] buffer, int count);

    [LibraryImport(LibraryName, EntryPoint = "retro_scene_update_viewports")]
    private static partial void NativeSetViewportSizes([In] ViewUpdate[] buffer, int count);

    [LibraryImport("retro_runtime", EntryPoint = "retro_geometry_set_render_data")]
    private static unsafe partial void NativeSetDrawGeometry(
        IntPtr nativeObject,
        ReadOnlySpan<Vertex> vertices,
        int vertexCount,
        ReadOnlySpan<uint> indices,
        int indexCount
    );
}
