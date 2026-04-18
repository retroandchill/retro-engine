// // @file BrotliCompressor.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MagicArchive.Utilities;

namespace MagicArchive.Compression;

public struct BrotliCompressor : IBufferWriter<byte>, IDisposable
{
    private ReusableLinkedArrayBufferWriter? _bufferWriter;
    private readonly int _quality;
    private readonly int _windowSize;

    public BrotliCompressor()
        : this(CompressionLevel.Fastest) { }

    public BrotliCompressor(CompressionLevel compressionLevel, int window = BrotliUtils.WindowBitsDefault)
        : this(BrotliUtils.GetQualityFromCompressionLevel(compressionLevel), window) { }

    public BrotliCompressor(int quality = 1, int window = 22)
    {
        _bufferWriter = ReusableLinkedArrayBufferWriterPool.Rent();
        _quality = quality;
        _windowSize = window;
    }

    void IBufferWriter<byte>.Advance(int count)
    {
        ThrowIfDisposed();
        _bufferWriter.Advance(count);
    }

    Memory<byte> IBufferWriter<byte>.GetMemory(int sizeHint)
    {
        ThrowIfDisposed();
        return _bufferWriter.GetMemory(sizeHint);
    }

    Span<byte> IBufferWriter<byte>.GetSpan(int sizeHint)
    {
        ThrowIfDisposed();
        return _bufferWriter.GetSpan(sizeHint);
    }

    public int GetMaxCompressedLength()
    {
        ThrowIfDisposed();
        return BrotliUtils.BrotliEncoderMaxCompressedSize(_bufferWriter.TotalWritten);
    }

    public byte[] ToArray()
    {
        ThrowIfDisposed();

        using var encoder = new BrotliEncoder(_quality, _windowSize);
        var maxLength = BrotliUtils.BrotliEncoderMaxCompressedSize(_bufferWriter.TotalWritten);

        var finalBuffer = ArrayPool<byte>.Shared.Rent(maxLength);
        try
        {
            var writtenCount = 0;
            var destination = finalBuffer.AsSpan(0, maxLength);
            foreach (var source in _bufferWriter)
            {
                var status = encoder.Compress(
                    source.Span,
                    destination,
                    out var bytesConsumed,
                    out var bytesWritten,
                    false
                );
                if (status != OperationStatus.Done)
                    ArchiveSerializationException.ThrowFailedEncoding(status);

                if (bytesConsumed != source.Span.Length)
                    ArchiveSerializationException.ThrowCompressionFailed();

                if (bytesWritten <= 0)
                    continue;
                destination = destination[bytesWritten..];
                writtenCount += bytesWritten;
            }

            encoder.Compress([], destination, out _, out var written, true);
            writtenCount += written;

            return finalBuffer.AsSpan(0, writtenCount).ToArray();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(finalBuffer);
        }
    }

    public void CopyTo<TBufferWriter>(in TBufferWriter bufferWriter)
        where TBufferWriter : IBufferWriter<byte>
    {
        ThrowIfDisposed();

        var encoder = new BrotliEncoder(_quality, _windowSize);
        try
        {
            var writtenNotAdvanced = 0;
            foreach (var item in _bufferWriter)
            {
                writtenNotAdvanced = CompressCore(
                    ref encoder,
                    item.Span,
                    ref Unsafe.AsRef(in bufferWriter),
                    null,
                    false
                );
            }

            var finalBlockLength = (writtenNotAdvanced == 0) ? null : (int?)(writtenNotAdvanced + 10);
            CompressCore(ref encoder, [], ref Unsafe.AsRef(in bufferWriter), finalBlockLength, true);
        }
        finally
        {
            encoder.Dispose();
        }
    }

    public async ValueTask CopyToAsync(
        Stream stream,
        int bufferSize = 65535,
        CancellationToken cancellationToken = default
    )
    {
        ThrowIfDisposed();

        using var encoder = new BrotliEncoder(_quality, _windowSize);

        var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        try
        {
            foreach (var item in _bufferWriter)
            {
                var source = item;
                var lastResult = OperationStatus.DestinationTooSmall;
                while (lastResult == OperationStatus.DestinationTooSmall)
                {
                    lastResult = encoder.Compress(
                        source.Span,
                        buffer,
                        out var bytesConsumed,
                        out var bytesWritten,
                        false
                    );
                    if (lastResult == OperationStatus.InvalidData)
                        ArchiveSerializationException.ThrowCompressionFailed();

                    if (bytesWritten > 0)
                    {
                        await stream.WriteAsync(buffer.AsMemory(0, bytesWritten), cancellationToken);
                    }

                    if (bytesConsumed > 0)
                    {
                        source = source[bytesConsumed..];
                    }
                }
            }

            var finalStatus = OperationStatus.DestinationTooSmall;
            while (finalStatus == OperationStatus.DestinationTooSmall)
            {
                finalStatus = encoder.Compress([], buffer, out _, out var written, true);
                if (written > 0)
                {
                    await stream.WriteAsync(buffer.AsMemory(0, written), cancellationToken);
                }
            }

            if (finalStatus != OperationStatus.Done)
            {
                ArchiveSerializationException.ThrowCompressionFailed(finalStatus);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public void CopyTo<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer)
        where TBufferWriter : IBufferWriter<byte>
    {
        ThrowIfDisposed();

        var encoder = new BrotliEncoder(_quality, _windowSize);
        try
        {
            var bytesWritten = 0;
            foreach (var item in _bufferWriter)
            {
                var span = item.Span;
                if (span.Length <= 0)
                    continue;
                bytesWritten += CompressCore(ref encoder, span, ref writer, initialLength: null, isFinalBlock: false);
            }

            // call BrotliEncoderOperation.Finish
            var finalBlockMaxLength = BrotliUtils.BrotliEncoderMaxCompressedSize(bytesWritten) - bytesWritten;
            CompressCore(
                ref encoder,
                ReadOnlySpan<byte>.Empty,
                ref writer,
                initialLength: finalBlockMaxLength,
                isFinalBlock: true
            );
        }
        finally
        {
            encoder.Dispose();
        }
    }

    private static int CompressCore<TBufferWriter>(
        ref BrotliEncoder encoder,
        ReadOnlySpan<byte> source,
        ref TBufferWriter writer,
        int? initialLength = null,
        bool isFinalBlock = false
    )
        where TBufferWriter : IBufferWriter<byte>
    {
        var writtenNotAdvanced = 0;

        var lastResult = OperationStatus.DestinationTooSmall;
        while (lastResult == OperationStatus.DestinationTooSmall)
        {
            var dest = writer.GetSpan(initialLength ?? source.Length);

            lastResult = encoder.Compress(source, dest, out var bytesConsumed, out var bytesWritten, isFinalBlock);
            writtenNotAdvanced += bytesConsumed;

            if (lastResult == OperationStatus.InvalidData)
                ArchiveSerializationException.ThrowCompressionFailed();

            if (bytesWritten > 0)
            {
                writer.Advance(bytesWritten);
                writtenNotAdvanced = 0;
            }

            if (bytesConsumed > 0)
            {
                source = source[bytesConsumed..];
            }
        }

        return writtenNotAdvanced;
    }

    private static int CompressCore<TBufferWriter>(
        ref BrotliEncoder encoder,
        ReadOnlySpan<byte> source,
        ref ArchiveWriter<TBufferWriter> writer,
        int? initialLength = null,
        bool isFinalBlock = false
    )
        where TBufferWriter : IBufferWriter<byte>
    {
        var totalWritten = 0;
        var lastResult = OperationStatus.DestinationTooSmall;

        var destLength = initialLength ?? BrotliUtils.BrotliEncoderMaxCompressedSize(source.Length);
        while (lastResult == OperationStatus.DestinationTooSmall)
        {
            ref var spanRef = ref writer.GetSpanReference(destLength);
            var dest = MemoryMarshal.CreateSpan(ref spanRef, destLength);

            lastResult = encoder.Compress(source, dest, out var bytesConsumed, out var bytesWritten, isFinalBlock);
            totalWritten += bytesWritten;

            if (lastResult == OperationStatus.InvalidData)
                ArchiveSerializationException.ThrowCompressionFailed();

            if (bytesWritten > 0)
            {
                writer.Advance(bytesWritten);
                destLength = BrotliUtils.BrotliEncoderMaxCompressedSize(totalWritten);
            }

            if (bytesConsumed > 0)
            {
                source = source[bytesConsumed..];
            }
        }

        return totalWritten;
    }

    public void Dispose()
    {
        if (_bufferWriter is null)
            return;

        _bufferWriter.Reset();
        ReusableLinkedArrayBufferWriterPool.Return(_bufferWriter);
        _bufferWriter = null;
    }

    [MemberNotNull(nameof(_bufferWriter))]
    private void ThrowIfDisposed()
    {
        if (_bufferWriter is null)
            throw new ObjectDisposedException(nameof(BrotliCompressor));
    }
}
