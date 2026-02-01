// // @file Asset.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using RetroEngine.Logging;
using RetroEngine.Strings;

namespace RetroEngine.Assets;

public enum AssetLoadError : byte
{
    BadAssetPath,
    InvalidAssetFormat,
    AmbiguousAssetPath,
    AssetNotFound,
    AssetTypeMismatch,
};

public abstract partial class Asset
{
    private static readonly ConcurrentDictionary<AssetPath, WeakReference<Asset>> AssetCache = new();
    private static readonly Dictionary<Name, Func<IntPtr, Asset>> AssetFactories = new();

    public static void RegisterAssetFactory(Name assetType, Func<IntPtr, Asset> factory)
    {
        AssetFactories[assetType] = factory;
    }

    public static Asset? Load(AssetPath path)
    {
        if (AssetCache.TryGetValue(path, out var asset) && asset.TryGetTarget(out var target))
        {
            return target;
        }

        var nativeAsset = NativeLoad(in path, out var assetType, out var error);
        if (nativeAsset == IntPtr.Zero)
        {
            return null;
        }

        if (!AssetFactories.TryGetValue(assetType, out var factory))
        {
            throw new InvalidOperationException($"No factory registered for asset type '{assetType}'.");
        }

        Asset assetInstance;
        try
        {
            assetInstance = factory(nativeAsset);
        }
        catch (Exception)
        {
            NativeRelease(nativeAsset);
            throw;
        }

        AssetCache.TryAdd(path, new WeakReference<Asset>(assetInstance));
        return assetInstance;
    }

    public static T? Load<T>(AssetPath path)
        where T : Asset
    {
        return Load(path) as T;
    }

    [LibraryImport("retro_runtime", EntryPoint = "retro_load_asset")]
    private static partial IntPtr NativeLoad(in AssetPath path, out Name assetType, out AssetLoadError error);

    [LibraryImport("retro_runtime", EntryPoint = "retro_release_asset")]
    private static partial void NativeRelease(IntPtr asset);
}
