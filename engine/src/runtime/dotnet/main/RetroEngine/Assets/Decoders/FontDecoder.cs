// @file FontDecoder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using RetroEngine.Rendering.Text;

namespace RetroEngine.Assets.Decoders;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public sealed class FontDecoder(FontService fontService) : IAssetDecoder, IAssetTranscoder
{
    public Type AssetType => typeof(Font);

    private static readonly ImmutableArray<string> ExtensionsArray = ["ttf"];
    public ImmutableArray<string> Extensions => ExtensionsArray;

    public object Decode(AssetStorageType type, scoped ReadOnlySpan<byte> source)
    {
        return fontService.LoadFont(source);
    }

    public async ValueTask<object> DecodeAsync(
        AssetStorageType type,
        ReadOnlyMemory<byte> source,
        CancellationToken cancellationToken = default
    )
    {
        return await fontService.LoadFontAsync(source.Span, cancellationToken);
    }
}
