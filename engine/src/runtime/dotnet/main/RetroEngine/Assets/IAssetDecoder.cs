// // @file IAssetDecoder.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Assets;

public interface IAssetDecoder
{
    Name AssetType { get; }

    bool CanCreateFromExtension(ReadOnlySpan<char> extension)
    {
        return false;
    }

    bool TryLoadFromNativeCache(AssetPath path, [NotNullWhen(true)] out Asset? asset)
    {
        asset = null;
        return false;
    }

    Asset Decode(IAssetPackage package, Name assetName, ReadOnlySequence<byte> bytes);

    ValueTask<Asset> DecodeAsync(
        IAssetPackage package,
        Name assetName,
        ReadOnlySequence<byte> bytes,
        CancellationToken cancellationToken = default
    );
}
