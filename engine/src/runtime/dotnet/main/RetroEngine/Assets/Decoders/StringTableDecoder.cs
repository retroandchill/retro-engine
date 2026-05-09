// // @file StringTableDecoder.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Localization.StringTables;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Assets.Decoders;

public static class StringTableAssetExtensions
{
    private static readonly Name StringTableAssetType = new("StringTable");

    extension(StringTable)
    {
        public static Name AssetType => StringTableAssetType;
    }
}

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public class StringTableDecoder() : MappedAssetDecoder<StringTable, StringTableDto>(StringTable.AssetType, ["loctable"])
{
    protected override StringTable Convert(StringTableDto dto)
    {
        return StringTable.FromDto(dto);
    }

    protected override StringTableDto Convert(StringTable asset)
    {
        return asset.ToDto();
    }
}
