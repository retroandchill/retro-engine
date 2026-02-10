// // @file ScreenLayout.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Core.Math;

namespace RetroEngine.Core.Drawing;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Margin(float Left, float Top, float Right, float Bottom);

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Anchors(Vector2F Minimum, Vector2F Maximum);

[StructLayout(LayoutKind.Sequential)]
public readonly record struct ScreenLayout(Margin Offsets, Anchors Anchors, Vector2F Alignment);
