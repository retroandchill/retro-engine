// // @file ArchiveReaderTest.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Runtime.InteropServices;

namespace MagicArchive.Test;

public class ArchiveReaderTest
{
    [Test]
    public void ByteSwappingWorksAsExpected()
    {
        const ulong writtenValue = 0x8877665544332211;
        const ulong expectedValue = 0x1122334455667788;

        Span<byte> bytes = stackalloc byte[8];
        MemoryMarshal.AsRef<ulong>(bytes) = writtenValue;
        var readValue = ArchiveSerializer.Deserialize<ulong>(
            bytes,
            new ArchiveSerializerOptions { ByteOrder = ByteOrder.BigEndian }
        );
        Assert.That(readValue, Is.EqualTo(expectedValue));
    }

    [Test]
    [TestCase(ByteOrder.LittleEndian)]
    [TestCase(ByteOrder.BigEndian)]
    public void WriteThenReadBack(ByteOrder byteOrder)
    {
        const byte testValueU8 = 0x12;
        const sbyte testValueS8 = 0x34;
        const ushort testValueU16 = 0x1122;
        const short testValueS16 = 0x3344;
        const uint testValueU32 = 0x11223344;
        const int testValueS32 = 0x55667788;
        const ulong testValueU64 = 0x1122334455667788;
        const long testValueS64 = unchecked((long)0x99AABBCCDDEEFF00);
        const float testValueF = 128.5f;
        const double testValueD = 256.5;
        const bool testValueB = true;
        const char testValueCh = '\xF2';
        const string testAnsiStr = "Joe";
        const string testUtf16Str = "\uC11C\uC6B8\uC0AC\uB78C";

        var bufferWriter = new ArrayBufferWriter<byte>();
        var settings = ArchiveSerializerStatePool.Rent(new ArchiveSerializerOptions { ByteOrder = byteOrder });

        var writer = new ArchiveWriter<ArrayBufferWriter<byte>>(ref bufferWriter, settings);
        writer.Write(testValueU8);
        writer.Write(testValueS8);
        writer.Write(testValueU16);
        writer.Write(testValueS16);
        writer.Write(testValueU32);
        writer.Write(testValueS32);
        writer.Write(testValueU64);
        writer.Write(testValueS64);
        writer.Write(testValueF);
        writer.Write(testValueD);
        writer.Write(testValueB);
        writer.Write(testValueCh);
        writer.Write(testAnsiStr);
        writer.Write(testUtf16Str);
        writer.Flush();

        var reader = new ArchiveReader(bufferWriter.WrittenSpan, settings);
        using var scope = Assert.EnterMultipleScope();
        Assert.That(reader.ReadByte(), Is.EqualTo(testValueU8));
        Assert.That(reader.ReadSByte(), Is.EqualTo(testValueS8));
        Assert.That(reader.ReadUInt16(), Is.EqualTo(testValueU16));
        Assert.That(reader.ReadInt16(), Is.EqualTo(testValueS16));
        Assert.That(reader.ReadUInt32(), Is.EqualTo(testValueU32));
        Assert.That(reader.ReadInt32(), Is.EqualTo(testValueS32));
        Assert.That(reader.ReadUInt64(), Is.EqualTo(testValueU64));
        Assert.That(reader.ReadInt64(), Is.EqualTo(testValueS64));
        Assert.That(reader.ReadSingle(), Is.EqualTo(testValueF));
        Assert.That(reader.ReadDouble(), Is.EqualTo(testValueD));
        Assert.That(reader.ReadBool(), Is.EqualTo(testValueB));
        Assert.That(reader.ReadChar(), Is.EqualTo(testValueCh));
        Assert.That(reader.ReadString(), Is.EqualTo(testAnsiStr));
        Assert.That(reader.ReadString(), Is.EqualTo(testUtf16Str));
    }
}
