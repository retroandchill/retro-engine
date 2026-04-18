// // @file BrotliDecompressor.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Diagnostics;
using System.IO.Compression;
using MagicArchive.Utilities;

namespace MagicArchive.Compression;

public struct BrotliDecompressor : IDisposable
{
    private ReusableReadOnlySequenceBuilder? _sequenceBuilder;

    public ReadOnlySequence<byte> Decompress(ReadOnlySpan<byte> compressedSpan)
    {
        return Decompress(compressedSpan, out _);
    }

    public ReadOnlySequence<byte> Decompress(ReadOnlySpan<byte> compressedSpan, out int consumed)
    {
        if (_sequenceBuilder is not null)
        {
            ArchiveSerializationException.ThrowAlreadyDecompressed();
        }

        _sequenceBuilder = ReusableReadOnlySequenceBuilderPool.Rent();
        var decoder = new BrotliDecoder();
        try
        {
            var status = OperationStatus.DestinationTooSmall;
            DecompressCore(ref status, ref decoder, compressedSpan, out consumed);
            if (status == OperationStatus.NeedMoreData)
                ArchiveSerializationException.ThrowCompressionFailed(status);
        }
        finally
        {
            decoder.Dispose();
        }

        return _sequenceBuilder.Build();
    }

    public ReadOnlySequence<byte> Decompress(ReadOnlySequence<byte> compressedSequence)
    {
        return Decompress(compressedSequence, out _);
    }

    public ReadOnlySequence<byte> Decompress(ReadOnlySequence<byte> compressedSequence, out int consumed)
    {
        if (_sequenceBuilder is not null)
        {
            ArchiveSerializationException.ThrowAlreadyDecompressed();
        }

        _sequenceBuilder = ReusableReadOnlySequenceBuilderPool.Rent();
        var decoder = new BrotliDecoder();
        try
        {
            var status = OperationStatus.DestinationTooSmall;
            consumed = 0;
            foreach (var item in compressedSequence)
            {
                DecompressCore(ref status, ref decoder, item.Span, out var bytesConsumed);
                consumed += bytesConsumed;
            }

            if (status == OperationStatus.NeedMoreData)
                ArchiveSerializationException.ThrowCompressionFailed(status);
        }
        finally
        {
            decoder.Dispose();
        }

        return _sequenceBuilder.Build();
    }

    private void DecompressCore(
        ref OperationStatus status,
        ref BrotliDecoder decoder,
        ReadOnlySpan<byte> source,
        out int consumed
    )
    {
        Debug.Assert(_sequenceBuilder is not null);
        consumed = 0;

        byte[]? buffer = null;
        status = OperationStatus.DestinationTooSmall;
        var nextCapacity = source.Length;
        while (status == OperationStatus.DestinationTooSmall)
        {
            if (buffer is null)
            {
                nextCapacity = GetDoubleCapacity(nextCapacity);
                buffer = ArrayPool<byte>.Shared.Rent(nextCapacity);
            }

            status = decoder.Decompress(source, buffer, out var bytesConsumed, out var bytesWritten);
            consumed += bytesConsumed;

            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (status)
            {
                case OperationStatus.InvalidData:
                    ArchiveSerializationException.ThrowCompressionFailed(status);
                    break;
                case OperationStatus.NeedMoreData:
                {
                    if (bytesWritten > 0)
                    {
                        _sequenceBuilder.Add(buffer.AsMemory(0, bytesWritten), true);
                    }

                    if (bytesConsumed > 0)
                    {
                        source = source[bytesConsumed..];
                    }

                    if (source.Length != 0)
                        ArchiveSerializationException.ThrowCompressionFailed();

                    return;
                }
            }

            if (bytesConsumed > 0)
                source = source[bytesConsumed..];

            if (bytesWritten <= 0)
                continue;
            _sequenceBuilder.Add(buffer.AsMemory(0, bytesWritten), true);
            buffer = null;
        }
    }

    private static int GetDoubleCapacity(int length)
    {
        var newCapacity = unchecked(length * 2);
        if ((uint)newCapacity > int.MaxValue)
            newCapacity = int.MaxValue;
        return Math.Max(newCapacity, 4096);
    }

    public void Dispose()
    {
        if (_sequenceBuilder is null)
            return;

        ReusableReadOnlySequenceBuilderPool.Return(_sequenceBuilder);
        _sequenceBuilder = null;
    }
}
