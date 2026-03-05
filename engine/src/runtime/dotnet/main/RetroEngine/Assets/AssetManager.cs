// // @file AssetManager.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using RetroEngine.Portable.Strings;
using Zomp.SyncMethodGenerator;

namespace RetroEngine.Assets;

[RegisterSingleton]
public sealed partial class AssetManager(
    ILogger<AssetManager> logger,
    IEnumerable<IAssetPackage> packages,
    IEnumerable<IAssetDecoder> decoders
)
{
    private readonly ImmutableDictionary<Name, IAssetPackage> _packages = packages.ToImmutableDictionary(x =>
        x.PackageName
    );

    private readonly ImmutableDictionary<Name, IAssetDecoder> _decoders = decoders.ToImmutableDictionary(x =>
        x.AssetType
    );

    private readonly ConcurrentDictionary<AssetPath, Task<Asset?>> _loadingTasks = new();
    private readonly ConcurrentDictionary<AssetPath, WeakReference<Asset>> _assetCache = new();

    public Asset? LoadAsset(AssetPath path)
    {
        if (_assetCache.TryGetValue(path, out var asset) && asset.TryGetTarget(out var cachedAsset))
        {
            return cachedAsset;
        }

        return LoadAssetInternal(path);
    }

    public async ValueTask<Asset?> LoadAssetAsync(AssetPath path, CancellationToken cancellationToken = default)
    {
        if (_assetCache.TryGetValue(path, out var asset) && asset.TryGetTarget(out var cachedAsset))
        {
            return cachedAsset;
        }

        var loadTask = _loadingTasks.GetOrAdd(path, _ => LoadAssetInternalAsync(path, cancellationToken));
        try
        {
            return await loadTask;
        }
        finally
        {
            _loadingTasks.TryRemove(path, out _);
        }
    }

    [CreateSyncVersion]
    private async Task<Asset?> LoadAssetInternalAsync(AssetPath path, CancellationToken cancellationToken)
    {
        if (!_packages.TryGetValue(path.PackageName, out var package))
        {
            logger.LogWarning("Package '{PathPackageName}' not found.", path.PackageName);
            return null;
        }

        if (!package.HasAsset(path.AssetName))
        {
            logger.LogWarning(
                "Asset '{AssetName}' not found in package '{PackageName}'",
                path.AssetName,
                path.PackageName
            );
            return null;
        }

        var assetType = await package.GetAssetTypeAsync(path.AssetName, cancellationToken);
        if (!_decoders.TryGetValue(assetType, out var decoder))
        {
            logger.LogError("No decoder found for asset type '{AssetType}'", assetType);
            return null;
        }

        var decodeContext = new AssetDecodeContext(path);
        if (decoder.TryLoadFromNativeCache(decodeContext, out var cachedAsset))
        {
            _assetCache[path] = new WeakReference<Asset>(cachedAsset);
            return cachedAsset;
        }

        await using var assetStream = package.OpenAsset(path.AssetName);
        var decoded = await decoder.DecodeAsync(decodeContext, assetStream, cancellationToken);
        _assetCache[path] = new WeakReference<Asset>(decoded);
        return decoded;
    }
}
