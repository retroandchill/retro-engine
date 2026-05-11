// // @file AssetTypeCategory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.AssetTools;

[Flags]
public enum AssetTypeCategory
{
    None = 0,
    Audio = 1 << 0,
    Data = 1 << 1,
    Graphics = 1 << 2,
    Gameplay = 1 << 3,
    Scripting = 1 << 4,
    UI = 1 << 5,
    Misc = 1 << 6,

    FirstUser = 1 << 7,
    LastUser = 1 << 31,
}
