// @file ScreenLayout.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Core.Math;
using RetroEngine.Utilities;

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
        var (x, width) = CalculateAxis(
            size.X,
            Anchors.Minimum.X,
            Anchors.Maximum.X,
            Offsets.Left,
            Offsets.Right,
            Alignment.X
        );
        var (y, height) = CalculateAxis(
            size.Y,
            Anchors.Minimum.Y,
            Anchors.Maximum.Y,
            Offsets.Top,
            Offsets.Bottom,
            Alignment.Y
        );
        return new RectF(x, y, width, height);
    }

    private static (float Position, float Size) CalculateAxis(
        float parentSize,
        float anchorMin,
        float anchorMax,
        float offsetMin,
        float offsetMax,
        float alignment
    )
    {
        alignment = System.Math.Clamp(alignment, 0f, 1f);

        if (System.Math.IsNearlyEqual(anchorMin, anchorMax))
        {
            var size = offsetMax;
            var anchorPosition = parentSize * anchorMin;
            var position = anchorPosition + offsetMin - size * alignment;

            return (position, size);
        }
        else
        {
            var min = parentSize * anchorMin + offsetMin;
            var max = parentSize * anchorMax - offsetMax;
            var size = max - min;
            var position = min - size * alignment;

            return (position, size);
        }
    }
}
