// @file IAssetCache.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace RetroEngine.Assets;

public interface IAssetCache
{
    object Get(AssetPath path);

    bool TryGet(AssetPath path, [NotNullWhen(true)] out object? asset);

    AssetPath GetPath(object asset);

    bool TryGetPath(object asset, out AssetPath path);

    object GetOrAdd(AssetPath path, Func<AssetPath, object> factory);

    object GetOrAdd<TContext>(AssetPath path, TContext context, Func<AssetPath, TContext, object> factory)
        where TContext : allows ref struct;

    ValueTask<object> GetOrAddAsync(
        AssetPath path,
        Func<AssetPath, CancellationToken, ValueTask<object>> factory,
        CancellationToken cancellationToken = default
    );

    ValueTask<object> GetOrAddAsync<TContext>(
        AssetPath path,
        TContext context,
        Func<AssetPath, TContext, CancellationToken, ValueTask<object>> factory,
        CancellationToken cancellationToken = default
    );

    void Set(AssetPath path, object asset);

    void Rename(AssetPath oldPath, AssetPath newPath);

    void Remove(AssetPath path);

    void Clear();

    void RemoveInvalidAssets();
}
