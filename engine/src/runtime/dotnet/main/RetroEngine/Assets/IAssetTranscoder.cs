// @file IAssetTranscoder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;

namespace RetroEngine.Assets;

public interface IAssetTranscoder : IAssetInterpreter
{
    void Transcode<TBufferWriter>(
        AssetStorageType sourceType,
        AssetStorageType destType,
        scoped ReadOnlySpan<byte> source,
        ReadOnlySpan<char> extension,
        in TBufferWriter writer
    )
        where TBufferWriter : IBufferWriter<byte>
    {
        EncodeAsSource(source, writer);
    }

    protected static void EncodeAsSource<TBufferWriter>(scoped ReadOnlySpan<byte> source, in TBufferWriter writer)
        where TBufferWriter : IBufferWriter<byte>
    {
        var destBuffer = writer.GetSpan(source.Length);
        source.CopyTo(destBuffer);
        writer.Advance(source.Length);
    }
}
