// // @file IAssetManager.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Strings;

namespace RetroEngine.Assets;

public interface IAssetManager : IDisposable
{
    IEnumerable<IAssetPackage> LoadedPackages { get; }
    IAssetPackage? FindPackage(Name packageName);

    ValueTask LoadPackageAsync(Name packageName, string path, CancellationToken cancellationToken = default);

    void UnloadPackage(Name packageName);
    void UnloadAllPackages();
    Type? GetAssetType(AssetPath path);
    Task CreateAssetAsync(AssetPath path, object asset, CancellationToken cancellationToken = default);

    object LoadAsset(AssetPath path);

    T LoadAsset<T>(AssetPath path)
        where T : class;

    ValueTask<object> LoadAssetAsync(AssetPath path, CancellationToken cancellationToken = default);

    ValueTask<T> LoadAssetAsync<T>(AssetPath path, CancellationToken cancellationToken = default)
        where T : class;

    AssetPath GetAssetPath(object asset);
    bool TryGetAssetPath(object asset, out AssetPath path);
}
