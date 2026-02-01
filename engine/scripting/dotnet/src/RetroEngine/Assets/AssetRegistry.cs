// // @file AssetRegistry.cs
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

public static partial class AssetRegistry
{
    private static readonly ConcurrentDictionary<AssetPath, WeakReference<Asset>> AssetCache = new();
    private static readonly Dictionary<Name, Func<IntPtr, Asset>> AssetFactories = new();

    public static void RegisterAssetFactory(Name assetType, Func<IntPtr, Asset> factory)
    {
        AssetFactories[assetType] = factory;
    }

    public static void UnregisterAssetFactory(Name assetType)
    {
        AssetFactories.Remove(assetType);
    }

    public static void RegisterDefaultAssetFactories()
    {
        Texture.RegisterAssetFactory();
    }

    public static void ClearAssetCache()
    {
        AssetCache.Clear();
    }

    public static void ReleaseUnusedAssets()
    {
        foreach (var (path, asset) in AssetCache)
        {
            if (!asset.TryGetTarget(out _))
            {
                AssetCache.TryRemove(path, out _);
            }
        }
    }

    extension(Asset)
    {
        public static Asset? Load(AssetPath path)
        {
            if (AssetCache.TryGetValue(path, out var asset) && asset.TryGetTarget(out var target))
            {
                return target;
            }

            var nativeAsset = NativeLoad(in path, out var assetType, out var error);
            if (nativeAsset == IntPtr.Zero)
            {
                LogAssetLoadError(in path, error);
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
            return Asset.Load(path) as T;
        }
    }

    private static void LogAssetLoadError(in AssetPath path, AssetLoadError error)
    {
        switch (error)
        {
            case AssetLoadError.BadAssetPath:
                Logger.Error($"Bad asset path when loading asset '{path}'.");
                break;
            case AssetLoadError.InvalidAssetFormat:
                Logger.Error($"Invalid asset format when loading asset '{path}'.");
                break;
            case AssetLoadError.AmbiguousAssetPath:
                Logger.Error($"Ambiguous asset path when loading asset '{path}'.");
                break;
            case AssetLoadError.AssetNotFound:
                Logger.Error($"Asset not found when loading asset '{path}'.");
                break;
            case AssetLoadError.AssetTypeMismatch:
                Logger.Error($"Asset type mismatch when loading asset '{path}'.");
                break;
            default:
                Logger.Error($"Unknown asset load error when loading asset '{path}'.");
                break;
        }
        ;
    }

    [LibraryImport("retro_runtime", EntryPoint = "retro_load_asset")]
    private static partial IntPtr NativeLoad(in AssetPath path, out Name assetType, out AssetLoadError error);

    [LibraryImport("retro_runtime", EntryPoint = "retro_release_asset")]
    internal static partial void NativeRelease(IntPtr asset);
}
