// @file RootWidget.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Core.Drawing;
using RetroEngine.Core.Math;
using RetroEngine.World;

namespace RetroEngine.UI;

public sealed class UiRoot : IDisposable
{
    private bool _disposed;

    private readonly Scene _scene;
    private readonly Viewport _viewport;
    private bool _layoutDirty = true;

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

            field = value;
            InvalidateLayout();
        }
    }

    public UiRoot(SceneManager sceneManager, ViewportManager viewportManager)
    {
        _scene = new Scene(sceneManager);
        _viewport = new Viewport(viewportManager) { Scene = _scene };
        _viewport.ScreenRectChanged += _ => InvalidateLayout();
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

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _scene.Dispose();
        _viewport.Dispose();
    }
}
