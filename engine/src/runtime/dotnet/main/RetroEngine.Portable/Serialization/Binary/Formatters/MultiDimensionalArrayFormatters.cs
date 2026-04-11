// // @file MultiDimentionalArrayFormatter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RetroEngine.Portable.Serialization.Binary.Utilities;

namespace RetroEngine.Portable.Serialization.Binary.Formatters;

public class TwoDimensionalArrayFormatter<T> : ArchiveFormatter<T[,]?>
{
    private const int HeaderValue = 3;

    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in T[,]? value)
    {
        if (value is null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(HeaderValue);
        var iLength = value.GetLength(0);
        var jLength = value.GetLength(1);
        writer.Write(iLength, jLength);

        if (!writer.IsByteSwapping && BinaryHandling.IsBlittable<T>())
        {
            var byteCount = Unsafe.SizeOf<T>() * iLength * jLength;
            ref var src = ref MemoryMarshal.GetArrayDataReference(value);
            ref var dest = ref writer.GetSpanReference(byteCount);

            Unsafe.CopyBlockUnaligned(ref dest, ref src, (uint)byteCount);
            writer.Advance(byteCount);
        }
        else
        {
            writer.WriteCollectionHeader(value.Length);
            var formatter = writer.GetFormatter<T>();
            foreach (var item in value)
            {
                formatter.Serialize(ref writer, in item);
            }
        }
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref T[,]? value)
    {
        if (!reader.TryReadObjectHeader(out var propertyCount))
        {
            value = null;
            return;
        }

        if (propertyCount != HeaderValue)
        {
            ArchiveSerializationException.ThrowInvalidPropertyCount(HeaderValue, propertyCount);
        }

        reader.Read(out int iLength, out int jLength);

        if (value is null || value.GetLength(0) != iLength || value.GetLength(1) != jLength)
        {
            value = new T[iLength, jLength];
        }

        if (!reader.IsByteSwapping && BinaryHandling.IsBlittable<T>())
        {
            var byteCount = Unsafe.SizeOf<T>() * iLength * jLength;
            ref var dest = ref MemoryMarshal.GetArrayDataReference(value);
            ref var src = ref reader.GetSpanReference(byteCount);
            Unsafe.CopyBlockUnaligned(ref dest, ref src, (uint)byteCount);
            reader.Advance(byteCount);
        }
        else
        {
            var formatter = reader.GetFormatter<T>();
            var length = iLength * jLength;
            var i = 0;
            var j = -1;
            var count = 0;
            while (count++ < length)
            {
                if (j < jLength - 1)
                {
                    j++;
                }
                else
                {
                    j = 0;
                    i++;
                }

                formatter.Deserialize(ref reader, ref value[i, j]);
            }
        }
    }
}

public class ThreeDimensionalArrayFormatter<T> : ArchiveFormatter<T[,,]?>
{
    private const int HeaderValue = 4;

    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in T[,,]? value)
    {
        if (value is null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(HeaderValue);
        var iLength = value.GetLength(0);
        var jLength = value.GetLength(1);
        var kLength = value.GetLength(2);
        writer.Write(iLength, jLength, kLength);

        if (!writer.IsByteSwapping && BinaryHandling.IsBlittable<T>())
        {
            var byteCount = Unsafe.SizeOf<T>() * iLength * jLength * kLength;
            ref var src = ref MemoryMarshal.GetArrayDataReference(value);
            ref var dest = ref writer.GetSpanReference(byteCount);

            Unsafe.CopyBlockUnaligned(ref dest, ref src, (uint)byteCount);
            writer.Advance(byteCount);
        }
        else
        {
            writer.WriteCollectionHeader(value.Length);
            var formatter = writer.GetFormatter<T>();
            foreach (var item in value)
            {
                formatter.Serialize(ref writer, in item);
            }
        }
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref T[,,]? value)
    {
        if (!reader.TryReadObjectHeader(out var propertyCount))
        {
            value = null;
            return;
        }

        if (propertyCount != HeaderValue)
        {
            ArchiveSerializationException.ThrowInvalidPropertyCount(HeaderValue, propertyCount);
        }

        reader.Read(out int iLength, out int jLength, out int kLength);

        if (
            value is null
            || value.GetLength(0) != iLength
            || value.GetLength(1) != jLength
            || value.GetLength(2) != kLength
        )
        {
            value = new T[iLength, jLength, kLength];
        }

        if (!reader.IsByteSwapping && BinaryHandling.IsBlittable<T>())
        {
            var byteCount = Unsafe.SizeOf<T>() * iLength * jLength;
            ref var dest = ref MemoryMarshal.GetArrayDataReference(value);
            ref var src = ref reader.GetSpanReference(byteCount);
            Unsafe.CopyBlockUnaligned(ref dest, ref src, (uint)byteCount);
            reader.Advance(byteCount);
        }
        else
        {
            var formatter = reader.GetFormatter<T>();
            var length = iLength * jLength;
            var i = 0;
            var j = -1;
            var k = -1;
            var count = 0;
            while (count++ < length)
            {
                if (k < kLength - 1)
                {
                    k++;
                }
                else if (j < jLength - 1)
                {
                    k = 0;
                    j++;
                }
                else
                {
                    k = 0;
                    j = 0;
                    i++;
                }

                formatter.Deserialize(ref reader, ref value[i, j, k]);
            }
        }
    }
}

public class FourDimensionalArrayFormatter<T> : ArchiveFormatter<T[,,,]?>
{
    private const int HeaderValue = 5;

    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in T[,,,]? value)
    {
        if (value is null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(HeaderValue);
        var iLength = value.GetLength(0);
        var jLength = value.GetLength(1);
        var kLength = value.GetLength(2);
        var lLength = value.GetLength(3);
        writer.Write(iLength, jLength, kLength, lLength);

        if (!writer.IsByteSwapping && BinaryHandling.IsBlittable<T>())
        {
            var byteCount = Unsafe.SizeOf<T>() * iLength * jLength * kLength;
            ref var src = ref MemoryMarshal.GetArrayDataReference(value);
            ref var dest = ref writer.GetSpanReference(byteCount);

            Unsafe.CopyBlockUnaligned(ref dest, ref src, (uint)byteCount);
            writer.Advance(byteCount);
        }
        else
        {
            writer.WriteCollectionHeader(value.Length);
            var formatter = writer.GetFormatter<T>();
            foreach (var item in value)
            {
                formatter.Serialize(ref writer, in item);
            }
        }
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref T[,,,]? value)
    {
        if (!reader.TryReadObjectHeader(out var propertyCount))
        {
            value = null;
            return;
        }

        if (propertyCount != HeaderValue)
        {
            ArchiveSerializationException.ThrowInvalidPropertyCount(HeaderValue, propertyCount);
        }

        reader.Read(out int iLength, out int jLength, out int kLength, out int lLength);

        if (
            value is null
            || value.GetLength(0) != iLength
            || value.GetLength(1) != jLength
            || value.GetLength(2) != kLength
            || value.GetLength(3) != lLength
        )
        {
            value = new T[iLength, jLength, kLength, lLength];
        }

        if (!reader.IsByteSwapping && BinaryHandling.IsBlittable<T>())
        {
            var byteCount = Unsafe.SizeOf<T>() * iLength * jLength;
            ref var dest = ref MemoryMarshal.GetArrayDataReference(value);
            ref var src = ref reader.GetSpanReference(byteCount);
            Unsafe.CopyBlockUnaligned(ref dest, ref src, (uint)byteCount);
            reader.Advance(byteCount);
        }
        else
        {
            var formatter = reader.GetFormatter<T>();
            var length = iLength * jLength;
            var i = 0;
            var j = -1;
            var k = -1;
            var l = -1;
            var count = 0;
            while (count++ < length)
            {
                if (l < lLength - 1)
                {
                    l++;
                }
                else if (k < kLength - 1)
                {
                    l = 0;
                    k++;
                }
                else if (j < jLength - 1)
                {
                    l = 0;
                    k = 0;
                    j++;
                }
                else
                {
                    l = 0;
                    k = 0;
                    j = 0;
                    i++;
                }

                formatter.Deserialize(ref reader, ref value[i, j, k, l]);
            }
        }
    }
}
