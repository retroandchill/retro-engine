// @file IAssetEncoder.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;

namespace RetroEngine.Assets;

public interface IAssetEncoder : IAssetTranscoder
{
    void Encode<TBufferWriter>(
        AssetStorageType sourceType,
        object asset,
        ReadOnlySpan<char> extension,
        in TBufferWriter writer
    )
        where TBufferWriter : IBufferWriter<byte>;
}

public interface IAssetEncoder<in T> : IAssetEncoder
    where T : class
{
    void Encode<TBufferWriter>(
        AssetStorageType sourceType,
        T asset,
        ReadOnlySpan<char> extension,
        in TBufferWriter writer
    )
        where TBufferWriter : IBufferWriter<byte>;
}
