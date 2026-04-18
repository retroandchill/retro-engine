// // @file BrotliUtils.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Compression;

namespace MagicArchive.Compression;

internal static class BrotliUtils
{
    public const int WindowBitsMin = 10;
    public const int WindowBitsDefault = 22;
    public const int WindowBitsMax = 24;
    public const int QualityMin = 0;
    public const int QualityDefault = 4;
    public const int QualityMax = 11;
    public const int MaxInputSize = int.MaxValue - 515; // 515 is the max compressed extra bytes

    internal static int GetQualityFromCompressionLevel(CompressionLevel compressionLevel) =>
        compressionLevel switch
        {
            CompressionLevel.NoCompression => QualityMin,
            CompressionLevel.Fastest => 1,
            CompressionLevel.Optimal => QualityDefault,
            CompressionLevel.SmallestSize => QualityMax,
            _ => throw new ArgumentOutOfRangeException(nameof(compressionLevel)),
        };

    // https://github.com/dotnet/runtime/issues/35142
    // BrotliEncoder.GetMaxCompressedLength is broken in .NET 7
    // port from encode.c https://github.com/google/brotli/blob/3914999fcc1fda92e750ef9190aa6db9bf7bdb07/c/enc/encode.c#L1200
    internal static int BrotliEncoderMaxCompressedSize(int inputSize)
    {
        var numLargeBlocks = inputSize >> 14;
        var overhead = 2 + 4 * numLargeBlocks + 3 + 1;
        var result = inputSize + overhead;
        if (inputSize == 0)
            return 2;
        return result < inputSize ? 0 : result;
    }
}
