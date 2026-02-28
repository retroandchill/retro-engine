// // @file IAssetDecoder.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Strings;

namespace RetroEngine.Assets;

public readonly record struct AssetDecodeContext(AssetPath Path);

public interface IAssetDecoder
{
    Name AssetType { get; }

    Asset Decode(AssetDecodeContext context, Stream stream);
}
