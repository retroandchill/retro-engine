// // @file PrimitiveTest.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using FluentAssertions;

namespace MagicArchive.Test;

public class PrimitiveTest
{
    [Test]
    public void ArrayWriterInt()
    {
        var buffer = new ArrayBufferWriter<byte>(1024);

        ArchiveSerializer.Serialize(buffer, 123);

        buffer.WrittenCount.Should().Be(4);

        var i = ArchiveSerializer.Deserialize<int>(buffer.WrittenSpan);
        i.Should().Be(123);
    }

    [Test]
    public void NoneGenericInt()
    {
        var bin = ArchiveSerializer.Serialize(123);
        var i = ArchiveSerializer.Deserialize<int>(bin);
        i.Should().Be(123);

#pragma warning disable CA2263
        var j = (int)ArchiveSerializer.Deserialize(typeof(int), bin)!;
#pragma warning restore CA2263
        j.Should().Be(123);
    }
}
