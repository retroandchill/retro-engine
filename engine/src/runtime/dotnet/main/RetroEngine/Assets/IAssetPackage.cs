// // @file IAssetPackage.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Assets;

public enum AssetPackageLoadState
{
    Unloaded,
    Loading,
    Loaded,
}

public abstract record AssetPackageEntry(Name Name);

public sealed record AssetPackageFolder(Name Name, ImmutableArray<AssetPackageEntry> Children)
    : AssetPackageEntry(Name);

public sealed record AssetPackageFile(Name Name, Name AssetType, string Extension = "") : AssetPackageEntry(Name)
{
    public string GetFullPath(string rootPath)
    {
        return $"{rootPath}/{Name}{Extension}";
    }
}

public interface IAssetPackage
{
    Name PackageName { get; }

    AssetPackageLoadState LoadState { get; }

    public string SourcePath { get; }

    public bool IsReadOnly { get; }

    public ImmutableArray<AssetPackageEntry> TopLevelEntries { get; }

    public event Action<AssetPackageEntry>? OnEntryAdded;

    public event Action<AssetPackageEntry>? OnEntryRemoved;

    public event Action<AssetPackageEntry, AssetPackageEntry>? OnEntryRenamed;

    public void Load();

    public ValueTask LoadAsync(CancellationToken cancellationToken = default);

    public void Unload();

    bool HasAsset(Name assetName);

    Name GetAssetType(Name assetName);

    Stream OpenAsset(Name assetName);
}

public interface IAssetPackageFactory
{
    bool CanCreate(Name packageName, string path);

    IAssetPackage Create(Name packageName, string path);
}
