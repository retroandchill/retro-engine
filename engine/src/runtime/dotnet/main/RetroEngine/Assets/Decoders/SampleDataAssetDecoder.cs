// // @file SampleDataAssetDecoder.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Assets.Decoders;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public sealed class SampleDataAssetDecoder() : DataAssetDecoder<SampleDataAsset>(["sample"]);
