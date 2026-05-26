// @file SpriteWidget.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Core.Drawing;
using RetroEngine.Core.Math;
using RetroEngine.Rendering;
using RetroEngine.World;

namespace RetroEngine.UI;

public sealed class SpriteWidget : Widget
{
    private readonly Sprite _sprite;

    public Texture? Texture
    {
        get
        {
            ThrowIfDisposed();
            return _sprite.Texture;
        }
        set
        {
            ThrowIfDisposed();
            if (ReferenceEquals(value, _sprite.Texture))
                return;

            _sprite.Texture = value;
            if (value is not null)
                PreferredSize = _sprite.PreferredSize;
        }
    }

    public Color Color
    {
        get
        {
            ThrowIfDisposed();
            return _sprite.Tint;
        }
        set
        {
            ThrowIfDisposed();
            _sprite.Tint = value;
        }
    }

    public UVs UVs
    {
        get
        {
            ThrowIfDisposed();
            return _sprite.UVs;
        }
        set
        {
            ThrowIfDisposed();
            if (_sprite.UVs == value)
                return;

            _sprite.UVs = value;
            if (_sprite.Texture is not null)
                PreferredSize = _sprite.PreferredSize;
        }
    }

    public SpriteDrawMode DrawMode
    {
        get
        {
            ThrowIfDisposed();
            return _sprite.DrawMode;
        }
        set
        {
            ThrowIfDisposed();
            _sprite.DrawMode = value;
        }
    }

    public Margin Margin
    {
        get
        {
            ThrowIfDisposed();
            return _sprite.Margin;
        }
        set
        {
            ThrowIfDisposed();
            _sprite.Margin = value;
        }
    }

    public Vector2F PreferredSize { get; set; }

    public SpriteWidget(Scene scene)
        : base(new Sprite(scene, false))
    {
        _sprite = (Sprite)SceneObject!;
    }

    protected override Vector2F ComputeDesiredSize(Vector2F availableSize)
    {
        return PreferredSize;
    }

    protected override void ApplyLayoutToScene(RectF finalRect)
    {
        base.ApplyLayoutToScene(finalRect);
        _sprite.Size = new Vector2F(finalRect.Width, finalRect.Height);
    }
}
