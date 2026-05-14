// // @file MappedAssetDecoder.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Immutable;
using System.Text.Json;
using MagicArchive;
using Microsoft.Extensions.Options;
using Zomp.SyncMethodGenerator;

namespace RetroEngine.Assets.Decoders;

public abstract partial class MappedAssetDecoder<TAsset, TDto>(
    ImmutableArray<string> extensions,
    IOptions<JsonSerializerOptions> options
) : IAssetDecoder, IAssetEncoder<TAsset>
    where TAsset : class
{
    private readonly JsonSerializerOptions _options = options.Value;
    public Type AssetType => typeof(TAsset);
    public ImmutableArray<string> Extensions { get; } = extensions;

    [CreateSyncVersion]
    public async ValueTask<object> DecodeAsync(
        AssetStorageType type,
        ReadOnlyMemory<byte> source,
        CancellationToken cancellationToken = default
    )
    {
        var dto = DecodeDto(type, source.Span);
        cancellationToken.ThrowIfCancellationRequested();

        return await ConvertAsync(dto, cancellationToken).ConfigureAwait(false);
    }

    private TDto DecodeDto(AssetStorageType type, scoped ReadOnlySpan<byte> source)
    {
        return type switch
        {
            AssetStorageType.File => JsonSerializer.Deserialize<TDto>(source, _options)
                ?? throw new InvalidOperationException("JSON deserialization failed"),
            AssetStorageType.Packaged => ArchiveSerializer.Deserialize<TDto>(source)
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
        if (asset is not TAsset sourceAsset)
            throw new ArgumentException("Asset must be of type TAsset", nameof(asset));

        Encode(sourceType, sourceAsset, extension, writer);
    }

    public void Encode<TBufferWriter>(
        AssetStorageType sourceType,
        TAsset asset,
        ReadOnlySpan<char> extension,
        in TBufferWriter writer
    )
        where TBufferWriter : IBufferWriter<byte>
    {
        var dto = Convert(asset);
        EncodeDto(sourceType, dto, writer);
    }

    private void EncodeDto<TBufferWriter>(AssetStorageType sourceType, TDto sourceAsset, in TBufferWriter writer)
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

        var sourceAsset = DecodeDto(sourceType, source);
        EncodeDto(sourceType, sourceAsset, writer);
    }

    protected abstract TAsset Convert(TDto dto);

    protected abstract TDto Convert(TAsset asset);

    protected virtual ValueTask<TAsset> ConvertAsync(TDto dto, CancellationToken cancellationToken = default)
    {
        return !cancellationToken.IsCancellationRequested
            ? new ValueTask<TAsset>(Convert(dto))
            : ValueTask.FromCanceled<TAsset>(cancellationToken);
    }
}
