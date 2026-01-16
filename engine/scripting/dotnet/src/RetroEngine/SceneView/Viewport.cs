// // @file 2026.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace RetroEngine.SceneView;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct ViewportId(uint Index, uint Generation);

public sealed partial class Viewport : IDisposable
{
    public ViewportId Id { get; } = NativeCreate();
    private bool _disposed;

    private readonly Dictionary<RenderObjectId, SceneObject> _objects = new();

    public void AddRenderObject(SceneObject obj)
    {
        _objects.Add(obj.Id, obj);
    }

    public bool RemoveRenderObject(RenderObjectId id)
    {
        if (!_objects.Remove(id, out var obj))
            return false;

        obj.Dispose();
        return true;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        foreach (var (_, obj) in _objects)
        {
            obj.Dispose();
        }
        NativeDispose(Id);
        _disposed = true;
    }

    private const string LibraryName = "retro_runtime";

    [LibraryImport(LibraryName, EntryPoint = "retro_viewport_create")]
    private static partial ViewportId NativeCreate();

    [LibraryImport(LibraryName, EntryPoint = "retro_viewport_dispose")]
    private static partial void NativeDispose(ViewportId viewportId);
}
