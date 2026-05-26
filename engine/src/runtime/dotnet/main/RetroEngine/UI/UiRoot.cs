// @file RootWidget.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Core.Drawing;
using RetroEngine.Core.Math;
using RetroEngine.Tickables;
using RetroEngine.World;

namespace RetroEngine.UI;

public interface IUiRoot : IDisposable
{
    bool Disposed { get; }

    internal Scene Scene { get; }

    AnchorData ScreenLayout { get; set; }

    int ZOrder { get; set; }

    Widget? Content { get; set; }

    void InvalidateLayout();

    void UpdateLayout();
}

public sealed class UiRoot : IUiRoot, ITickable
{
    public bool Disposed { get; private set; }

    private readonly IUiManager _uiManager;
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

            field = value;
            InvalidateLayout();
        }
    }

    internal UiRoot(IUiManager uiManager, TickManager tickManager)
    {
        _uiManager = uiManager;
        Scene = new Scene();
        _viewport = new Viewport { Scene = Scene };
        _viewport.ScreenRectChanged += _ => InvalidateLayout();
        _tickHandle = new TickHandle(this, tickManager);
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
        _uiManager.DestroyRoot(this);
    }
}
