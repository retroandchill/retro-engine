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
using RetroEngine.Portable.Serialization.Binary.Utilities;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Portable.Serialization.Binary;

public ref struct ArchiveReader : IDisposable
{
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
    public IArchiveFormatter GetFormatter(Type type)
    {
        throw new NotImplementedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IArchiveFormatter<T> GetFormatter<T>()
    {
        throw new NotImplementedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReadObjectHeader(out byte memberCount)
    {
        memberCount = GetSpanReference(1);
        Advance(1);
        return memberCount != ArchiveCodes.NullObject;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReadUnionHeader(out ushort tag)
    {
        var firstTag = GetSpanReference(1);
        Advance(1);
        switch (firstTag)
        {
            case < ArchiveCodes.WideTag:
                tag = firstTag;
                return true;
            case ArchiveCodes.WideTag:
                tag = ReadUInt16();
                return true;
            default:
                tag = 0;
                return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReadCollectionHeader(out int length)
    {
        length = ReadInt32();

        if (RemainingBytes < length)
        {
            ArchiveSerializationException.ThrowInsufficientBufferUnless(length);
        }

        return length != ArchiveCodes.NullCollection;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool PeekIsNull()
    {
        var code = GetSpanReference(1);
        return code == ArchiveCodes.NullObject;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPeekObjectHeader(out byte memberCount)
    {
        memberCount = GetSpanReference(1);
        return memberCount != ArchiveCodes.NullObject;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPeekUnionHeader(out ushort tag)
    {
        var firstTag = GetSpanReference(1);
        switch (firstTag)
        {
            case < ArchiveCodes.WideTag:
                tag = firstTag;
                return true;
            case ArchiveCodes.WideTag:
                tag = ReadUInt16();
                return true;
            default:
                tag = 0;
                return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPeekCollectionHeader(out int length)
    {
        const int size = sizeof(int);
        ref var spanRef = ref GetSpanReference(size);
        length = !IsByteSwapping
            ? Unsafe.ReadUnaligned<int>(ref spanRef)
            : BinaryPrimitives.ReverseEndianness(Unsafe.ReadUnaligned<int>(ref spanRef));

        if (RemainingBytes < length)
        {
            ArchiveSerializationException.ThrowInsufficientBufferUnless(length);
        }

        return length != ArchiveCodes.NullCollection;
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
        return (char)ReadUInt16();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rune ReadRune()
    {
        return new Rune(ReadUInt32());
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
        return Unsafe.BitCast<byte, sbyte>(ReadByte());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public short ReadInt16()
    {
        return Unsafe.BitCast<ushort, short>(ReadUInt16());
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
        return Unsafe.BitCast<uint, int>(ReadUInt32());
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
        return Unsafe.BitCast<ulong, long>(ReadUInt64());
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
        return Unsafe.BitCast<uint, float>(ReadUInt32());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double ReadDouble()
    {
        return Unsafe.BitCast<ulong, double>(ReadUInt64());
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Name ReadName()
    {
        if (!TryReadCollectionHeader(out var length))
        {
            ArchiveSerializationException.ThrowDeserializeObjectIsNull("Name");
        }

        ref var spanRef = ref GetSpanReference(length);
        var span = MemoryMarshal.CreateReadOnlySpan(ref spanRef, length);
        var value = new Name(span);
        Advance(length);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string? ReadString()
    {
        if (!TryReadCollectionHeader(out var length))
            return null;

        return length switch
        {
            0 => "",
            > 0 => ReadUtf16(length),
            _ => ReadUtf8(length),
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string ReadUtf16(int length)
    {
        var byteCount = checked(length * 2);
        ref var src = ref GetSpanReference(byteCount);

        if (!IsByteSwapping)
        {
            var str = new string(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<byte, char>(ref src), length));
            Advance(byteCount);
            return str;
        }

        using var builder = new ValueStringBuilder(byteCount);
        for (var i = 0; i < length; i++)
        {
            builder.Append(Unsafe.ReadUnaligned<char>(ref src));
            src = ref Unsafe.Add(ref src, 1);
        }
        Advance(byteCount);
        return builder.ToString();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private string ReadUtf8(int length)
    {
        // (int ~utf8-byte-count, int utf16-length, utf8-bytes)
        // already read utf8 length, but it is complement.
        var utf8Length = ~length;

        ref var spanRef = ref GetSpanReference(utf8Length);

        var maxBufferSize = Encoding.UTF8.GetMaxCharCount(utf8Length);
        var buffer = ArrayPool<char>.Shared.Rent(maxBufferSize);
        try
        {
            var src = MemoryMarshal.CreateReadOnlySpan(ref spanRef, utf8Length);
            var status = Utf8.ToUtf16(src, buffer, out _, out var strLength, replaceInvalidSequences: false);
            if (status != OperationStatus.Done)
            {
                ArchiveSerializationException.ThrowFailedEncoding(status);
            }
            Advance(utf8Length);
            return new string(buffer.AsSpan(0, strLength));
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }

    public bool TryReadString(Span<char> span, out int length)
    {
        // ReSharper disable once InvertIf
        if (!TryPeekCollectionHeader(out length))
        {
            Advance(sizeof(int));
            return true;
        }

        return length switch
        {
            0 => true,
            > 0 => TryReadUtf16(span, length),
            _ => TryReadUtf8(span, ref length),
        };
    }

    private bool TryReadUtf16(Span<char> span, int length)
    {
        if (length > span.Length)
            return false;

        Advance(sizeof(int));

        var byteCount = checked(length * 2);
        ref var src = ref GetSpanReference(byteCount);
        if (!IsByteSwapping)
        {
            MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<byte, char>(ref src), length).CopyTo(span);
            Advance(byteCount);
            return true;
        }

        using var builder = new ValueStringBuilder(byteCount);
        for (var i = 0; i < length; i++)
        {
            span[i] = Unsafe.ReadUnaligned<char>(ref src);
            src = ref Unsafe.Add(ref src, 1);
        }
        Advance(byteCount);
        return true;
    }

    private bool TryReadUtf8(Span<char> span, ref int length)
    {
        // (int ~utf8-byte-count, int utf16-length, utf8-bytes)
        // already read utf8 length, but it is complement.
        var utf8Length = ~length;

        ref var spanRef = ref GetSpanReference(utf8Length + sizeof(int));

        var maxBufferSize = Encoding.UTF8.GetMaxCharCount(utf8Length);
        char[]? buffer;
        Span<char> dest;
        if (span.Length < maxBufferSize)
        {
            buffer = ArrayPool<char>.Shared.Rent(maxBufferSize);
            dest = buffer;
        }
        else
        {
            buffer = null;
            dest = span;
        }

        try
        {
            var src = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref spanRef, sizeof(int)), utf8Length);
            var status = Utf8.ToUtf16(src, dest, out _, out length, replaceInvalidSequences: false);
            if (status != OperationStatus.Done)
            {
                ArchiveSerializationException.ThrowFailedEncoding(status);
            }

            if (buffer is not null)
            {
                if (length > buffer.Length)
                {
                    return false;
                }

                dest[..length].CopyTo(span);
            }

            Advance(utf8Length + sizeof(int));
            return true;
        }
        finally
        {
            if (buffer is not null)
                ArrayPool<char>.Shared.Return(buffer);
        }
    }

    public T Read<T>()
    {
        throw new NotImplementedException();
    }

    public T ReadUnmanaged<T>()
        where T : unmanaged
    {
        if (BinaryHandling.IsBlittable<T>())
        {
            var size = Unsafe.SizeOf<T>();
            ref var spanRef = ref GetSpanReference(size);
            var value = Unsafe.ReadUnaligned<T>(ref spanRef);
            if (IsByteSwapping)
                BinaryHandling.ReverseEndianness(ref value);
            Advance(size);
            return value;
        }

        if (typeof(T) == typeof(bool))
            return Unsafe.BitCast<bool, T>(ReadBool());

        if (typeof(T) == typeof(Name))
            return Unsafe.BitCast<Name, T>(ReadName());

        if (typeof(T) == typeof(Guid))
            return Unsafe.BitCast<Guid, T>(ReadGuid());

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (typeof(T) == typeof(DateTimeOffset))
            return Unsafe.BitCast<DateTimeOffset, T>(ReadDateTime());

        // Fallback: if we don't have a known-type we'll go through the regular serialization path
        return Read<T>();
    }

    public T[]? ReadArray<T>()
    {
        T[]? value = null;
        ReadArray(ref value);
        return value;
    }

    public void ReadArray<T>(scoped ref T[]? value)
    {
        if (!TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (length == 0)
        {
            value = [];
            return;
        }

        if (!IsByteSwapping && BinaryHandling.IsBlittable<T>())
        {
            ReadUnmanagedArray(ref value, length);
            return;
        }

        if (value is null || value.Length != length)
        {
            value = new T[length];
        }

        var formatter = GetFormatter<T>();
        for (var i = 0; i < length; i++)
        {
            formatter.Deserialize(ref this, ref value[i]);
        }
    }

    private void ReadUnmanagedArray<T>(scoped ref T[]? value, int length)
    {
        var byteCount = length * Unsafe.SizeOf<T>();
        ref var src = ref GetSpanReference(byteCount);

        if (value is null || value.Length != length)
        {
            value = GC.AllocateUninitializedArray<T>(length);
        }

        ref var dest = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetArrayDataReference(value));
        Unsafe.CopyBlockUnaligned(ref dest, ref src, (uint)byteCount);

        Advance(byteCount);
    }

    public bool TryReadInto<T>(Span<T?> buffer, out int length)
    {
        if (!TryPeekCollectionHeader(out length) || length == 0)
        {
            Advance(sizeof(int));
            return true;
        }

        if (length > buffer.Length)
        {
            return false;
        }

        Advance(sizeof(int));
        if (!IsByteSwapping && BinaryHandling.IsBlittable<T>())
        {
            ReadIntoUnmanaged(buffer[..length]);
            return true;
        }

        var formatter = GetFormatter<T>();
        for (var i = 0; i < length; i++)
        {
            formatter.Deserialize(ref this, ref buffer[i]);
        }

        return true;
    }

    private void ReadIntoUnmanaged<T>(Span<T> buffer)
    {
        var byteCount = buffer.Length * Unsafe.SizeOf<T>();
        ref var src = ref GetSpanReference(byteCount);

        ref var dest = ref Unsafe.As<T, byte>(ref MemoryMarshal.GetReference(buffer));
        Unsafe.CopyBlockUnaligned(ref dest, ref src, (uint)byteCount);
        Advance(byteCount);
    }
}
