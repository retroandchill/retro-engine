// @file Widget.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

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

public abstract class Widget(IUiRoot root, SceneObject? sceneObject) : IDisposable
{
    private bool _disposed;

    public IUiRoot Root { get; } = root;

    public LayoutSlot? LayoutSlot { get; internal set; }

    public Widget? Parent => LayoutSlot?.Parent;

    public Vector2F DesiredSize { get; protected set; }

    public Vector2F ActualSize { get; protected set; }

    public Vector2F ActualPosition { get; private set; }

    public RectF ActualBounds { get; private set; }

    public LayoutDirtyFlags DirtyFlags { get; private set; } =
        LayoutDirtyFlags.Measure | LayoutDirtyFlags.Arrange | LayoutDirtyFlags.Visual;

    protected internal SceneObject? SceneObject { get; } = sceneObject;

    public Vector2F Measure(Vector2F availableSize)
    {
        ThrowIfDisposed();
        if (!DirtyFlags.HasFlag(LayoutDirtyFlags.Measure))
            return DesiredSize;

        DesiredSize = ComputeDesiredSize(availableSize);
        DirtyFlags &= ~LayoutDirtyFlags.Measure;

        return DesiredSize;
    }

    public void Arrange(RectF finalRect)
    {
        ThrowIfDisposed();
        if (!DirtyFlags.HasFlag(LayoutDirtyFlags.Arrange))
            return;

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
        var wasMeasureDirty = DirtyFlags.HasFlag(LayoutDirtyFlags.Measure);

        DirtyFlags |= LayoutDirtyFlags.Measure | LayoutDirtyFlags.Arrange;
        Root.InvalidateLayout();

        if (!wasMeasureDirty)
            LayoutSlot?.Parent.InvalidateMeasure();
    }

    public void InvalidateArrange()
    {
        var wasArrangeDirty = DirtyFlags.HasFlag(LayoutDirtyFlags.Arrange);

        DirtyFlags |= LayoutDirtyFlags.Arrange;
        Root.InvalidateLayout();

        if (!wasArrangeDirty)
            LayoutSlot?.Parent.InvalidateArrange();
    }

    protected static Scene GetSceneFrom(IUiRoot root)
    {
        return root.Scene;
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
