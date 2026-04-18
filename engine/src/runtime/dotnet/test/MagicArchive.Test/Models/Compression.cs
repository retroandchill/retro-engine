// // @file Compression.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MagicArchive.Compression;

namespace MagicArchive.Test.Models;

[Archivable]
public partial class CompressionAttrData
{
    public int Id1 { get; set; }

    [BrotliFormatter]
    public byte[] Data { get; set; } = null!;

    [BrotliStringFormatter]
    public string String { get; set; } = null!;

    public int Id2 { get; set; }
}

[Archivable]
public partial class CompressionAttrData2
{
    public int Id1 { get; set; }

    [BrotliFormatter]
    public byte[] Data { get; set; } = null!;

    [BrotliStringFormatter]
    public string String { get; set; } = null!;

    [BrotliFormatter<StandardTypeTwo>]
    public StandardTypeTwo Two { get; set; } = null!;

    public int Id2 { get; set; }
}

[Archivable]
public partial class SaveData
{
    public byte[] Areas = new byte[10000000];

    public SaveData()
    {
        var rnd = new Random(1000);
        for (int i = 0; i < Areas.Length; ++i)
        {
            // if (rnd.Next() % 2 != 0) continue;
            Areas[i] = (byte)(rnd.Next() % 256);
        }
    }

    public byte[] MemCmpSerialize()
    {
        using var cp = new BrotliCompressor();
        ArchiveSerializer.Serialize(cp, this);
        return cp.ToArray();
    }

    public bool MemDecmpDeserialize(byte[] bin)
    {
        try
        {
            using var dcp = new BrotliDecompressor();
            var buffer = dcp.Decompress(bin);
            var data = ArchiveSerializer.Deserialize<SaveData>(buffer);
            if (data is null)
                return false;
            Array.Copy(data.Areas, Areas, data.Areas.Length);
        }
        catch
        {
            return false;
        }
        return true;
    }
}
