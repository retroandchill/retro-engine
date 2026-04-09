// // @file ArchiveReader.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;
using LinkDotNet.StringBuilder;

namespace RetroEngine.Portable.Serialization.Binary;

public ref struct ArchiveReader : IDisposable
{
    private const string BufferEndReached = "Buffer reached end, reader can not provide more data.";

    private ReadOnlySequence<byte> _sequence;
    public long TotalSize { get; }
    private ref byte _bufferStart;
    private int _bufferLength;
    private byte[]? _rentedBuffer;
    private int _advanced;

    public int ConsumedBytes { get; private set; }
    public long RemainingBytes => TotalSize - ConsumedBytes;

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

    public ArchiveReader(in ReadOnlySequence<byte> sequence, ArchiveSerializerState state)
    {
        _sequence = sequence.IsSingleSegment ? ReadOnlySequence<byte>.Empty : sequence;
        var span = sequence.FirstSpan;
        _bufferStart = ref MemoryMarshal.GetReference(span);
        _bufferLength = span.Length;
        _advanced = 0;
        ConsumedBytes = 0;
        _rentedBuffer = null;
        TotalSize = sequence.Length;
        State = state;
    }

    public ArchiveReader(ReadOnlySpan<byte> buffer, ArchiveSerializerState state)
    {
        _sequence = ReadOnlySequence<byte>.Empty;
        _bufferStart = ref MemoryMarshal.GetReference(buffer);
        _bufferLength = buffer.Length;
        _advanced = 0;
        ConsumedBytes = 0;
        _rentedBuffer = null;
        TotalSize = buffer.Length;
        State = state;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref byte GetSpanReference(int sizeHint)
    {
        if (sizeHint <= _bufferLength)
        {
            return ref _bufferStart;
        }

        return ref GetNextSpan(sizeHint);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref byte GetNextSpan(int sizeHint)
    {
        if (_rentedBuffer is not null)
        {
            ArrayPool<byte>.Shared.Return(_rentedBuffer);
            _rentedBuffer = null;
        }

        if (RemainingBytes == 0)
        {
            ArchiveSerializationException.ThrowSequenceReachedEnd();
        }

        try
        {
            _sequence = _sequence.Slice(_advanced);
        }
        catch (ArgumentOutOfRangeException)
        {
            ArchiveSerializationException.ThrowSequenceReachedEnd();
        }

        _advanced = 0;

        if (sizeHint > RemainingBytes)
            ArchiveSerializationException.ThrowSequenceReachedEnd();

        if (sizeHint <= _sequence.FirstSpan.Length)
        {
            _bufferStart = ref MemoryMarshal.GetReference(_sequence.FirstSpan);
            _bufferLength = _sequence.FirstSpan.Length;
            return ref _bufferStart;
        }

        _rentedBuffer = ArrayPool<byte>.Shared.Rent(sizeHint);
        _sequence.Slice(0, sizeHint).CopyTo(_rentedBuffer);
        var span = _rentedBuffer.AsSpan(0, sizeHint);
        _bufferStart = ref MemoryMarshal.GetReference(span);
        _bufferLength = sizeHint;
        return ref _bufferStart;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        if (count == 0)
            return;

        var rest = _bufferLength - count;
        if (rest < 0)
        {
            AdvanceSequence(count);
            return;
        }

        _bufferLength = rest;
        _bufferStart = ref Unsafe.Add(ref _bufferStart, count);
        _advanced += count;
        ConsumedBytes += count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AdvanceSequence(int count)
    {
        var rest = _sequence.Length - count;
        if (rest < 0)
        {
            ArchiveSerializationException.ThrowInvalidAdvance();
        }

        _sequence = _sequence.Slice(_advanced + count);
        _bufferStart = ref MemoryMarshal.GetReference(_sequence.FirstSpan);
        _bufferLength = _sequence.FirstSpan.Length;
        _advanced = 0;
        ConsumedBytes += count;
    }

    public void GetRemainingSource(out ReadOnlySpan<byte> singleSource, out ReadOnlySequence<byte> remainingSource)
    {
        if (_sequence.IsEmpty)
        {
            remainingSource = ReadOnlySequence<byte>.Empty;
            singleSource = MemoryMarshal.CreateReadOnlySpan(ref _bufferStart, _bufferLength);
            return;
        }

        if (_sequence.IsSingleSegment)
        {
            remainingSource = ReadOnlySequence<byte>.Empty;
            singleSource = _sequence.FirstSpan[_advanced..];
            return;
        }

        singleSource = default;
        remainingSource = _sequence.Slice(_advanced);
        if (!remainingSource.IsSingleSegment)
            return;

        singleSource = remainingSource.FirstSpan;
        remainingSource = ReadOnlySequence<byte>.Empty;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (_rentedBuffer is not null)
            ArrayPool<byte>.Shared.Return(_rentedBuffer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ReadBool()
    {
        var value = ReadByte();
        return value switch
        {
            0 => false,
            1 => true,
            _ => throw new ArchiveSerializationException($"Invalid boolean value: {value}."),
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public char ReadChar()
    {
        const int size = sizeof(char);
        ref var spanRef = ref GetSpanReference(size);
        var value = !IsByteSwapping
            ? Unsafe.ReadUnaligned<char>(ref spanRef)
            : (char)BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<char>(ref spanRef));
        Advance(size);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte ReadByte()
    {
        ref var spanRef = ref GetSpanReference(1);
        Advance(1);
        return spanRef;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public sbyte ReadSByte()
    {
        ref var spanRef = ref GetSpanReference(1);
        Advance(1);
        return Unsafe.BitCast<byte, sbyte>(spanRef);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short ReadInt16()
    {
        const int size = sizeof(short);
        ref var spanRef = ref GetSpanReference(size);
        var value = !IsByteSwapping
            ? Unsafe.ReadUnaligned<short>(ref spanRef)
            : BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<short>(ref spanRef));
        Advance(size);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort ReadUInt16()
    {
        const int size = sizeof(ushort);
        ref var spanRef = ref GetSpanReference(size);
        var value = !IsByteSwapping
            ? Unsafe.ReadUnaligned<ushort>(ref spanRef)
            : BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<ushort>(ref spanRef));
        Advance(size);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadInt32()
    {
        const int size = sizeof(int);
        ref var spanRef = ref GetSpanReference(size);
        var value = !IsByteSwapping
            ? Unsafe.ReadUnaligned<int>(ref spanRef)
            : BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<int>(ref spanRef));
        Advance(size);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint ReadUInt32()
    {
        const int size = sizeof(uint);
        ref var spanRef = ref GetSpanReference(size);
        var value = !IsByteSwapping
            ? Unsafe.ReadUnaligned<uint>(ref spanRef)
            : BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<uint>(ref spanRef));
        Advance(size);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long ReadInt64()
    {
        const int size = sizeof(long);
        ref var spanRef = ref GetSpanReference(size);
        var value = !IsByteSwapping
            ? Unsafe.ReadUnaligned<long>(ref spanRef)
            : BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<long>(ref spanRef));
        Advance(size);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong ReadUInt64()
    {
        const int size = sizeof(ulong);
        ref var spanRef = ref GetSpanReference(size);
        var value = !IsByteSwapping
            ? Unsafe.ReadUnaligned<ulong>(ref spanRef)
            : BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<ulong>(ref spanRef));
        Advance(size);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float ReadSingle()
    {
        const int size = sizeof(float);
        ref var spanRef = ref GetSpanReference(size);
        var value = !IsByteSwapping
            ? Unsafe.ReadUnaligned<float>(ref spanRef)
            : BitConverter.Int32BitsToSingle(
                BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<int>(ref spanRef))
            );
        Advance(size);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ReadDouble()
    {
        const int size = sizeof(double);
        ref var spanRef = ref GetSpanReference(size);
        var value = !IsByteSwapping
            ? Unsafe.ReadUnaligned<double>(ref spanRef)
            : BitConverter.Int64BitsToDouble(
                BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<long>(ref spanRef))
            );
        Advance(size);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Guid ReadGuid()
    {
        const int guidSize = 16;
        ref var spanRef = ref GetSpanReference(guidSize);
        var span = MemoryMarshal.CreateReadOnlySpan(ref spanRef, guidSize);
        Advance(guidSize);
        return new Guid(span, Options.ByteOrder == ByteOrder.BigEndian);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DateTimeOffset ReadDateTime()
    {
        var unixTimestamp = ReadInt64();
        return DateTimeOffset.FromUnixTimeMilliseconds(unixTimestamp);
    }
}
