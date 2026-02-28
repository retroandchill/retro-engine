// // @file TextureDecoder.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Strings;

namespace RetroEngine.Assets.Textures;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public class TextureDecoder : IAssetDecoder
{
    public Name AssetType => Texture.TypeName;

    public Asset Decode(AssetDecodeContext context, Stream stream)
    {
        throw new NotImplementedException();
    }
}
