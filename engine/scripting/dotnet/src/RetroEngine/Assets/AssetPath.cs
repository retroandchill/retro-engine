// // @file AssetPath.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Strings;

namespace RetroEngine.Assets;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct AssetPath(Name PackageName, Name AssetName);
