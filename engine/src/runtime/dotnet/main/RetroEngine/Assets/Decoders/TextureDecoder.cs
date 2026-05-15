// // @file TextureDecoder.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using RetroEngine.Rendering;

namespace RetroEngine.Assets.Decoders;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public class TextureDecoder(RenderBackend renderBackend) : IAssetDecoder, IAssetTranscoder
{
    public Type AssetType => typeof(Texture);

    private static readonly ImmutableArray<string> ExtensionsArray = ["png", "jpg", "jpeg", "bmp"];
    public ImmutableArray<string> Extensions => ExtensionsArray;

    public object Decode(AssetStorageType type, scoped ReadOnlySpan<byte> source)
    {
        return renderBackend.UploadTexture(source);
    }
}
