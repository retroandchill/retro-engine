// // @file ContentBrowserPackageRoot.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using DynamicData;
using FluentIcons.Common;
using RetroEngine.Assets;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Editor.Core.ViewModels.Tabs;

public sealed class ContentBrowserPackageRoot : IDisposable
{
    private readonly IAssetPackage _package;

    private readonly Dictionary<Name, ContentBrowserItem> _items = new();

    public ContentBrowserItem Item { get; }

    public ContentBrowserPackageRoot(IAssetPackage package)
    {
        _package = package;

        _package.OnEntriesRefreshed += OnPackageChanged;

        Item = new ContentBrowserItem(package)
        {
            Name = package.PackageName,
            Icon = Icon.Box,
            CanRenameOrDelete = false,
            CanEdit = package is IEditableAssetPackage,
            IsDirectory = true,
        };
        Item.ChildrenSource.AddRange(package.TopLevelEntries.Select(CreateContentFolder));
        _items[Name.None] = Item;
    }

    private ContentBrowserItem CreateContentFolder(IAssetPackageEntry entry)
    {
        var children = entry is IAssetPackageFolder folder ? folder.Children.Select(CreateContentFolder) : [];
        var item = new ContentBrowserItem(_package)
        {
            Name = entry.DisplayName,
            Key = entry.Key,
            Icon = entry.IsDirectory ? Icon.Folder : Icon.Document,
            CanEdit = _package is IEditableAssetPackage,
            IsDirectory = entry.IsDirectory,
        };
        item.ChildrenSource.AddRange(children);

        _items[entry.Name] = item;
        return item;
    }

    private void OnPackageChanged(scoped in AssetPackageChangeManifest manifest)
    {
        foreach (var entry in manifest.AddedEntries)
        {
            OnEntryAdded(entry);
        }

        foreach (var entry in manifest.RemovedEntries)
        {
            OnEntryRemoved(entry);
        }

        foreach (var (oldEntry, newEntry) in manifest.RenamedEntries)
        {
            OnEntryRenamed(oldEntry, newEntry);
        }
    }

    private void OnEntryAdded(IAssetPackageEntry entry)
    {
        var parent = _items[entry.ParentName];
        parent.ChildrenSource.Add(CreateContentFolder(entry));
    }

    private void OnEntryRemoved(IAssetPackageEntry entry)
    {
        var parent = _items[entry.ParentName];
        parent.ChildrenSource.Remove(_items[entry.Name]);
    }

    private void OnEntryRenamed(IAssetPackageEntry oldEntry, IAssetPackageEntry newEntry)
    {
        var oldParent = _items[oldEntry.ParentName];
        var newParent = _items[newEntry.ParentName];

        if (!_items.Remove(oldEntry.Name, out var oldItem))
            return;
        oldItem.Key = newEntry.Key;
        oldItem.Name = newEntry.DisplayName;
        _items[newEntry.Name] = oldItem;

        oldItem.Key = newEntry.Key;
        oldItem.Name = newEntry.DisplayName;

        if (ReferenceEquals(oldParent, newParent))
            return;

        oldParent.ChildrenSource.Remove(_items[oldEntry.Name]);
        newParent.ChildrenSource.Add(_items[oldEntry.Name]);
    }

    public void Dispose()
    {
        _package.OnEntriesRefreshed -= OnPackageChanged;
    }
}
