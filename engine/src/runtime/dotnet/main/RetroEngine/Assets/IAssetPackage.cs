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

public ref struct AssetPackageChangeManifest
{
    public required ReadOnlySpan<IAssetPackageEntry> AddedEntries { get; init; }
    public required ReadOnlySpan<IAssetPackageEntry> RemovedEntries { get; init; }
    public required ReadOnlySpan<(IAssetPackageEntry Old, IAssetPackageEntry New)> RenamedEntries { get; init; }
    public required ReadOnlySpan<(IAssetPackageEntry Old, IAssetPackageEntry New)> ModifiedEntries { get; init; }
}

public delegate void AssetPackageChangeEvent(scoped in AssetPackageChangeManifest manifest);

public interface IAssetPackage
{
    Name PackageName { get; }

    AssetPackageLoadState LoadState { get; }

    string SourcePath { get; }

    bool IsReadOnly { get; }

    IReadOnlyCollection<IAssetPackageEntry> TopLevelEntries { get; }

    event AssetPackageChangeEvent? OnEntriesRefreshed;

    event Action<Exception>? OnRefreshError;

    void Load();

    ValueTask LoadAsync(CancellationToken cancellationToken = default);

    void Unload();

    IAssetPackageEntry? GetEntry(Name entryName);

    bool HasAsset(Name assetName);

    Name GetAssetType(Name assetName);

    Stream OpenAsset(Name assetName);

    Task RefreshAllAsync(CancellationToken cancellationToken = default);

    Task RefreshAsync(Name entryName, CancellationToken cancellationToken = default);
}

public interface IAssetPackageFactory
{
    bool CanCreate(Name packageName, string path);

    IAssetPackage Create(Name packageName, string path);
}

public interface IEditableAssetPackage : IAssetPackage
{
    Task AddFolderAsync(Name name, CancellationToken cancellationToken = default);

    Task RenameAssetAsync(Name oldName, Name newName, CancellationToken cancellationToken = default);

    Task DeleteAssetAsync(Name name, CancellationToken cancellationToken = default);
}

public static class AssetPackageExtensions
{
    extension(IAssetPackage package)
    {
        public IEnumerable<IAssetPackageEntry> WalkEntriesDepthFirst()
        {
            return package.TopLevelEntries.SelectMany(GetSelfAndChildrenDepthFirst);
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
                    exploreSet.Enqueue(subEntry);
                }
            }
        }
    }

    extension(IAssetPackageEntry entry)
    {
        public IEnumerable<IAssetPackageEntry> GetSelfAndChildrenDepthFirst()
        {
            yield return entry;

            if (entry is not IAssetPackageFolder folder)
                yield break;
            foreach (var subEntry in folder.Children.SelectMany(GetSelfAndChildrenDepthFirst))
            {
                yield return subEntry;
            }
        }

        public IEnumerable<IAssetPackageEntry> GetSelfAndChildrenBreadthFirst()
        {
            var exploreSet = new Queue<IAssetPackageEntry>();
            exploreSet.Enqueue(entry);
            while (exploreSet.TryDequeue(out var e))
            {
                yield return e;
                if (e is not IAssetPackageFolder folder)
                    continue;

                foreach (var subEntry in folder.Children)
                {
                    exploreSet.Enqueue(subEntry);
                }
            }
        }
    }
}
