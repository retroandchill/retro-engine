// @file ScreenLayout.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Core.Math;

namespace RetroEngine.Core.Drawing;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Margin(float Left, float Top, float Right, float Bottom)
{
    public Margin(float all)
        : this(all, all, all, all) { }
}

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Anchors(Vector2F Minimum, Vector2F Maximum);

[StructLayout(LayoutKind.Sequential)]
public readonly record struct AnchorData(Margin Offsets, Anchors Anchors, Vector2F Alignment)
{
    public RectF GetBounds(Vector2F size)
    {
        var clampedAlignment = new Vector2F(
            System.Math.Clamp(Alignment.X, 0f, 1f),
            System.Math.Clamp(Alignment.Y, 0f, 1f)
        );

        var relativeX1 = size.X * Anchors.Minimum.X + Offsets.Left;
        var relativeY1 = size.Y * Anchors.Minimum.Y + Offsets.Top;
        var relativeX2 = size.X * Anchors.Maximum.X - Offsets.Right;
        var relativeY2 = size.Y * Anchors.Maximum.Y - Offsets.Bottom;

        var relativeWidth = relativeX2 - relativeX1;
        var relativeHeight = relativeY2 - relativeY1;
        var x = relativeX1 - relativeWidth * clampedAlignment.X;
        var y = relativeY1 - relativeHeight * clampedAlignment.Y;
        return new RectF(x, y, relativeWidth, relativeHeight);
    }
}
