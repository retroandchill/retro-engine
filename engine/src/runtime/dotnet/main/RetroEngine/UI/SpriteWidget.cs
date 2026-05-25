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
        get => _sprite.Texture;
        set => _sprite.Texture = value;
    }

    public Color Color
    {
        get => _sprite.Tint;
        set => _sprite.Tint = value;
    }

    public UVs UVs
    {
        get => _sprite.UVs;
        set => _sprite.UVs = value;
    }

    public SpriteDrawMode DrawMode
    {
        get => _sprite.DrawMode;
        set => _sprite.DrawMode = value;
    }

    public Margin Margin
    {
        get => _sprite.Margin;
        set => _sprite.Margin = value;
    }

    public SpriteWidget(Scene scene)
        : base(new Sprite(scene))
    {
        _sprite = (Sprite)SceneObject!;
    }
}
