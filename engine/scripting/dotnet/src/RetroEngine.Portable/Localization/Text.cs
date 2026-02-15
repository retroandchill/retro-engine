// // @file Text.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization;

[Flags]
public enum TextIdenticalModeFlags : byte
{
    None = 0,
    DeepCompare = 1 << 0,
    LexicalCompareInvariants = 1 << 1,
}

public struct Text { }
