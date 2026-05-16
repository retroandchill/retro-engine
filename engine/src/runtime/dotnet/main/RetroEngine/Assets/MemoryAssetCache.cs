// @file MemoryAssetCache.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using RetroEngine.Utilities.Collections;
using RetroEngine.Utilities.Concurrency;
using RetroEngine.Utilities.Memory;

namespace RetroEngine.Assets;

[RegisterSingleton]
public sealed class MemoryAssetCache : IAssetCache, IDisposable
{
    private readonly StripedDictionary<AssetPath, WeakReference> _assets = new(32);
    private readonly ConcurrentDictionary<AssetPath, SemaphoreSlim> _semaphores = new();
    private readonly ConditionalWeakTable<object, ReadOnlyBox<AssetPath>> _assetPaths = new();

    public object Get(AssetPath path)
    {
        return TryGet(path, out var asset) ? asset : throw new KeyNotFoundException();
    }

    public bool TryGet(AssetPath path, [NotNullWhen(true)] out object? asset)
    {
        if (_assets.TryGetOrRemove(path, out var weakReference, w => w.IsAlive))
        {
            asset = weakReference.Target!;
            return true;
        }

        asset = null;
        return false;
    }

    public AssetPath GetPath(object asset)
    {
        return TryGetPath(asset, out var path) ? path : throw new KeyNotFoundException();
    }

    public bool TryGetPath(object asset, out AssetPath path)
    {
        if (_assetPaths.TryGetValue(asset, out var box))
        {
            path = box.Value;
            return true;
        }

        path = default;
        return false;
    }

    public object GetOrAdd(AssetPath path, Func<AssetPath, object> factory)
    {
        if (TryGet(path, out var asset))
        {
            return asset;
        }

        var semaphore = _semaphores.GetOrAdd(path, _ => new SemaphoreSlim(1));

        using var scope = semaphore.EnterScope();
        if (TryGet(path, out asset))
        {
            return asset;
        }

        asset = factory(path);
        if (asset is null)
        {
            throw new InvalidOperationException("Factory returned null for asset path: " + path);
        }

        _assets[path] = new WeakReference(asset);
        _assetPaths.Add(asset, Box.CreateReadOnly(path));
        return asset;
    }

    public object GetOrAdd<TContext>(AssetPath path, TContext context, Func<AssetPath, TContext, object> factory)
        where TContext : allows ref struct
    {
        if (TryGet(path, out var asset))
        {
            return asset;
        }

        var semaphore = _semaphores.GetOrAdd(path, _ => new SemaphoreSlim(1));

        using var scope = semaphore.EnterScope();
        if (TryGet(path, out asset))
        {
            return asset;
        }

        asset = factory(path, context);
        if (asset is null)
        {
            throw new InvalidOperationException("Factory returned null for asset path: " + path);
        }

        _assets[path] = new WeakReference(asset);
        _assetPaths.Add(asset, Box.CreateReadOnly(path));
        return asset;
    }

    public async ValueTask<object> GetOrAddAsync(
        AssetPath path,
        Func<AssetPath, CancellationToken, ValueTask<object>> factory,
        CancellationToken cancellationToken = default
    )
    {
        if (TryGet(path, out var asset))
        {
            return asset;
        }

        var semaphore = _semaphores.GetOrAdd(path, _ => new SemaphoreSlim(1));

        using var scope = await semaphore.EnterScopeAsync(cancellationToken).ConfigureAwait(false);
        if (TryGet(path, out asset))
        {
            return asset;
        }

        asset = await factory(path, cancellationToken).ConfigureAwait(false);
        if (asset is null)
        {
            throw new InvalidOperationException("Factory returned null for asset path: " + path);
        }

        _assets[path] = new WeakReference(asset);
        _assetPaths.Add(asset, Box.CreateReadOnly(path));
        return asset;
    }

    public async ValueTask<object> GetOrAddAsync<TContext>(
        AssetPath path,
        TContext context,
        Func<AssetPath, TContext, CancellationToken, ValueTask<object>> factory,
        CancellationToken cancellationToken = default
    )
    {
        if (TryGet(path, out var asset))
        {
            return asset;
        }

        var semaphore = _semaphores.GetOrAdd(path, _ => new SemaphoreSlim(1));

        using var scope = await semaphore.EnterScopeAsync(cancellationToken).ConfigureAwait(false);
        if (TryGet(path, out asset))
        {
            return asset;
        }

        asset = await factory(path, context, cancellationToken).ConfigureAwait(false);
        if (asset is null)
        {
            throw new InvalidOperationException("Factory returned null for asset path: " + path);
        }

        _assets[path] = new WeakReference(asset);
        _assetPaths.Add(asset, Box.CreateReadOnly(path));
        return asset;
    }

    public void Set(AssetPath path, object asset)
    {
        _assets[path] = new WeakReference(asset);
        _assetPaths.Add(asset, Box.CreateReadOnly(path));
    }

    public void Rename(AssetPath oldPath, AssetPath newPath)
    {
        if (_assets.ChangeKey(oldPath, newPath, out var value))
        {
            _assetPaths.AddOrUpdate(value, Box.CreateReadOnly(newPath));
        }
    }

    public void Remove(AssetPath path)
    {
        if (!_assets.Remove(path, out var oldReference))
            return;
        var target = oldReference.Target;
        if (target is not null)
        {
            _assetPaths.Remove(target);
        }
    }

    public void Clear()
    {
        _assets.Clear();
    }

    public void RemoveInvalidAssets()
    {
        _assets.RemoveIf((_, w) => !w.IsAlive);
    }

    public void Dispose()
    {
        _assets.ForEach(
            (_, asset) =>
            {
                if (asset.Target is IDisposable disposable)
                    disposable.Dispose();
            }
        );
    }
}
