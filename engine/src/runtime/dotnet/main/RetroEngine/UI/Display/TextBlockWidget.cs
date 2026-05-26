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
    private TextBlock? _textBlock;

    public Text Text
    {
        get;
        set
        {
            ThrowIfDisposed();
            if (field == value)
                return;

            field = value;
            _textBlock?.Text = value;
            InvalidateMeasure();
        }
    }

    public Font? Font
    {
        get;
        set
        {
            ThrowIfDisposed();
            if (ReferenceEquals(value, field))
                return;

            field = value;
            _textBlock?.Font = value;
            InvalidateMeasure();
        }
    }

    public uint FontSize
    {
        get;
        set
        {
            ThrowIfDisposed();
            if (field == value)
                return;

            field = value;
            _textBlock?.FontSize = value;
            InvalidateMeasure();
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
            _textBlock?.Tint = value;
        }
    }

    protected override void OnAttached(UiRoot root)
    {
        base.OnAttached(root);
        _textBlock = new TextBlock(root.Scene)
        {
            Text = Text,
            Font = Font,
            FontSize = FontSize,
            Tint = Color,
        };
        SceneObject = _textBlock;
    }

    protected override void OnDetached()
    {
        base.OnDetached();
        _textBlock!.Dispose();
        _textBlock = null;
    }

    protected override Vector2F ComputeDesiredSize(Vector2F availableSize)
    {
        // TODO: Actually measure the text block size
        return Vector2F.Zero;
    }
}
