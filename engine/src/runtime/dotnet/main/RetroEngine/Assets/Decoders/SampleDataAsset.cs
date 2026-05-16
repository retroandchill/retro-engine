// @file SampleDataAsset.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MagicArchive;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Assets.Decoders;

[Archivable]
public sealed partial record SampleDataAsset
{
    public static readonly Name AssetType = new("SampleDataAsset");

    public bool BoolProperty { get; init; }

    public int IntProperty { get; init; }

    public float FloatProperty { get; init; }

    public Name NameProperty { get; init; }

    public string StringProperty { get; init; } = "";

    public Text TextProperty { get; init; }
}
