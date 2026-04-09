// // @file ArchiveWriter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RetroEngine.Portable.Serialization.Binary;

public ref struct ArchiveWriter<TBufferWriter>
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

    public ArchiveSerializerState State { get; }
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

    public ArchiveWriter(ref TBufferWriter bufferWriter, ArchiveSerializerState state)
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

    public ArchiveWriter(ref TBufferWriter bufferWriter, byte[] firstBufferOfWriter, ArchiveSerializerState state)
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

    public ArchiveWriter(ref TBufferWriter bufferWriter, Span<byte> firstBufferOfWriter, ArchiveSerializerState state)
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

        var rest = _bufferStart - count;
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
    public void WriteRawBytes(ReadOnlySpan<byte> buffer)
    {
        ref var spanRef = ref GetSpanReference(buffer.Length);
        var span = MemoryMarshal.CreateSpan(ref spanRef, buffer.Length);
        buffer.CopyTo(span);
        Advance(buffer.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(bool value)
    {
        Write(value ? (byte)1 : (byte)0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(char value)
    {
        const int size = sizeof(char);
        ref var spanRef = ref GetSpanReference(size);
        if (!IsByteSwapping)
            Unsafe.WriteUnaligned(ref spanRef, value);
        else
            Unsafe.WriteUnaligned(ref spanRef, BinaryPrimitives.ReverseEndianness(value));
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(byte value)
    {
        ref var spanRef = ref GetSpanReference(1);
        spanRef = value;
        Advance(1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(sbyte value)
    {
        ref var spanRef = ref GetSpanReference(1);
        spanRef = Unsafe.BitCast<sbyte, byte>(value);
        Advance(1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(short value)
    {
        const int size = sizeof(short);
        ref var spanRef = ref GetSpanReference(size);
        if (!IsByteSwapping)
            Unsafe.WriteUnaligned(ref spanRef, value);
        else
            Unsafe.WriteUnaligned(ref spanRef, BinaryPrimitives.ReverseEndianness(value));
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ushort value)
    {
        const int size = sizeof(ushort);
        ref var spanRef = ref GetSpanReference(size);
        if (!IsByteSwapping)
            Unsafe.WriteUnaligned(ref spanRef, value);
        else
            Unsafe.WriteUnaligned(ref spanRef, BinaryPrimitives.ReverseEndianness(value));
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(int value)
    {
        const int size = sizeof(int);
        ref var spanRef = ref GetSpanReference(size);
        if (!IsByteSwapping)
            Unsafe.WriteUnaligned(ref spanRef, value);
        else
            Unsafe.WriteUnaligned(ref spanRef, BinaryPrimitives.ReverseEndianness(value));
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(uint value)
    {
        const int size = sizeof(uint);
        ref var spanRef = ref GetSpanReference(size);
        if (!IsByteSwapping)
            Unsafe.WriteUnaligned(ref spanRef, value);
        else
            Unsafe.WriteUnaligned(ref spanRef, BinaryPrimitives.ReverseEndianness(value));
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(long value)
    {
        const int size = sizeof(long);
        ref var spanRef = ref GetSpanReference(size);
        if (!IsByteSwapping)
            Unsafe.WriteUnaligned(ref spanRef, value);
        else
            Unsafe.WriteUnaligned(ref spanRef, BinaryPrimitives.ReverseEndianness(value));
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(ulong value)
    {
        const int size = sizeof(ulong);
        ref var spanRef = ref GetSpanReference(size);
        if (!IsByteSwapping)
            Unsafe.WriteUnaligned(ref spanRef, value);
        else
            Unsafe.WriteUnaligned(ref spanRef, BinaryPrimitives.ReverseEndianness(value));
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(float value)
    {
        const int size = sizeof(float);
        ref var spanRef = ref GetSpanReference(size);
        if (!IsByteSwapping)
            Unsafe.WriteUnaligned(ref spanRef, value);
        else
            Unsafe.WriteUnaligned(
                ref spanRef,
                BinaryPrimitives.ReverseEndianness(BitConverter.SingleToInt32Bits(value))
            );
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(double value)
    {
        const int size = sizeof(double);
        ref var spanRef = ref GetSpanReference(size);
        if (!IsByteSwapping)
            Unsafe.WriteUnaligned(ref spanRef, value);
        else
            Unsafe.WriteUnaligned(
                ref spanRef,
                BinaryPrimitives.ReverseEndianness(BitConverter.DoubleToInt64Bits(value))
            );
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(Guid value)
    {
        const int size = 16;
        ref var spanRef = ref GetSpanReference(size);
        value.TryWriteBytes(
            MemoryMarshal.CreateSpan(ref spanRef, size),
            Options.ByteOrder == ByteOrder.BigEndian,
            out _
        );
        Advance(size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(DateTimeOffset value)
    {
        Write(value.ToUnixTimeMilliseconds());
    }
}
