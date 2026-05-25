// @file Widget.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Core.Drawing;
using RetroEngine.Core.Math;
using RetroEngine.World;

namespace RetroEngine.UI;

[Flags]
public enum LayoutDirtyFlags : byte
{
    None = 0,
    Measure = 1 << 0,
    Arrange = 1 << 1,
    Visual = 1 << 2,
}

public abstract class Widget(SceneObject? sceneObject) : IDisposable
{
    private bool _disposed;

    public LayoutSlot? LayoutSlot { get; internal set; }

    public Vector2F DesiredSize { get; protected set; }

    public Vector2F ActualSize { get; protected set; }

    public Vector2F ActualPosition { get; private set; }

    public RectF ActualBounds { get; private set; }

    protected SceneObject? SceneObject { get; } = sceneObject;

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        LayoutSlot?.Parent.Remove(this);
        SceneObject?.Dispose();
        GC.SuppressFinalize(this);
    }
}
