// @file DataAssetDecoder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Immutable;
using System.Text.Json;
using MagicArchive;
using Microsoft.Extensions.Options;

namespace RetroEngine.Assets.Decoders;

public abstract class DataAssetDecoder<T>(ImmutableArray<string> extensions, IOptions<JsonSerializerOptions> options)
    : IAssetDecoder,
        IAssetEncoder<T>
    where T : class
{
    private readonly JsonSerializerOptions _options = options.Value;

    public Type AssetType => typeof(T);
    public ImmutableArray<string> Extensions { get; } = extensions;

    public object Decode(AssetStorageType type, scoped ReadOnlySpan<byte> source)
    {
        return DecodeInternal(type, source);
    }

    private T DecodeInternal(AssetStorageType type, scoped ReadOnlySpan<byte> source)
    {
        return type switch
        {
            AssetStorageType.File => JsonSerializer.Deserialize<T>(source, _options)
                ?? throw new InvalidOperationException("JSON deserialization failed"),
            AssetStorageType.Packaged => ArchiveSerializer.Deserialize<T>(source)
                ?? throw new InvalidOperationException("Archive deserialization failed"),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }

    public void Encode<TBufferWriter>(
        AssetStorageType sourceType,
        object asset,
        ReadOnlySpan<char> extension,
        in TBufferWriter writer
    )
        where TBufferWriter : IBufferWriter<byte>
    {
        if (asset is not T sourceAsset)
            throw new ArgumentException($"Asset must be of type {typeof(T)}", nameof(asset));

        Encode(sourceType, sourceAsset, extension, writer);
    }

    public void Encode<TBufferWriter>(
        AssetStorageType sourceType,
        T sourceAsset,
        ReadOnlySpan<char> extension,
        in TBufferWriter writer
    )
        where TBufferWriter : IBufferWriter<byte>
    {
        switch (sourceType)
        {
            case AssetStorageType.File:
                var utf8JsonWriter = new Utf8JsonWriter(
                    writer,
                    new JsonWriterOptions
                    {
                        Indented = _options.WriteIndented,
                        Encoder = _options.Encoder,
                        IndentCharacter = _options.IndentCharacter,
                        IndentSize = _options.IndentSize,
                        NewLine = _options.NewLine,
                    }
                );
                JsonSerializer.Serialize(utf8JsonWriter, sourceAsset, _options);
                break;
            case AssetStorageType.Packaged:
                ArchiveSerializer.Serialize(writer, sourceAsset);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(sourceType), sourceType, null);
        }
    }

    public void Transcode<TBufferWriter>(
        AssetStorageType sourceType,
        AssetStorageType destType,
        scoped ReadOnlySpan<byte> source,
        ReadOnlySpan<char> extension,
        in TBufferWriter writer
    )
        where TBufferWriter : IBufferWriter<byte>
    {
        if (sourceType == destType)
        {
            IAssetTranscoder.EncodeAsSource(source, writer);
            return;
        }

        var sourceAsset = DecodeInternal(sourceType, source);
        Encode(destType, sourceAsset, extension, writer);
    }
}
