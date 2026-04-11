// // @file ArchiveSerializer.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MagicArchive.Utilities;

namespace MagicArchive;

public static class ArchiveSerializer
{
    [ThreadStatic]
    private static SerializeWriterThreadStaticState? _threadStaticState;

    [ThreadStatic]
    private static ArchiveSerializerState? _threadStaticWriterState;

    [ThreadStatic]
    private static ArchiveSerializerState? _threadStaticReaderState;

    public static byte[] Serialize<T>(in T? value, ArchiveSerializerOptions? options = null)
    {
        var writeLittleEndian = options is null || options.ByteOrder == ByteOrder.LittleEndian;
        var byteSwapping = writeLittleEndian ? !BitConverter.IsLittleEndian : BitConverter.IsLittleEndian;
        if (BinaryHandling.IsBlittable<T>())
        {
            var array = GC.AllocateUninitializedArray<byte>(Unsafe.SizeOf<T>());
            Unsafe.WriteUnaligned(
                ref MemoryMarshal.GetArrayDataReference(array),
                !byteSwapping ? value : BinaryHandling.ReverseEndianness(value)
            );
            return array;
        }

        var typeKind = TypeHelpers.TryGetArchivableSize<T>(out var elementSize);
        if (typeKind == TypeHelpers.TypeKind.None)
        {
            // DO nothing
        }
        else if (typeKind == TypeHelpers.TypeKind.BlittableSZArray)
        {
            if (value is null)
            {
                return ArchiveCodes.NullCollectionData.ToArray();
            }

            var srcArray = (Array)(object)value;
            var length = srcArray.Length;
            if (length == 0)
            {
                return ArchiveCodes.ZeroCollectionData.ToArray();
            }

            var dataSize = elementSize * length;
            var destArray = GC.AllocateUninitializedArray<byte>(dataSize + sizeof(int));
            ref var head = ref MemoryMarshal.GetArrayDataReference(destArray);

            SerializeBlittableArray(byteSwapping, ref head, length, srcArray, dataSize, elementSize);
        }
        else if (typeKind == TypeHelpers.TypeKind.FixedSizeArchivable)
        {
            var buffer = new byte[value is null ? 1 : elementSize];
            var bufferWriter = new FixedArrayBufferWriter(buffer);
            var nullState = writeLittleEndian
                ? ArchiveSerializerState.NullStateLittleEndian
                : ArchiveSerializerState.NullStateBigEndian;
            var writer = ArchiveWriter.Create(ref bufferWriter, nullState);
            Serialize(ref writer, value);
            return bufferWriter.FilledBuffer;
        }

        _threadStaticState ??= new SerializeWriterThreadStaticState();
        _threadStaticState.Init(options);

        try
        {
            var writer = ArchiveWriter.Create(
                ref _threadStaticState.BufferWriter,
                _threadStaticState.BufferWriter.FirstBuffer,
                _threadStaticState.State
            );
            Serialize(ref writer, in value);
            return _threadStaticState.BufferWriter.ToArrayAndReset();
        }
        finally
        {
            _threadStaticState.Reset();
        }
    }

    private static void SerializeBlittableArray(
        bool byteSwapping,
        ref byte head,
        int length,
        Array srcArray,
        int dataSize,
        int elementSize
    )
    {
        if (!byteSwapping)
        {
            Unsafe.WriteUnaligned(ref head, length);
            Unsafe.CopyBlockUnaligned(
                ref Unsafe.Add(ref head, sizeof(int)),
                ref MemoryMarshal.GetArrayDataReference(srcArray),
                (uint)dataSize
            );
        }
        else
        {
            Unsafe.WriteUnaligned(ref head, BinaryPrimitives.ReverseEndianness(length));
            head = ref Unsafe.Add(ref head, sizeof(int));
            for (var i = 0; i < length; i++)
            {
                Unsafe.WriteUnaligned(ref head, BinaryHandling.ReverseEndianness(srcArray.GetValue(i)));
                head = ref Unsafe.Add(ref head, elementSize);
            }
        }
    }

    public static void Serialize<T, TBufferWriter>(
        in TBufferWriter bufferWriter,
        in T? value,
        ArchiveSerializerOptions? options = null
    )
        where TBufferWriter : IBufferWriter<byte>
    {
        ref var bufferWriterRef = ref Unsafe.AsRef(in bufferWriter);
        var writeLittleEndian = options is null || options.ByteOrder == ByteOrder.LittleEndian;
        var byteSwapping = writeLittleEndian ? !BitConverter.IsLittleEndian : BitConverter.IsLittleEndian;
        if (BinaryHandling.IsBlittable<T>())
        {
            var buffer = bufferWriterRef.GetSpan(Unsafe.SizeOf<T>());
            Unsafe.WriteUnaligned(
                ref MemoryMarshal.GetReference(buffer),
                !byteSwapping ? value : BinaryHandling.ReverseEndianness(value)
            );
            bufferWriterRef.Advance(Unsafe.SizeOf<T>());
            return;
        }

        var typeKind = TypeHelpers.TryGetArchivableSize<T>(out var elementSize);
        if (typeKind == TypeHelpers.TypeKind.BlittableSZArray)
        {
            if (value is null)
            {
                var span = bufferWriterRef.GetSpan(sizeof(int));
                ArchiveCodes.NullCollectionData.CopyTo(span);
                bufferWriterRef.Advance(sizeof(int));
                return;
            }

            var srcArray = (Array)(object)value;
            var length = srcArray.Length;
            if (length == 0)
            {
                var span = bufferWriterRef.GetSpan(sizeof(int));
                ArchiveCodes.ZeroCollectionData.CopyTo(span);
                bufferWriterRef.Advance(sizeof(int));
                return;
            }

            var dataSize = elementSize * length;
            var destSpan = bufferWriterRef.GetSpan(dataSize + sizeof(int));
            ref var head = ref MemoryMarshal.GetReference(destSpan);
            SerializeBlittableArray(byteSwapping, ref head, length, srcArray, dataSize, elementSize);
        }

        _threadStaticWriterState ??= new ArchiveSerializerState();
        _threadStaticWriterState.Init(options);

        try
        {
            var writer = ArchiveWriter.Create(ref bufferWriterRef, _threadStaticWriterState);
            Serialize(ref writer, in value);
        }
        finally
        {
            _threadStaticWriterState.Reset();
        }
    }

    public static void Serialize<T, TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, in T? value)
        where TBufferWriter : IBufferWriter<byte>
    {
        writer.Write(value);
        writer.Flush();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static async ValueTask SerializeAsync<T>(
        Stream stream,
        T? value,
        ArchiveSerializerOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        var tempWriter = ReusableLinkedArrayBufferWriterPool.Rent();
        try
        {
            Serialize(tempWriter, in value, options);
            await tempWriter.WriteToAndResetAsync(stream, cancellationToken).ConfigureAwait(false);
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            ReusableLinkedArrayBufferWriterPool.Return(tempWriter);
        }
    }

    public static T? Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        ReadOnlySpan<byte> buffer,
        ArchiveSerializerOptions? options = null
    )
    {
        T? value = default;
        Deserialize(buffer, ref value, options);
        return value;
    }

    public static int Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        ReadOnlySpan<byte> buffer,
        ref T? value,
        ArchiveSerializerOptions? options = null
    )
    {
        var writeLittleEndian = options is null || options.ByteOrder == ByteOrder.LittleEndian;
        var byteSwapping = writeLittleEndian ? !BitConverter.IsLittleEndian : BitConverter.IsLittleEndian;
        if (BinaryHandling.IsBlittable<T>())
        {
            if (buffer.Length < Unsafe.SizeOf<T>())
            {
                ArchiveSerializationException.ThrowInvalidRange(Unsafe.SizeOf<T>(), buffer.Length);
            }
            value = Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(buffer));
            if (byteSwapping)
                BinaryHandling.ReverseEndianness(ref value);
            return Unsafe.SizeOf<T>();
        }

        _threadStaticReaderState ??= new ArchiveSerializerState();
        _threadStaticReaderState.Init(options);

        var reader = new ArchiveReader(buffer, _threadStaticReaderState);
        try
        {
            reader.Read(ref value);
            return reader.ConsumedBytes;
        }
        finally
        {
            reader.Dispose();
            _threadStaticReaderState.Reset();
        }
    }

    public static T? Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        in ReadOnlySequence<byte> buffer,
        ArchiveSerializerOptions? options = null
    )
    {
        T? value = default;
        Deserialize(buffer, ref value, options);
        return value;
    }

    public static int Deserialize<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        in ReadOnlySequence<byte> buffer,
        ref T? value,
        ArchiveSerializerOptions? options = null
    )
    {
        var writeLittleEndian = options is null || options.ByteOrder == ByteOrder.LittleEndian;
        var byteSwapping = writeLittleEndian ? !BitConverter.IsLittleEndian : BitConverter.IsLittleEndian;
        if (BinaryHandling.IsBlittable<T>())
        {
            var sizeOfT = Unsafe.SizeOf<T>();
            if (buffer.Length < sizeOfT)
            {
                ArchiveSerializationException.ThrowInvalidRange(sizeOfT, (int)buffer.Length);
            }
            var slice = buffer.Slice(0, sizeOfT);

            if (slice.IsSingleSegment)
            {
                value = Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(buffer.FirstSpan));
                if (byteSwapping)
                    BinaryHandling.ReverseEndianness(ref value);
                return sizeOfT;
            }

            byte[]? tempArray = null;

            var tempSpan = sizeOfT <= 512 ? stackalloc byte[sizeOfT] : default;
            try
            {
                if (sizeOfT > 512)
                {
                    tempArray = ArrayPool<byte>.Shared.Rent(sizeOfT);
                    tempSpan = tempArray;
                }

                slice.CopyTo(tempSpan);
                value = Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(tempSpan));
                if (byteSwapping)
                    BinaryHandling.ReverseEndianness(ref value);
                return sizeOfT;
            }
            finally
            {
                if (tempArray is not null)
                    ArrayPool<byte>.Shared.Return(tempArray);
            }
        }

        _threadStaticReaderState ??= new ArchiveSerializerState();
        _threadStaticReaderState.Init(options);

        var reader = new ArchiveReader(buffer, _threadStaticReaderState);
        try
        {
            reader.Read(ref value);
            return reader.ConsumedBytes;
        }
        finally
        {
            reader.Dispose();
            _threadStaticReaderState.Reset();
        }
    }

    public static async ValueTask<T?> DeserializeAsync<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T
    >(Stream stream, ArchiveSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (stream is MemoryStream memoryStream && memoryStream.TryGetBuffer(out var streamBuffer))
        {
            cancellationToken.ThrowIfCancellationRequested();
            T? value = default;
            var bytesRead = Deserialize(streamBuffer.AsSpan(checked((int)memoryStream.Position)), ref value, options);
            memoryStream.Seek(bytesRead, SeekOrigin.Current);
            return value;
        }

        var builder = ReusableReadOnlySequenceBuilderPool.Rent();
        try
        {
            var buffer = ArrayPool<byte>.Shared.Rent(65536); // initial 64K
            var offset = 0;
            do
            {
                if (offset == buffer.Length)
                {
                    builder.Add(buffer, returnToPool: true);
                    buffer = ArrayPool<byte>.Shared.Rent(MathEx.NewArrayCapacity(buffer.Length));
                    offset = 0;
                }

                int read;
                try
                {
                    read = await stream
                        .ReadAsync(buffer.AsMemory(offset, buffer.Length - offset), cancellationToken)
                        .ConfigureAwait(false);
                }
                catch
                {
                    // buffer is not added in builder, so return here.
                    ArrayPool<byte>.Shared.Return(buffer);
                    throw;
                }

                offset += read;

                if (read != 0)
                    continue;
                builder.Add(buffer.AsMemory(0, offset), returnToPool: true);
                break;
            } while (true);

            // If single buffer, we can avoid ReadOnlySequence build cost.
            if (builder.TryGetSingleMemory(out var memory))
            {
                return Deserialize<T>(memory.Span, options);
            }
            else
            {
                var seq = builder.Build();
                var result = Deserialize<T>(seq, options);
                return result;
            }
        }
        finally
        {
            builder.Reset();
        }
    }

    private sealed class SerializeWriterThreadStaticState
    {
        public ReusableLinkedArrayBufferWriter BufferWriter = new(true, true);
        public ArchiveSerializerState State = new();

        public void Init(ArchiveSerializerOptions? options)
        {
            State.Init(options);
        }

        public void Reset()
        {
            BufferWriter.Reset();
            State.Reset();
        }
    }
}
