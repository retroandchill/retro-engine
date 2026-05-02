// // @file ContentBrowserViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.IO.Abstractions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.RetroEngine.Controls;
using DynamicData;
using IconPacks.Avalonia.Codicons;
using RetroEngine.Assets;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.Views.Tabs;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Strings;
using RetroEngine.Utils;

namespace RetroEngine.Editor.Core.ViewModels.Tabs;

public sealed partial class ContentBrowserItem : ObservableObject
{
    private readonly IAssetPackage _package;
    internal AssetPackageEntryKey Key { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; } = "";

    [ObservableProperty]
    public partial PackIconCodiconsKind Icon { get; set; } = PackIconCodiconsKind.Folder;

    [ObservableProperty]
    public partial bool IsRenaming { get; set; }

    [ObservableProperty]
    public partial bool IsExpanded { get; set; }

    [ObservableProperty]
    public partial bool CanEdit { get; set; } = true;

    [ObservableProperty]
    public partial bool IsDirectory { get; set; }

    internal SourceList<ContentBrowserItem> ChildrenSource { get; } = new();
    private readonly ReadOnlyObservableCollection<ContentBrowserItem> _sortedChildren;

    public ReadOnlyObservableCollection<ContentBrowserItem> Children
    {
        get => _sortedChildren;
        init => ChildrenSource.AddRange(value);
    }

    public ContentBrowserItem(IAssetPackage package)
    {
        _package = package;
        ChildrenSource.Connect().Sort(KeyComparer.Instance).Bind(out _sortedChildren).Subscribe();
    }

    [RelayCommand]
    private void NewFolder() { }

    [RelayCommand]
    private void Rename()
    {
        IsRenaming = true;
    }

    [RelayCommand]
    private void Refresh()
    {
        _ = _package.RefreshAsync(Key.Name);
    }

    internal sealed class KeyComparer : IComparer<ContentBrowserItem>
    {
        public static KeyComparer Instance { get; } = new();

        public int Compare(ContentBrowserItem? x, ContentBrowserItem? y)
        {
            if (ReferenceEquals(x, y))
                return 0;
            if (x is null)
                return 1;
            if (y is null)
                return -1;

            return x.Key.CompareTo(y.Key);
        }
    }
}

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
            Icon = PackIconCodiconsKind.Package,
            CanEdit = false,
            IsDirectory = true,
        };
        Item.ChildrenSource.AddRange(package.TopLevelEntries.Select(x => CreateContentFolder(package, x)));
        _items[Name.None] = Item;
    }

    private ContentBrowserItem CreateContentFolder(IAssetPackage package, IAssetPackageEntry entry)
    {
        var children = entry is IAssetPackageFolder folder
            ? folder.Children.Select(x => CreateContentFolder(package, x))
            : [];
        var item = new ContentBrowserItem(package)
        {
            Name = entry.DisplayName,
            Key = entry.Key,
            Icon = entry.IsDirectory ? PackIconCodiconsKind.Folder : PackIconCodiconsKind.File,
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
            OnEntryAdded(_package, entry);
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

    private void OnEntryAdded(IAssetPackage package, IAssetPackageEntry entry)
    {
        var parent = _items[entry.ParentName];
        parent.ChildrenSource.Add(CreateContentFolder(package, entry));
    }

    private void OnEntryRemoved(IAssetPackageEntry entry)
    {
        var parent = _items[entry.ParentName];
        parent.ChildrenSource.Remove(_items[entry.Name]);
    }

    private void OnEntryRenamed(IAssetPackageEntry entry, IAssetPackageEntry oldEntry)
    {
        var oldParent = _items[oldEntry.ParentName];
        var newParent = _items[entry.ParentName];

        if (!_items.Remove(oldEntry.Name, out var oldItem))
            return;
        oldItem.Key = entry.Key;
        oldItem.Name = entry.DisplayName;
        _items[entry.Name] = oldItem;

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

[ViewModelFor<ContentBrowserView>]
public sealed partial class ContentBrowserViewModel : Tool, IDisposable
{
    private const string TextNamespace = "RetroEngine.Editor.Core.ViewModels.Tabs.ContentBrowserViewModel";
    private bool _disposed;

    public IFileSystem FileSystem { get; init; } = IFileSystem.Default;

    [ObservableProperty]
    public partial ContentBrowserItem? SelectedFolder { get; internal set; }

    private readonly SourceList<ContentBrowserItem> _items = new();
    private readonly ReadOnlyObservableCollection<ContentBrowserItem> _sortedItems;
    public ReadOnlyObservableCollection<ContentBrowserItem> Items => _sortedItems;

    public ObservableCollection<ContentBrowserPackageRoot> Packages { get; } = [];

    public ContentBrowserViewModel()
    {
        Title = Text.AsLocalizable(TextNamespace, "ContentBrowser", "Content Browser");
        _items.Connect().Sort(ContentBrowserItem.KeyComparer.Instance).Bind(out _sortedItems).Subscribe();
        Packages.CollectionChanged += (_, c) =>
        {
            if (c.OldItems is not null)
            {
                foreach (var item in c.OldItems.OfType<ContentBrowserPackageRoot>())
                {
                    item.Dispose();
                    _items.Remove(item.Item);
                }
            }

            if (c.NewItems is null)
                return;
            foreach (var item in c.NewItems.OfType<ContentBrowserPackageRoot>())
            {
                _items.Add(item.Item);
            }
        };
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        foreach (var item in Packages)
        {
            item.Dispose();
        }
    }
}
