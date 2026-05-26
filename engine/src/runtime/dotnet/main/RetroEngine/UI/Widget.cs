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

    public Widget? Parent => LayoutSlot?.Parent;

    public Vector2F DesiredSize { get; protected set; }

    public Vector2F ActualSize { get; protected set; }

    public Vector2F ActualPosition { get; private set; }

    public RectF ActualBounds { get; private set; }

    public LayoutDirtyFlags DirtyFlags { get; private set; } =
        LayoutDirtyFlags.Measure | LayoutDirtyFlags.Arrange | LayoutDirtyFlags.Visual;

    protected SceneObject? SceneObject { get; } = sceneObject;

    public Vector2F Measure(Vector2F availableSize)
    {
        ThrowIfDisposed();

        DesiredSize = ComputeDesiredSize(availableSize);
        DirtyFlags &= ~LayoutDirtyFlags.Measure;

        return DesiredSize;
    }

    public void Arrange(RectF finalRect)
    {
        ThrowIfDisposed();

        ActualPosition = new Vector2F(finalRect.X, finalRect.Y);
        ActualSize = new Vector2F(finalRect.Width, finalRect.Height);
        ActualBounds = finalRect;

        OnArrange(finalRect);

        ApplyLayoutToScene(finalRect);

        DirtyFlags &= ~LayoutDirtyFlags.Arrange;
    }

    protected abstract Vector2F ComputeDesiredSize(Vector2F availableSize);

    protected virtual void OnArrange(RectF finalRect) { }

    protected virtual void ApplyLayoutToScene(RectF finalRect)
    {
        SceneObject?.Position = new Vector2F(finalRect.X, finalRect.Y);
    }

    public void InvalidateMeasure()
    {
        DirtyFlags |= LayoutDirtyFlags.Measure | LayoutDirtyFlags.Arrange;
        LayoutSlot?.Parent.InvalidateMeasure();
    }

    public void InvalidateArrange()
    {
        DirtyFlags |= LayoutDirtyFlags.Arrange;
        LayoutSlot?.Parent.InvalidateArrange();
    }

    protected void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        SceneObject?.Dispose();
        GC.SuppressFinalize(this);
    }
}
