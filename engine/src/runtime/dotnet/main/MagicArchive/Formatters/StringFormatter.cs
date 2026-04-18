// // @file StringFormatter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LinkDotNet.StringBuilder;
using MagicArchive.Compression;
using MagicArchive.Utilities;

namespace MagicArchive.Formatters;

public sealed class StringFormatter : ArchiveFormatter<string>
{
    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in string? value)
    {
        writer.WriteString(value);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref string? value)
    {
        value = reader.ReadString();
    }
}

public sealed class Utf8StringFormatter : ArchiveFormatter<string>
{
    public static readonly Utf8StringFormatter Default = new();

    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in string? value)
    {
        writer.WriteUtf8(value);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref string? value)
    {
        value = reader.ReadString();
    }
}

public sealed class Utf16StringFormatter : ArchiveFormatter<string>
{
    public static readonly Utf16StringFormatter Default = new();

    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in string? value)
    {
        writer.WriteUtf16(value);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref string? value)
    {
        value = reader.ReadString();
    }
}

public sealed class InternStringFormatter : ArchiveFormatter<string>
{
    public static readonly InternStringFormatter Default = new();

    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in string? value)
    {
        writer.WriteString(value);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref string? value)
    {
        var str = reader.ReadString();
        if (str is null)
        {
            value = null;
            return;
        }

        value = string.Intern(str);
    }
}

public sealed class BrotliStringFormatter(
    CompressionLevel compressionLevel = CompressionLevel.Fastest,
    int window = BrotliUtils.WindowBitsDefault,
    int decompressionSizeLimit = BrotliFormatter.DefaultDecompressionSizeLimit
) : ArchiveFormatter<string>
{
    [ThreadStatic]
    private static StrongBox<int>? _threadStatusConsumedBox;

    public static readonly BrotliStringFormatter Default = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in string? value)
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

        var srcLength = value.Length * sizeof(char);
        var maxLength = BrotliUtils.BrotliEncoderMaxCompressedSize(srcLength);

        const int headerSize = sizeof(int);
        ref var spanRef = ref writer.GetSpanReference(maxLength + headerSize);
        var dest = MemoryMarshal.CreateSpan(ref Unsafe.Add(ref spanRef, headerSize), maxLength);

        char[]? buffer = null;
        try
        {
            ReadOnlySpan<char> bufferSpan;
            if (!writer.IsByteSwapping)
            {
                bufferSpan = value;
            }
            else
            {
                buffer = ArrayPool<char>.Shared.Rent(value.Length);
                for (var i = 0; i < value.Length; i++)
                {
                    buffer[i] = BlittableMarshalling.ReverseEndianness(value[i]);
                }
                bufferSpan = buffer.AsSpan(0, value.Length);
            }

            var status = encoder.Compress(
                MemoryMarshal.AsBytes(bufferSpan),
                dest,
                out var bytesConsumed,
                out var bytesWritten,
                true
            );
            if (status != OperationStatus.Done)
                ArchiveSerializationException.ThrowCompressionFailed(status);

            if (bytesConsumed != srcLength)
                ArchiveSerializationException.ThrowCompressionFailed();

            Unsafe.WriteUnaligned(ref spanRef, value.Length);
            if (writer.IsByteSwapping)
            {
                BlittableMarshalling.ReverseEndianness(ref Unsafe.As<byte, int>(ref spanRef));
            }
            writer.Advance(bytesWritten + headerSize);
        }
        finally
        {
            if (buffer is not null)
                ArrayPool<char>.Shared.Return(buffer);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Deserialize(ref ArchiveReader reader, scoped ref string? value)
    {
        if (!reader.UnsafeTryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (length == 0)
        {
            value = "";
            return;
        }

        var byteLength = length * 2;

        if (decompressionSizeLimit < byteLength)
            ArchiveSerializationException.ThrowDecompressionSizeLimitExceeded(decompressionSizeLimit, byteLength);

        reader.GetRemainingSource(out var singleSource, out var remainingSource);

        _threadStatusConsumedBox ??= new StrongBox<int>();
        _threadStatusConsumedBox.Value = 0;

        if (singleSource.Length != 0)
        {
            unsafe
            {
                fixed (byte* p = singleSource)
                {
                    value = string.Create(
                        length,
                        (
                            Pointer: (IntPtr)p,
                            singleSource.Length,
                            ByteLength: byteLength,
                            Consumed: _threadStatusConsumedBox,
                            reader.IsByteSwapping
                        ),
                        static (stringSpan, state) =>
                        {
                            var src = MemoryMarshal.CreateSpan(
                                ref Unsafe.AsRef<byte>((byte*)state.Pointer),
                                state.Length
                            );
                            var destination = MemoryMarshal.AsBytes(stringSpan);

                            using var decoder = new BrotliDecoder();
                            var status = decoder.Decompress(
                                src,
                                destination,
                                out var bytesConsumed,
                                out var bytesWritten
                            );
                            if (status != OperationStatus.Done)
                            {
                                ArchiveSerializationException.ThrowCompressionFailed(status);
                            }
                            if (bytesWritten != state.ByteLength)
                            {
                                ArchiveSerializationException.ThrowCompressionFailed();
                            }

                            if (state.IsByteSwapping)
                            {
                                foreach (ref var c in stringSpan)
                                {
                                    BlittableMarshalling.ReverseEndianness(ref c);
                                }
                            }

                            state.Consumed.Value = bytesConsumed;
                        }
                    );
                    reader.Advance(_threadStatusConsumedBox.Value);
                }
            }
        }
        else
        {
            value = string.Create(
                length,
                (
                    Remaining: remainingSource,
                    remainingSource.Length,
                    ByteLength: byteLength,
                    Consumed: _threadStatusConsumedBox,
                    reader.IsByteSwapping
                ),
                static (stringSpan, state) =>
                {
                    var destination = MemoryMarshal.AsBytes(stringSpan);

                    using var decoder = new BrotliDecoder();

                    var consumed = 0;
                    OperationStatus status = OperationStatus.DestinationTooSmall;
                    foreach (var item in state.Remaining)
                    {
                        status = decoder.Decompress(
                            item.Span,
                            destination,
                            out var bytesConsumed,
                            out var bytesWritten
                        );
                        consumed += bytesConsumed;

                        destination = destination.Slice(bytesWritten);
                        if (status == OperationStatus.Done)
                        {
                            break;
                        }
                    }
                    if (status != OperationStatus.Done)
                    {
                        ArchiveSerializationException.ThrowCompressionFailed(status);
                    }

                    if (state.IsByteSwapping)
                    {
                        foreach (ref var c in stringSpan)
                        {
                            BlittableMarshalling.ReverseEndianness(ref c);
                        }
                    }

                    state.Consumed.Value = consumed;
                }
            );
            reader.Advance(_threadStatusConsumedBox.Value);
        }
    }
}
