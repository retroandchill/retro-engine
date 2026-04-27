// // @file MappedAssetDecoder.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Immutable;
using System.Text.Json;
using MagicArchive;
using RetroEngine.Portable.Strings;
using Zomp.SyncMethodGenerator;

namespace RetroEngine.Assets.Decoders;

public abstract partial class MappedAssetDecoder<TAsset, TDto> : IAssetDecoder
    where TAsset : class
{
    public abstract Name AssetType { get; }
    public abstract ImmutableArray<string> Extensions { get; }

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

    private static TDto DecodeDto(AssetStorageType type, scoped ReadOnlySpan<byte> source)
    {
        return type switch
        {
            AssetStorageType.File => JsonSerializer.Deserialize<TDto>(source)
                ?? throw new InvalidOperationException("JSON deserialization failed"),
            AssetStorageType.Packaged => ArchiveSerializer.Deserialize<TDto>(source)
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

        var sourceAsset = DecodeDto(sourceType, source);
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

    protected abstract TAsset Convert(TDto dto);

    protected virtual ValueTask<TAsset> ConvertAsync(TDto dto, CancellationToken cancellationToken = default)
    {
        return !cancellationToken.IsCancellationRequested
            ? new ValueTask<TAsset>(Convert(dto))
            : ValueTask.FromCanceled<TAsset>(cancellationToken);
    }
}
