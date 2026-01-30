// @file Vertex.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Core.Math;

namespace RetroEngine.Core.Drawing;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Vertex(Vector2F Position, Vector2F UV);
