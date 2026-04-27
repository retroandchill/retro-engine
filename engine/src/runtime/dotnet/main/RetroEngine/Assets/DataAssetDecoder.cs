// // @file DataAssetDecoder.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Immutable;
using System.Text.Json;
using MagicArchive;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Assets;

public abstract class DataAssetDecoder<T> : IAssetDecoder
    where T : class
{
    public abstract Name AssetType { get; }
    public abstract ImmutableArray<string> Extensions { get; }

    public object Decode(AssetStorageType type, scoped ReadOnlySpan<byte> source)
    {
        return DecodeInternal(type, source);
    }

    private static T DecodeInternal(AssetStorageType type, scoped ReadOnlySpan<byte> source)
    {
        return type switch
        {
            AssetStorageType.File => JsonSerializer.Deserialize<T>(source)
                ?? throw new InvalidOperationException("JSON deserialization failed"),
            AssetStorageType.Packaged => ArchiveSerializer.Deserialize<T>(source)
                ?? throw new InvalidOperationException("Archive deserialization failed"),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }

    public void Encode<TBufferWriter>(
        AssetStorageType sourceType,
        AssetStorageType destType,
        scoped ReadOnlySpan<byte> source,
        in TBufferWriter writer
    )
        where TBufferWriter : IBufferWriter<byte>
    {
        if (sourceType == destType)
        {
            IAssetDecoder.EncodeAsSource(source, writer);
            return;
        }

        var sourceAsset = DecodeInternal(sourceType, source);
        switch (sourceType)
        {
            case AssetStorageType.File:
                var utf8JsonWriter = new Utf8JsonWriter(writer);
                JsonSerializer.Serialize(utf8JsonWriter, sourceAsset);
                break;
            case AssetStorageType.Packaged:
                ArchiveSerializer.Serialize(writer, sourceAsset);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(sourceType), sourceType, null);
        }
    }
}
