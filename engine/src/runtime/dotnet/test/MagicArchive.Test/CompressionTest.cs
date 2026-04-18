// // @file CompressionTest.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.IO.Compression;
using System.Text;
using MagicArchive.Compression;
using MagicArchive.Test.Models;
using MagicArchive.Test.Utils;

namespace MagicArchive.Test;

public class CompressionTest
{
    [Test]
    public async Task CompressDecompress()
    {
        var pattern1 = Enumerable
            .Range(1, 1000)
            .Select(_ => string.Concat(Enumerable.Repeat("http://", 1000)))
            .Prepend("hogehogehugahugahugahugahogehoge!")
            .ToArray();

        var pattern2 = new[] { "a", "b", "c" };

        var texts = new[] { pattern1, pattern2 };
        foreach (var text in texts)
        {
            using var brotli = new BrotliCompressor();

            ArchiveSerializer.Serialize(brotli, text);

            var originalSerialized = ArchiveSerializer.Serialize(text);
            var array1 = brotli.ToArray();

            var arrayWriter = new ArrayBufferWriter<byte>();
            brotli.CopyTo(arrayWriter);

            var array2 = arrayWriter.WrittenMemory;

            // check BrotliCompressor ToArray()/CopyTo returns same result.
            Assert.That(array1, Is.EquivalentTo(array2.ToArray()));

            var stream = new MemoryStream();
            await brotli.CopyToAsync(stream);
            Assert.That(stream.ToArray(), Is.EquivalentTo(array2.ToArray()));

            using var decompressor = new BrotliDecompressor();

            var decompressed = decompressor.Decompress(array1);

            var referenceDecompress = ReferenceDecompress(array1);
            var decompressedArray = decompressed.ToArray();

            using (Assert.EnterMultipleScope())
            {
                // check decompress results correct
                Assert.That(referenceDecompress, Is.EquivalentTo(decompressedArray));
                Assert.That(originalSerialized, Is.EquivalentTo(decompressed.ToArray()));
            }

            // deserialized check
            var more = ArchiveSerializer.Deserialize<string[]>(decompressed);
            Assert.That(text, Has.Length.EqualTo(more!.Length));
            foreach (var (first, second) in text.Zip(more))
            {
                Assert.That(first, Is.EquivalentTo(second));
            }
        }
    }

    [Test]
    public void AttributeCompression()
    {
        // pattern1, huge compression
        var pattern1 = Enumerable
            .Range(1, 1000)
            .Select(_ => string.Concat(Enumerable.Repeat("http://", 1000)))
            .Prepend("hogehogehugahugahugahugahogehoge!")
            .ToArray();

        // pattern2, small compression
        var pattern2 = new[] { "a", "b", "c" };

        foreach (var pattern in new[] { pattern1, pattern2 })
        {
            var data = new CompressionAttrData()
            {
                Id1 = 14141,
                Data = Encoding.UTF8.GetBytes(string.Concat(pattern)),
                String = string.Concat(pattern),
                Id2 = 99999,
            };

            var bin = ArchiveSerializer.Serialize(data);
            var v2 = ArchiveSerializer.Deserialize<CompressionAttrData>(bin)!;

            Assert.That(v2, Is.Not.Null);
            using var scope = Assert.EnterMultipleScope();
            Assert.That(v2.Id1, Is.EqualTo(data.Id1));
            Assert.That(v2.Id2, Is.EqualTo(data.Id2));
            Assert.That(v2.Data, Is.EquivalentTo(data.Data));
            Assert.That(v2.String, Is.EqualTo(data.String));
        }
    }

    [Test]
    public void AttributeCompression2()
    {
        // pattern1, huge compression
        var pattern1 = Enumerable
            .Range(1, 1000)
            .Select(_ => string.Concat(Enumerable.Repeat("http://", 1000)))
            .Prepend("hogehogehugahugahugahugahogehoge!")
            .ToArray();

        // pattern2, small compression
        var pattern2 = new[] { "a", "b", "c" };

        foreach (var pattern in new[] { pattern1, pattern2 })
        {
            var data = new CompressionAttrData2()
            {
                Id1 = 14141,
                Data = Encoding.UTF8.GetBytes(string.Concat(pattern)),
                Two = new StandardTypeTwo { One = 9999, Two = 1111 },
                String = string.Concat(pattern),
                Id2 = 99999,
            };

            var bin = ArchiveSerializer.Serialize(data);

            {
                var v2 = ArchiveSerializer.Deserialize<CompressionAttrData2>(bin)!;

                using var scope = Assert.EnterMultipleScope();
                Assert.That(v2.Id1, Is.EqualTo(data.Id1));
                Assert.That(v2.Id2, Is.EqualTo(data.Id2));
                Assert.That(v2.Data, Is.EquivalentTo(data.Data));
                Assert.That(v2.String, Is.EqualTo(data.String));

                Assert.That(v2.Two.One, Is.EqualTo(data.Two.One));
                Assert.That(v2.Two.Two, Is.EqualTo(data.Two.Two));
            }
            {
                var seq = ReadOnlySequenceBuilder.Create(bin.Chunk(bin.Length / 5).ToArray());

                var v2 = ArchiveSerializer.Deserialize<CompressionAttrData2>(seq)!;

                using var scope = Assert.EnterMultipleScope();
                Assert.That(v2.Id1, Is.EqualTo(data.Id1));
                Assert.That(v2.Id2, Is.EqualTo(data.Id2));
                Assert.That(v2.Data, Is.EquivalentTo(data.Data));
                Assert.That(v2.String, Is.EqualTo(data.String));

                Assert.That(v2.Two.One, Is.EqualTo(data.Two.One));
                Assert.That(v2.Two.Two, Is.EqualTo(data.Two.Two));
            }
        }
    }

    private static byte[] ReferenceDecompress(byte[] bytes)
    {
        using var ms = new MemoryStream(bytes);
        using var brotli = new BrotliStream(ms, CompressionMode.Decompress);
        var dest = new MemoryStream();
        brotli.CopyTo(dest);
        return dest.ToArray();
    }
}
