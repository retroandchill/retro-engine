// // @file AssetManager.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace RetroEngine.Assets;

[RegisterSingleton]
public sealed class AssetManager(IEnumerable<IAssetDecoder> decoders)
{
    private readonly ImmutableArray<IAssetDecoder> _decoders = [.. decoders];
    private readonly ConcurrentDictionary<AssetPath, WeakReference<Asset>> _assetCache = new();
}
