// // @file IAssetDecoder.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Assets;

public interface IAssetDecoder
{
    Name AssetType { get; }

    bool CanCreateFromExtension(ReadOnlySpan<char> extension)
    {
        return false;
    }

    Asset Decode(IAssetPackage package, Name assetName, ReadOnlySequence<byte> bytes);

    ValueTask<Asset> DecodeAsync(
        IAssetPackage package,
        Name assetName,
        ReadOnlySequence<byte> bytes,
        CancellationToken cancellationToken = default
    );

    void Encode<TBufferWriter>(IAssetPackage package, Asset asset, in TBufferWriter writer)
        where TBufferWriter : IBufferWriter<byte>;

    Asset ImportFromFile(IAssetPackage package, Name assetName, IFileInfo stream);

    ValueTask<Asset> ImportFromFileAsync(
        IAssetPackage package,
        Name assetName,
        IFileInfo sourceFile,
        CancellationToken cancellationToken = default
    );
}
