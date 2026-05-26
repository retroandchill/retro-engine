// @file Asset.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace RetroEngine.Assets;

public static class Asset
{
    internal static IAssetManager? Manager { get; set; }

    [MemberNotNull(nameof(Manager))]
    private static void ThrowIfManagerNotSet()
    {
        if (Manager is null)
            throw new InvalidOperationException("Asset manager not set.");
    }

    public static object Load(AssetPath path)
    {
        ThrowIfManagerNotSet();
        return Manager.LoadAsset(path);
    }

    public static T Load<T>(AssetPath path)
        where T : class
    {
        ThrowIfManagerNotSet();
        return Manager.LoadAsset<T>(path);
    }

    public static ValueTask<object> LoadAsync(AssetPath path, CancellationToken cancellationToken = default)
    {
        ThrowIfManagerNotSet();
        return Manager.LoadAssetAsync(path, cancellationToken);
    }

    public static ValueTask<T> LoadAsync<T>(AssetPath path, CancellationToken cancellationToken = default)
        where T : class
    {
        ThrowIfManagerNotSet();
        return Manager.LoadAssetAsync<T>(path, cancellationToken);
    }

    public static AssetPath GetPath(object asset)
    {
        ThrowIfManagerNotSet();
        return Manager.GetAssetPath(asset);
    }

    public static bool TryGetPath(object asset, out AssetPath path)
    {
        ThrowIfManagerNotSet();
        return Manager.TryGetAssetPath(asset, out path);
    }
}
