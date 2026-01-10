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
public readonly record struct ViewportId(uint Index, uint Generation);

public sealed class Viewport : IDisposable
{
    public ViewportId Id { get; } = SceneExporter.CreateViewport();
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
        SceneExporter.DisposeViewport(Id);
        _disposed = true;
    }
}
