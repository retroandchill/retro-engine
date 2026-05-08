// // @file SampleDataAssetDecoder.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Assets.Decoders;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public sealed class SampleDataAssetDecoder : DataAssetDecoder<SampleDataAsset>
{
    public override Name AssetType => SampleDataAsset.AssetType;

    private static readonly ImmutableArray<string> ExtensionsArray = ["sample"];
    public override ImmutableArray<string> Extensions => ExtensionsArray;
}
