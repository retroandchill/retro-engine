// // @file BrotliFormatter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MagicArchive.Compression;

public sealed class BrotliFormatter(
    CompressionLevel compressionLevel = CompressionLevel.Fastest,
    int window = BrotliUtils.WindowBitsDefault,
    int decompressionSizeLimit = BrotliFormatter.DefaultDecompressionSizeLimit
) : ArchiveFormatter<byte[]>
{
    internal const int DefaultDecompressionSizeLimit = 1024 * 1024 + 128; // 128MB

    public static readonly BrotliFormatter Default = new();

    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in byte[]? value)
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        if (value.Length == 0)
        {
            writer.WriteCollectionHeader(0);
            return;
        }

        var quality = BrotliUtils.GetQualityFromCompressionLevel(compressionLevel);
        using var encoder = new BrotliEncoder(quality, window);

        var maxLength = BrotliUtils.BrotliEncoderMaxCompressedSize(value.Length);
        const int headerSize = 8;
        ref var head = ref writer.GetSpanReference(maxLength + headerSize);

        var dest = MemoryMarshal.CreateSpan(ref Unsafe.Add(ref head, headerSize), maxLength);
        var status = encoder.Compress(value.AsSpan(), dest, out var bytesConsumed, out var bytesWritten, true);
        if (status != OperationStatus.Done)
            ArchiveSerializationException.ThrowCompressionFailed(status);

        if (bytesConsumed != value.Length)
            ArchiveSerializationException.ThrowCompressionFailed();

        Unsafe.WriteUnaligned(ref head, value.Length);
        Unsafe.WriteUnaligned(ref Unsafe.Add(ref head, sizeof(int)), bytesWritten);

        writer.Advance(bytesWritten + headerSize);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref byte[]? value)
    {
        var uncompressedLength = reader.ReadBlittable<int>();

        reader.UnsafeReadBlittableSpanView<byte>(out var isNull, out var compressedBuffer);

        if (isNull)
        {
            value = null;
            return;
        }

        if (compressedBuffer.Length == 0)
        {
            value = [];
            return;
        }

        if (decompressionSizeLimit < uncompressedLength)
            ArchiveSerializationException.ThrowDecompressionSizeLimitExceeded(
                decompressionSizeLimit,
                uncompressedLength
            );

        if (value is null || value.Length != uncompressedLength)
            value = new byte[uncompressedLength];

        using var decoder = new BrotliDecoder();

        var status = decoder.Decompress(compressedBuffer, value, out var bytesConsumed, out var bytesWritten);
        if (status != OperationStatus.Done)
            ArchiveSerializationException.ThrowCompressionFailed(status);

        if (bytesConsumed != compressedBuffer.Length || bytesWritten != value.Length)
            ArchiveSerializationException.ThrowCompressionFailed();
    }
}

public sealed class BrotliFormatter<T>(
    CompressionLevel compressionLevel = CompressionLevel.Fastest,
    int window = BrotliUtils.WindowBitsDefault
) : ArchiveFormatter<T>
{
    public static readonly BrotliFormatter<T> Default = new();

    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in T? value)
    {
        var compressor = new BrotliCompressor(compressionLevel, window);
        try
        {
            var coWriter = ArchiveWriter.Create(ref compressor, writer.State);

            coWriter.WriteValue(value);
            coWriter.Flush();

            compressor.CopyTo(ref writer);
        }
        finally
        {
            compressor.Dispose();
        }
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref T? value)
    {
        using var decompressor = new BrotliDecompressor();
        reader.GetRemainingSource(out var singleSource, out var remainingSource);
        var decompressedSource =
            singleSource.Length != 0
                ? decompressor.Decompress(singleSource, out var consumed)
                : decompressor.Decompress(remainingSource, out consumed);
        using var coReader = new ArchiveReader(decompressedSource, reader.State);
        coReader.ReadValue(ref value);
        coReader.Advance(consumed);
    }
}
