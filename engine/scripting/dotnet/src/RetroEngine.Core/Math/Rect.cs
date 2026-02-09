// // @file Rect.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Runtime.InteropServices;

namespace RetroEngine.Core.Math;

public interface IRect<out TPosition, out TExtent>
    where TPosition : INumber<TPosition>
    where TExtent : INumber<TExtent>
{
    TPosition X { get; }
    TPosition Y { get; }
    TExtent Width { get; }
    TExtent Height { get; }
}

[StructLayout(LayoutKind.Sequential)]
public readonly record struct RectI(int X, int Y, uint Width, uint Height) : IRect<int, uint>;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct RectF(float X, float Y, float Width, float Height) : IRect<float, float>;
