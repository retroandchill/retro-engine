// // @file IAssetDecoder.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Assets;

public enum AssetStorageType
{
    File,
    Packaged,
}

public interface IAssetDecoder : IAssetInterpreter
{
    object Decode(AssetStorageType type, scoped ReadOnlySpan<byte> source);

    ValueTask<object> DecodeAsync(
        AssetStorageType type,
        ReadOnlyMemory<byte> source,
        CancellationToken cancellationToken = default
    )
    {
        return !cancellationToken.IsCancellationRequested
            ? new ValueTask<object>(Decode(type, source.Span))
            : ValueTask.FromCanceled<object>(cancellationToken);
    }
}
