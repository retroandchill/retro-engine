// // @file AssetTypeCategory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.AssetTools;

[Flags]
public enum AssetTypeCategory
{
    None = 0,
    Basic = 1 << 0,
    Animation = 1 << 1,
    Materials = 1 << 2,
    Sounds = 1 << 3,
    Physics = 1 << 4,
    UI = 1 << 5,
    Misc = 1 << 6,
    Gameplay = 1 << 7,
    Scripting = 1 << 8,
    Media = 1 << 9,
    Textures = 1 << 10,
    World = 1 << 11,
    FX = 1 << 12,

    FirstUser = 1 << 13,
    LastUser = 1 << 31,
}
