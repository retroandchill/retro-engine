// // @file StringTableDecoder.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Microsoft.Extensions.Options;
using RetroEngine.Portable.Localization.StringTables;

namespace RetroEngine.Assets.Decoders;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public class StringTableDecoder(IOptions<JsonSerializerOptions> options)
    : MappedAssetDecoder<StringTable, StringTableDto>(["loctable"], options)
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
