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
    IAssetCache assetCache,
    IEnumerable<IAssetPackageFactory> packageFactories,
    IEnumerable<IAssetDecoder> decoders
) : IAssetManager
{
    private readonly ImmutableArray<IAssetPackageFactory> _packageFactories = [.. packageFactories];
    private readonly ConcurrentDictionary<Name, IAssetPackage> _packages = new();

    private readonly ImmutableDictionary<Type, IAssetDecoder> _decoders = decoders.ToImmutableDictionary(x =>
        x.AssetType
    );

    public IEnumerable<IAssetPackage> LoadedPackages =>
        _packages.Values.OrderBy(x => x.PackageName, NameComparer.CaseInsensitive);

    public IAssetPackage? FindPackage(Name packageName) => _packages.GetValueOrDefault(packageName);

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

        package.OnEntriesRefreshed += OnAssetEntriesChanged;
    }

    private void OnAssetEntriesChanged(in AssetPackageChangeManifest manifest)
    {
        foreach (var (oldEntry, newEntry) in manifest.RenamedEntries)
        {
            assetCache.Rename(
                new AssetPath(manifest.Package.PackageName, oldEntry.Name),
                new AssetPath(manifest.Package.PackageName, newEntry.Name)
            );
        }

        foreach (var entry in manifest.RemovedEntries)
        {
            assetCache.Remove(new AssetPath(manifest.Package.PackageName, entry.Name));
        }
    }

    public void UnloadPackage(Name packageName)
    {
        if (_packages.Remove(packageName, out var package))
        {
            package.Unload();
            package.OnEntriesRefreshed -= OnAssetEntriesChanged;
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
            package.OnEntriesRefreshed -= OnAssetEntriesChanged;
        }
        _packages.Clear();
    }

    public Type? GetAssetType(AssetPath path)
    {
        if (_packages.TryGetValue(path.PackageName, out var package))
            return package.GetAssetType(path.AssetName);

        logger.LogWarning("Package '{PathPackageName}' not found.", path.PackageName);
        return null;
    }

    public async Task CreateAssetAsync(AssetPath path, object asset, CancellationToken cancellationToken = default)
    {
        if (!_packages.TryGetValue(path.PackageName, out var package))
        {
            throw new AssetLoadException($"Package '{path.PackageName}' not found");
        }

        if (package is not IEditableAssetPackage editablePackage)
        {
            throw new AssetLoadException($"Package '{path.PackageName}' is not editable");
        }

        await editablePackage.AddAssetAsync(path.AssetName, asset, cancellationToken);
        assetCache.Set(path, asset);
    }

    [CreateSyncVersion]
    public ValueTask<object> LoadAssetAsync(AssetPath path, CancellationToken cancellationToken = default)
    {
        return assetCache.GetOrAddAsync(path, this, (p, t, c) => t.LoadAssetInternalAsync(p, c), cancellationToken);
    }

    [CreateSyncVersion]
    private async ValueTask<object> LoadAssetInternalAsync(AssetPath path, CancellationToken cancellationToken)
    {
        if (!_packages.TryGetValue(path.PackageName, out var package))
        {
            throw new AssetLoadException($"Package '{path.PackageName}' not found");
        }

        if (!package.HasAsset(path.AssetName))
        {
            throw new AssetLoadException($"Asset '{path.AssetName}' not found in package '{path.PackageName}'");
        }

        var assetType = package.GetAssetType(path.AssetName);
        if (!_decoders.TryGetValue(assetType, out var decoder))
        {
            throw new AssetLoadException($"No decoder found for asset type '{assetType}'");
        }

        await using var stream = package.OpenAsset(path.AssetName);
        using var builder = AssetReadBufferPool.Rent();
        await builder.ReadFromStreamAsync(stream, cancellationToken).ConfigureAwait(false);
#if SYNC_ONLY
        return decoder.Decode(AssetStorageType.File, builder.Span);
#else
        return await decoder
            .DecodeAsync(AssetStorageType.File, builder.Memory, cancellationToken)
            .ConfigureAwait(false);
#endif
    }

    [CreateSyncVersion]
    public async ValueTask<T> LoadAssetAsync<T>(AssetPath path, CancellationToken cancellationToken = default)
        where T : class
    {
        return (T)await LoadAssetAsync(path, cancellationToken);
    }

    public AssetPath GetAssetPath(object asset)
    {
        return assetCache.GetPath(asset);
    }

    public bool TryGetAssetPath(object asset, out AssetPath path)
    {
        return assetCache.TryGetPath(asset, out path);
    }

    public void Dispose()
    {
        foreach (var package in _packages.Values.OfType<IDisposable>())
        {
            package.Dispose();
        }
    }
}
