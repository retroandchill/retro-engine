// // @file AssetManager.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using MagicArchive.Utilities;
using Microsoft.Extensions.Logging;
using RetroEngine.Portable.Strings;
using Zomp.SyncMethodGenerator;

namespace RetroEngine.Assets;

[RegisterSingleton]
public sealed partial class AssetManager(
    ILogger<AssetManager> logger,
    IEnumerable<IAssetPackageFactory> packageFactories,
    IEnumerable<IAssetDecoder> decoders
)
{
    private readonly ImmutableArray<IAssetPackageFactory> _packageFactories = [.. packageFactories];
    private readonly ConcurrentDictionary<Name, IAssetPackage> _packages = new();

    private readonly ImmutableDictionary<Name, IAssetDecoder> _decoders = decoders.ToImmutableDictionary(x =>
        x.AssetType
    );

    private readonly ConcurrentDictionary<AssetPath, SemaphoreSlim> _loadingSemaphores = new();
    private readonly ConcurrentDictionary<AssetPath, WeakReference<Asset>> _assetCache = new();

    public async ValueTask LoadPackageAsync(
        Name packageName,
        string path,
        CancellationToken cancellationToken = default
    )
    {
        var factory = _packageFactories.FirstOrDefault(x => x.CanCreate(packageName, path));
        if (factory is null)
            throw new AssetLoadException($"No package factory found for package '{packageName}'");

        var package = factory.Create(packageName, path);
        if (!_packages.TryAdd(packageName, package))
            throw new AssetLoadException($"Package '{packageName}' already exists");

        await package.LoadAsync(cancellationToken);
    }

    public void UnloadPackage(Name packageName)
    {
        if (_packages.Remove(packageName, out var package))
        {
            package.Unload();
        }
        else
        {
            throw new AssetLoadException($"Package '{packageName}' does not exist");
        }
    }

    public void UnloadAllPackages()
    {
        foreach (var package in _packages.Values)
        {
            package.Unload();
        }
        _packages.Clear();
    }

    [CreateSyncVersion]
    public async ValueTask<Asset?> LoadAssetAsync(AssetPath path, CancellationToken cancellationToken = default)
    {
        if (_assetCache.TryGetValue(path, out var asset) && asset.TryGetTarget(out var cachedAsset))
        {
            return cachedAsset;
        }

        var semaphore = _loadingSemaphores.GetOrAdd(path, _ => new SemaphoreSlim(1));
#if SYNC_ONLY
        semaphore.Wait();
#else
        await semaphore.WaitAsync(cancellationToken);
#endif

        try
        {
            if (_assetCache.TryGetValue(path, out asset) && asset.TryGetTarget(out cachedAsset))
            {
                return cachedAsset;
            }

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

            var assetType = package.GetAssetType(path.AssetName);
            if (!_decoders.TryGetValue(assetType, out var decoder))
            {
                logger.LogError("No decoder found for asset type '{AssetType}'", assetType);
                return null;
            }

            if (decoder.TryLoadFromNativeCache(path, out cachedAsset))
            {
                _assetCache[path] = new WeakReference<Asset>(cachedAsset);
                return cachedAsset;
            }

            await using var stream = package.OpenAsset(path.AssetName);
            var builder = ReusableReadOnlySequenceBuilderPool.Rent();
            try
            {
                await builder.ReadFromStreamAsync(stream, cancellationToken);

                var decoded = await decoder.DecodeAsync(package, path.AssetName, builder.Build(), cancellationToken);
                _assetCache[path] = new WeakReference<Asset>(decoded);
                return decoded;
            }
            finally
            {
                ReusableReadOnlySequenceBuilderPool.Return(builder);
            }
        }
        finally
        {
            semaphore.Release();
        }
    }
}
