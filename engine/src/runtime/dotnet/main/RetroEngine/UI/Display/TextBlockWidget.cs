// @file TextBlockWidget.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Core.Drawing;
using RetroEngine.Core.Math;
using RetroEngine.Portable.Localization;
using RetroEngine.Rendering.Text;
using RetroEngine.World;

namespace RetroEngine.UI.Display;

public sealed class TextBlockWidget : Widget
{
    private readonly TextBlock _textBlock;

    public Text Text
    {
        get
        {
            ThrowIfDisposed();
            return _textBlock.Text;
        }
        set
        {
            ThrowIfDisposed();
            if (_textBlock.Text == value)
                return;

            _textBlock.Text = value;
            InvalidateMeasure();
        }
    }

    public Font? Font
    {
        get
        {
            ThrowIfDisposed();
            return _textBlock.Font;
        }
        set
        {
            ThrowIfDisposed();
            if (ReferenceEquals(value, _textBlock.Font))
                return;

            _textBlock.Font = value;
            InvalidateMeasure();
        }
    }

    public uint FontSize
    {
        get
        {
            ThrowIfDisposed();
            return _textBlock.FontSize;
        }
        set
        {
            ThrowIfDisposed();
            if (_textBlock.FontSize == value)
                return;

            _textBlock.FontSize = value;
            InvalidateMeasure();
        }
    }

    public Color Color
    {
        get
        {
            ThrowIfDisposed();
            return _textBlock.Tint;
        }
        set
        {
            ThrowIfDisposed();
            _textBlock.Tint = value;
        }
    }

    public TextBlockWidget(IUiRoot root)
        : base(root, new TextBlock(GetSceneFrom(root)))
    {
        _textBlock = (TextBlock)SceneObject!;
    }

    protected override Vector2F ComputeDesiredSize(Vector2F availableSize)
    {
        // TODO: Actually measure the text block size
        return Vector2F.Zero;
    }
}
