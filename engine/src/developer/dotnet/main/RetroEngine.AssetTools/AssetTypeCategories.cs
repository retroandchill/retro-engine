// // @file AssetTypeCategory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Localization;

namespace RetroEngine.AssetTools;

[Flags]
public enum AssetTypeCategories : uint
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
    LastUser = (uint)1 << 31,
}

public static class KnownAssetTypes
{
    private const string AssetCategoryNamespace = "AssetCategories";
    public static readonly Text Audio = Text.AsLocalizable(AssetCategoryNamespace, "Audio", "Audio");
    public static readonly Text Data = Text.AsLocalizable(AssetCategoryNamespace, "Data", "Data");
    public static readonly Text Graphics = Text.AsLocalizable(AssetCategoryNamespace, "Graphics", "Graphics");
    public static readonly Text Gameplay = Text.AsLocalizable(AssetCategoryNamespace, "Gameplay", "Gameplay");
    public static readonly Text Scripting = Text.AsLocalizable(AssetCategoryNamespace, "Scripting", "Scripting");
    public static readonly Text UI = Text.AsLocalizable(AssetCategoryNamespace, "UI", "UI");
    public static readonly Text Misc = Text.AsLocalizable(AssetCategoryNamespace, "Misc", "Misc");
}
