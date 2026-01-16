// // @file $FILE$
// //
// // @copyright Copyright (c) $[InvalidReference] Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace RetroEngine.Core.Drawing;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Color(float Red, float Green, float Blue, float Alpha = 1.0f);
