// // @file ArchiveWriterTest.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using MagicArchive.Utilities;

namespace MagicArchive.Test;

public class ArchiveWriterTest
{
    private static readonly ImmutableArray<ByteOrder> ByteOrders = [ByteOrder.LittleEndian, ByteOrder.BigEndian];

    private static IEnumerable GetUnmanagedWriteableValues()
    {
        foreach (var byteOrder in ByteOrders)
        {
            yield return new TestCaseData<byte, ByteOrder>(0x12, byteOrder)
            {
                TypeArgs = [typeof(byte)],
                TestName = $"{byteOrder} byte",
            };
            yield return new TestCaseData<sbyte, ByteOrder>(0x34, byteOrder)
            {
                TypeArgs = [typeof(sbyte)],
                TestName = $"{byteOrder} sbyte",
            };
            yield return new TestCaseData<ushort, ByteOrder>(0x1122, byteOrder)
            {
                TypeArgs = [typeof(ushort)],
                TestName = $"{byteOrder} ushort",
            };
            yield return new TestCaseData<short, ByteOrder>(0x3344, byteOrder)
            {
                TypeArgs = [typeof(short)],
                TestName = $"{byteOrder} short",
            };
            yield return new TestCaseData<uint, ByteOrder>(0x11223344, byteOrder)
            {
                TypeArgs = [typeof(uint)],
                TestName = $"{byteOrder} uint",
            };
            yield return new TestCaseData<int, ByteOrder>(0x55667788, byteOrder)
            {
                TypeArgs = [typeof(int)],
                TestName = $"{byteOrder} int",
            };
            yield return new TestCaseData<ulong, ByteOrder>(0x1122334455667788, byteOrder)
            {
                TypeArgs = [typeof(ulong)],
                TestName = $"{byteOrder} ulong",
            };
            yield return new TestCaseData<long, ByteOrder>(unchecked((long)0x99AABBCCDDEEFF00), byteOrder)
            {
                TypeArgs = [typeof(long)],
                TestName = $"{byteOrder} long",
            };
            yield return new TestCaseData<float, ByteOrder>(128.5f, byteOrder)
            {
                TypeArgs = [typeof(float)],
                TestName = $"{byteOrder} float",
            };
            yield return new TestCaseData<double, ByteOrder>(256.5, byteOrder)
            {
                TypeArgs = [typeof(double)],
                TestName = $"{byteOrder} double",
            };
            yield return new TestCaseData<bool, ByteOrder>(true, byteOrder)
            {
                TypeArgs = [typeof(bool)],
                TestName = $"{byteOrder} bool",
            };
            yield return new TestCaseData<char, ByteOrder>('\xF2', byteOrder)
            {
                TypeArgs = [typeof(char)],
                TestName = $"{byteOrder} char",
            };
        }
    }

    [Test]
    [TestCaseSource(nameof(GetUnmanagedWriteableValues))]
    public void CanWriteBlittableValuesToTheArchive<T>(T value, ByteOrder byteOrder)
        where T : unmanaged
    {
        using var state = ArchiveWriterStatePool.Rent(new ArchiveSerializerOptions { ByteOrder = byteOrder });
        var bufferWriter = new ArrayBufferWriter<byte>(Unsafe.SizeOf<T>());
        var writer = ArchiveWriter.Create(ref bufferWriter, state);
        writer.WriteValue(in value);
        writer.Flush();

        var span = bufferWriter.WrittenSpan;
        Assert.That(span.Length, Is.EqualTo(Unsafe.SizeOf<T>()));

        var rawValue = Unsafe.ReadUnaligned<T>(in span.GetPinnableReference());
        var readValue = byteOrder == ByteOrder.BigEndian ? BlittableMarshalling.ReverseEndianness(rawValue) : rawValue;
        Assert.That(readValue, Is.EqualTo(value));
    }
}
