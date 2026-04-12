// // @file PrimitiveTest.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;

namespace MagicArchive.Test;

public class PrimitiveTest
{
    [Test]
    public void ArrayWriterInt()
    {
        var buffer = new ArrayBufferWriter<byte>(1024);

        ArchiveSerializer.Serialize(buffer, 123);

        Assert.That(buffer.WrittenCount, Is.EqualTo(4));

        var i = ArchiveSerializer.Deserialize<int>(buffer.WrittenSpan);
        Assert.That(i, Is.EqualTo(123));
    }

    [Test]
    public void NoneGenericInt()
    {
        var bin = ArchiveSerializer.Serialize(123);
        var i = ArchiveSerializer.Deserialize<int>(bin);
        Assert.That(i, Is.EqualTo(123));

#pragma warning disable CA2263
        var j = (int)ArchiveSerializer.Deserialize(typeof(int), bin)!;
#pragma warning restore CA2263
        Assert.That(j, Is.EqualTo(123));
    }
}
