// // @file IAssetPackage.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Strings;

namespace RetroEngine.Assets;

public enum AssetPackageLoadState
{
    Unloaded,
    Loading,
    Loaded,
}

public interface IAssetPackage
{
    Name PackageName { get; }

    AssetPackageLoadState LoadState { get; }

    public string SourcePath { get; }

    public bool IsReadOnly { get; }

    public IReadOnlyCollection<IAssetPackageEntry> TopLevelEntries { get; }

    public event Action<IAssetPackageEntry>? OnEntryAdded;

    public event Action<IAssetPackageEntry>? OnEntryRemoved;

    public event Action<IAssetPackageEntry, IAssetPackageEntry>? OnEntryRenamed;

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

public static class AssetPackageExtensions
{
    extension(IAssetPackage package)
    {
        public IEnumerable<IAssetPackageEntry> WalkEntriesDepthFirst()
        {
            return package.TopLevelEntries.SelectMany(GetSelfAndChildren);
        }

        public IEnumerable<IAssetPackageEntry> WalkEntriesBreadthFirst()
        {
            var exploreSet = new Queue<IAssetPackageEntry>(package.TopLevelEntries);
            while (exploreSet.TryDequeue(out var entry))
            {
                yield return entry;
                if (entry is not IAssetPackageFolder folder)
                    continue;

                foreach (var subEntry in folder.Children)
                {
                    yield return subEntry;
                }
            }
        }
    }

    private static IEnumerable<IAssetPackageEntry> GetSelfAndChildren(this IAssetPackageEntry entry)
    {
        yield return entry;

        if (entry is not IAssetPackageFolder folder)
            yield break;
        foreach (var subEntry in folder.Children.SelectMany(GetSelfAndChildren))
        {
            yield return subEntry;
        }
    }
}
