// // @file IAssetDecoder.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Immutable;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Assets;

public enum AssetStorageType
{
    File,
    Packaged,
}

public interface IAssetDecoder
{
    Name AssetType { get; }

    int Priority => 0;

    ImmutableArray<string> Extensions { get; }

    Asset Decode(AssetStorageType type, AssetPath assetPath, scoped ReadOnlySpan<byte> source);

    void Encode<TBufferWriter>(
        AssetStorageType type,
        Name assetName,
        scoped ReadOnlySpan<byte> source,
        in TBufferWriter writer
    )
        where TBufferWriter : IBufferWriter<byte>
    {
        var destBuffer = writer.GetSpan(source.Length);
        source.CopyTo(destBuffer);
        writer.Advance(source.Length);
    }
}
