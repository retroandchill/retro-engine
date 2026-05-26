// @file RootWidget.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Core.Drawing;
using RetroEngine.Core.Math;
using RetroEngine.Tickables;
using RetroEngine.World;

namespace RetroEngine.UI;

public sealed class UiRoot : ITickable, IDisposable
{
    public bool Disposed { get; private set; }

    private readonly TickHandle _tickHandle;
    public Scene Scene { get; }
    private readonly Viewport _viewport;
    private bool _layoutDirty = true;

    public TickGroup TickGroup => TickGroup.UiLayout;
    public bool TickEnabled => true;

    public AnchorData ScreenLayout
    {
        get
        {
            ThrowIfDisposed();
            return _viewport.ScreenLayout;
        }
        set
        {
            ThrowIfDisposed();
            _viewport.ScreenLayout = value;
        }
    }

    public int ZOrder
    {
        get
        {
            ThrowIfDisposed();
            return _viewport.ZOrder;
        }
        set
        {
            ThrowIfDisposed();
            _viewport.ZOrder = value;
        }
    }

    public Widget? Content
    {
        get;
        set
        {
            ThrowIfDisposed();
            if (ReferenceEquals(field, value))
                return;

            field?.Root = null;
            field = value;
            field?.Root = this;
            InvalidateLayout();
        }
    }

    public UiRoot()
    {
        Scene = new Scene();
        _viewport = new Viewport { Scene = Scene };
        _viewport.ScreenRectChanged += _ => InvalidateLayout();
        _tickHandle = new TickHandle(this);
    }

    public void InvalidateLayout()
    {
        ThrowIfDisposed();
        _layoutDirty = true;
    }

    public void UpdateLayout()
    {
        ThrowIfDisposed();
        if (!_layoutDirty || Content is null)
            return;

        var screenRect = _viewport.ScreenRect;
        var size = new Vector2F(screenRect.Width, screenRect.Height);
        var rect = new RectF(0, 0, size.X, size.Y);

        Content.Measure(size);
        Content.Arrange(rect);

        _layoutDirty = false;
    }

    public void Tick(float deltaTime)
    {
        UpdateLayout();
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
    }

    public void Dispose()
    {
        if (Disposed)
            return;

        Disposed = true;
        Scene.Dispose();
        _viewport.Dispose();
        _tickHandle.Dispose();
    }
}
