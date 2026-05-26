// @file SpriteWidget.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Core.Drawing;
using RetroEngine.Core.Math;
using RetroEngine.Rendering;
using RetroEngine.World;

namespace RetroEngine.UI.Display;

public sealed class SpriteWidget : Widget
{
    private Sprite? _sprite;

    public Texture? Texture
    {
        get;
        set
        {
            ThrowIfDisposed();
            if (ReferenceEquals(value, field))
                return;

            field = value;

            if (value is not null)
                PreferredSize = ComputePreferredSize();

            _sprite?.Texture = value;
        }
    }

    public Color Color
    {
        get;
        set
        {
            ThrowIfDisposed();
            if (field == value)
                return;
            field = value;
            _sprite?.Tint = value;
        }
    } = new(1, 1, 1);

    public UVs UVs
    {
        get;
        set
        {
            ThrowIfDisposed();
            if (field == value)
                return;
            field = value;
            if (_sprite is null)
                return;
            _sprite.UVs = value;

            if (Texture is null)
                return;
            PreferredSize = ComputePreferredSize();
        }
    } = new(Vector2F.Zero, Vector2F.One);

    public SpriteDrawMode DrawMode
    {
        get;
        set
        {
            ThrowIfDisposed();
            if (field == value)
                return;

            field = value;
            _sprite?.DrawMode = value;
        }
    }

    public Margin Margin
    {
        get;
        set
        {
            ThrowIfDisposed();
            if (field == value)
                return;

            field = value;
            _sprite?.Margin = value;
        }
    }

    public Vector2F PreferredSize
    {
        get;
        set
        {
            if (field == value)
                return;

            field = value;
            InvalidateMeasure();
        }
    }

    protected override void OnAttached(UiRoot root)
    {
        base.OnAttached(root);
        _sprite = new Sprite(root.Scene)
        {
            Texture = Texture,
            Tint = Color,
            UVs = UVs,
            DrawMode = DrawMode,
            Margin = Margin,
        };
        SceneObject = _sprite;
    }

    protected override void OnDetached()
    {
        base.OnDetached();
        _sprite!.Dispose();
        _sprite = null;
    }

    protected override Vector2F ComputeDesiredSize(Vector2F availableSize)
    {
        return PreferredSize;
    }

    protected override void ApplyLayoutToScene(RectF finalRect)
    {
        base.ApplyLayoutToScene(finalRect);
        _sprite!.Size = new Vector2F(finalRect.Width, finalRect.Height);
    }

    private Vector2F ComputePreferredSize()
    {
        if (Texture is null)
            return Vector2F.Zero;

        var uvXRange = UVs.Max.X - UVs.Min.X;
        var uvYRange = UVs.Max.Y - UVs.Min.Y;
        return new Vector2F(Texture.Width * uvXRange, Texture.Height * uvYRange);
    }
}
