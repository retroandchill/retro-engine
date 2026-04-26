// // @file ArchiveWriter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;
using MagicArchive.Utilities;

namespace MagicArchive;

public static class ArchiveWriter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ArchiveWriter<TBufferWriter> Create<TBufferWriter>(
        ref TBufferWriter bufferWriter,
        ArchiveWriterState state
    )
        where TBufferWriter : IBufferWriter<byte>
    {
        return new ArchiveWriter<TBufferWriter>(ref bufferWriter, state);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ArchiveWriter<TBufferWriter> Create<TBufferWriter>(
        ref TBufferWriter bufferWriter,
        byte[] firstBufferOfWriter,
        ArchiveWriterState state
    )
        where TBufferWriter : IBufferWriter<byte>
    {
        return new ArchiveWriter<TBufferWriter>(ref bufferWriter, firstBufferOfWriter, state);
    }
}

public ref partial struct ArchiveWriter<TBufferWriter>
    where TBufferWriter : IBufferWriter<byte>
{
    private const string BufferLimitReached = "Buffer limit reached, writer can not provide more data.";
    private const int DepthLimit = 1000;

    private ref TBufferWriter _bufferWriter;
    private ref byte _bufferStart;
    public int BufferLength { get; private set; }
    private int _advanced;
    private int _depth;

    public int WrittenBytes { get; private set; }

    private readonly bool _serializeStringAsUtf8;

    public ArchiveWriterState State { get; }
    public ArchiveSerializerOptions Options => State.Options;

    public bool IsByteSwapping
    {
        get
        {
            if (BitConverter.IsLittleEndian)
            {
                return Options.ByteOrder == ByteOrder.BigEndian;
            }

            return Options.ByteOrder == ByteOrder.LittleEndian;
        }
    }

    public ArchiveWriter(ref TBufferWriter bufferWriter, ArchiveWriterState state)
    {
        _bufferWriter = ref bufferWriter;
        _bufferStart = ref Unsafe.NullRef<byte>();
        BufferLength = 0;
        _advanced = 0;
        _depth = 0;
        WrittenBytes = 0;
        _serializeStringAsUtf8 = state.Options.StringEncoding == StringEncoding.Utf8;
        State = state;
    }

    public ArchiveWriter(ref TBufferWriter bufferWriter, byte[] firstBufferOfWriter, ArchiveWriterState state)
    {
        _bufferWriter = ref bufferWriter;
        _bufferStart = ref MemoryMarshal.GetArrayDataReference(firstBufferOfWriter);
        BufferLength = firstBufferOfWriter.Length;
        _advanced = 0;
        _depth = 0;
        WrittenBytes = 0;
        _serializeStringAsUtf8 = state.Options.StringEncoding == StringEncoding.Utf8;
        State = state;
    }

    public ArchiveWriter(ref TBufferWriter bufferWriter, Span<byte> firstBufferOfWriter, ArchiveWriterState state)
    {
        _bufferWriter = ref bufferWriter;
        _bufferStart = ref MemoryMarshal.GetReference(firstBufferOfWriter);
        BufferLength = firstBufferOfWriter.Length;
        _advanced = 0;
        _depth = 0;
        WrittenBytes = 0;
        _serializeStringAsUtf8 = state.Options.StringEncoding == StringEncoding.Utf8;
        State = state;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref byte GetSpanReference(int sizeHint)
    {
        if (BufferLength < sizeHint)
        {
            RequestNewBuffer(sizeHint);
        }

        return ref _bufferStart;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void RequestNewBuffer(int sizeHint)
    {
        if (_advanced != 0)
        {
            _bufferWriter.Advance(_advanced);
            _advanced = 0;
        }

        var span = _bufferWriter.GetSpan(sizeHint);
        _bufferStart = ref MemoryMarshal.GetReference(span);
        BufferLength = span.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        if (count == 0)
            return;

        var rest = BufferLength - count;
        if (rest < 0)
        {
            throw new ArchiveSerializationException(BufferLimitReached);
        }

        BufferLength = rest;
        _bufferStart = ref Unsafe.Add(ref _bufferStart, count);
        _advanced += count;
        WrittenBytes += count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Flush()
    {
        if (_advanced != 0)
        {
            _bufferWriter.Advance(_advanced);
            _advanced = 0;
        }

        _bufferStart = ref Unsafe.NullRef<byte>();
        BufferLength = 0;
        WrittenBytes = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IArchiveFormatter GetFormatter(Type type)
    {
        return ArchiveFormatterRegistry.GetFormatter(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IArchiveFormatter<T> GetFormatter<T>()
    {
        return ArchiveFormatterRegistry.GetFormatter<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteObjectHeader(byte memberCount)
    {
        if (memberCount >= ArchiveCodes.Reserved1)
        {
            ArchiveSerializationException.ThrowWriteInvalidMemberCount(memberCount);
        }

        WriteBlittable(memberCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteNullObjectHeader()
    {
        WriteBlittable(ArchiveCodes.NullObject);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteObjectReferenceId(uint referenceId)
    {
        GetSpanReference(sizeof(byte)) = ArchiveCodes.ReferenceId;
        Advance(sizeof(byte));
        WriteBlittable(referenceId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUnionHeader(ushort tag)
    {
        if (tag < ArchiveCodes.WideTag)
        {
            WriteBlittable((byte)tag);
        }
        else
        {
            const int size = sizeof(byte) + sizeof(ushort);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, ArchiveCodes.WideTag);
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref spanRef, 1), tag);
            Advance(size);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteNullUnionHeader()
    {
        WriteNullObjectHeader();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteCollectionHeader(int length)
    {
        WriteBlittable(length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteNullCollectionHeader()
    {
        WriteBlittable(ArchiveCodes.NullCollection);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteString(string? value)
    {
        if (_serializeStringAsUtf8)
        {
            WriteUtf8(value);
        }
        else
        {
            WriteUtf16(value);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteString(scoped ReadOnlySpan<char> value)
    {
        if (_serializeStringAsUtf8)
        {
            WriteUtf8(value);
        }
        else
        {
            WriteUtf16(value);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUtf16(string? value)
    {
        if (value is null)
        {
            WriteNullCollectionHeader();
            return;
        }

        WriteUtf16(value.AsSpan());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUtf16(scoped ReadOnlySpan<char> value)
    {
        if (value.Length == 0)
        {
            WriteCollectionHeader(0);
            return;
        }

        var copyByteCount = checked(value.Length * sizeof(char)) + 4;

        ref var dest = ref GetSpanReference(copyByteCount);
        if (!IsByteSwapping)
        {
            ref var src = ref Unsafe.As<char, byte>(ref Unsafe.AsRef(in value.GetPinnableReference()));
            Unsafe.WriteUnaligned(ref dest, value.Length);
            Unsafe.CopyBlockUnaligned(ref Unsafe.Add(ref dest, 4), ref src, (uint)copyByteCount);
        }
        else
        {
            Unsafe.WriteUnaligned(ref dest, BinaryPrimitives.ReverseEndianness(value.Length));
            for (var i = 0; i < value.Length; i++)
            {
                Unsafe.WriteUnaligned(
                    ref Unsafe.Add(ref dest, i * sizeof(char) + sizeof(int)),
                    BinaryPrimitives.ReverseEndianness(value[i])
                );
            }
        }

        Advance(copyByteCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUtf8(string? value)
    {
        if (value is null)
        {
            WriteNullCollectionHeader();
            return;
        }

        WriteUtf8(value.AsSpan());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUtf8(scoped ReadOnlySpan<char> value)
    {
        if (value.Length == 0)
        {
            WriteCollectionHeader(0);
            return;
        }

        var maxUtf8Size = Encoding.UTF8.GetMaxByteCount(value.Length);
        ref var destPointer = ref GetSpanReference(maxUtf8Size + sizeof(int));
        var dest = MemoryMarshal.CreateSpan(ref Unsafe.Add(ref destPointer, sizeof(int)), maxUtf8Size);
        var status = Utf8.FromUtf16(value, dest, out _, out var bytesWritten, replaceInvalidSequences: false);
        if (status != OperationStatus.Done)
        {
            ArchiveSerializationException.ThrowFailedEncoding(status);
        }

        if (!IsByteSwapping)
            Unsafe.WriteUnaligned(ref destPointer, ~bytesWritten);
        else
            Unsafe.WriteUnaligned(ref destPointer, BinaryPrimitives.ReverseEndianness(~bytesWritten));
        Advance(bytesWritten + sizeof(int));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBool(bool value)
    {
        WriteBlittable(value ? (byte)1 : (byte)0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteArchivable<T>(in T value)
        where T : IArchivable<T>
    {
        _depth++;
        if (_depth == DepthLimit)
            ArchiveSerializationException.ThrowReachedDepthLimit(typeof(T));
        T.Serialize(ref this, in value);
        _depth--;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteArchivableArray<T>(T[]? value)
        where T : IArchivable<T>
    {
        if (value is null)
        {
            WriteNullCollectionHeader();
            return;
        }

        WriteArchivableSpan(value.AsSpan());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteArchivableSpan<T>(scoped ReadOnlySpan<T?> value)
        where T : IArchivable<T>
    {
        WriteCollectionHeader(value.Length);
        foreach (ref readonly var t in value)
        {
            T.Serialize(ref this, in t);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteValue(Type type, object? value)
    {
        _depth++;
        if (_depth == DepthLimit)
            ArchiveSerializationException.ThrowReachedDepthLimit(type);
        GetFormatter(type).Serialize(ref this, value);
        _depth--;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteValue<T>(in T value)
    {
        _depth++;
        if (_depth == DepthLimit)
            ArchiveSerializationException.ThrowReachedDepthLimit(typeof(T));
        if (BlittableMarshalling.IsBlittable<T>())
        {
            UnsafeWriteBlittable(in value);
            _depth--;
            return;
        }

        var formatter = GetFormatter<T>();
        formatter.Serialize(ref this, in value);
        _depth--;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteValueWithFormatter<TFormatter, T>(TFormatter formatter, in T value)
        where TFormatter : IArchiveFormatter<T>
    {
        _depth++;
        formatter.Serialize(ref this, in value);
        _depth--;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteNullable<T>(in T? value)
        where T : struct
    {
        if (!value.HasValue)
        {
            WriteNullObjectHeader();
            return;
        }

        WriteObjectHeader(1);
        WriteValue(value.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteArray<T>(T[]? value)
    {
        if (value is null)
        {
            WriteNullCollectionHeader();
            return;
        }

        WriteCollectionHeader(value.Length);
        WriteSpanWithoutHeader(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSpan<T>(scoped ReadOnlySpan<T> value)
    {
        WriteCollectionHeader(value.Length);
        WriteSpanWithoutHeader(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteSpanWithoutHeader<T>(scoped ReadOnlySpan<T> value)
    {
        if (!IsByteSwapping && BlittableMarshalling.IsBlittable<T>())
        {
            UnsafeWriteBlittableSpan(value);
            return;
        }

        var formatter = GetFormatter<T>();
        foreach (ref readonly var t in value)
        {
            formatter.Serialize(ref this, in t);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittableSpan<T>(scoped ReadOnlySpan<T> value)
    {
        if (value.Length == 0)
            return;

        var byteCount = value.Length * Unsafe.SizeOf<T>();
        ref var dest = ref GetSpanReference(byteCount);
        ref var src = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(value));

        Unsafe.CopyBlockUnaligned(ref dest, ref src, (uint)byteCount);
        Advance(byteCount);
    }
}
