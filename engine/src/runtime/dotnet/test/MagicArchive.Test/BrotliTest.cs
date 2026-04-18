// // @file BrotliTest.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.IO.Compression;
using MagicArchive.Compression;
using MagicArchive.Test.Models;

namespace MagicArchive.Test;

public class BrotliTest
{
    [Test]
    public void LargeByteArray()
    {
        var data = new SaveData();

        var bin = data.MemCmpSerialize();
        Assert.That(data.MemDecmpDeserialize(bin), Is.True);
    }

    [Test]
    public void EncodeEmptyContent()
    {
        var buffer = new ArrayBufferWriter<byte>();
        using var state = ArchiveWriterStatePool.Rent(null);
        var writer = ArchiveWriter.Create(ref buffer, state);

        using var compressor = new BrotliCompressor(CompressionLevel.Fastest);
        compressor.CopyTo(ref writer);

        using var decompressor = new BrotliDecompressor();
        Assert.That(decompressor.Decompress(compressor.ToArray()).ToArray(), Is.Empty);
    }

    [Test]
    public void EncodeEmptyFinalBlock()
    {
        using var state = ArchiveWriterStatePool.Rent(null);

        var compressor = new BrotliCompressor(CompressionLevel.Fastest);
        var coWriter = ArchiveWriter.Create(ref compressor, state);

        var bytes = new byte[248];
        Random.Shared.NextBytes(bytes);
        coWriter.WriteArray(bytes);
        coWriter.Flush();

        var buffer = new ArrayBufferWriter<byte>();
        compressor.CopyTo(buffer);

        using var readerState = ArchiveReaderStatePool.Rent(null);
        using var decompressor = new BrotliDecompressor();
        var decompressed = decompressor.Decompress(compressor.ToArray());
        var reader = new ArchiveReader(in decompressed, readerState);

        Assert.That(reader.ReadArray<byte>(), Is.EquivalentTo(bytes));
        compressor.Dispose();
    }
}
