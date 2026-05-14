// // @file ContentBrowserItem.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using FluentIcons.Common;
using RetroEngine.Assets;

namespace RetroEngine.Editor.Core.ViewModels.Tabs;

public sealed partial class ContentBrowserItem : ObservableObject
{
    public IAssetPackage Package { get; }
    internal AssetPackageEntryKey Key { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; } = "";

    [ObservableProperty]
    public partial Icon Icon { get; set; } = Icon.Folder;

    [ObservableProperty]
    public partial bool IsExpanded { get; set; }

    [ObservableProperty]
    public partial bool CanRenameOrDelete { get; set; } = true;

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
        Package = package;
        ChildrenSource.Connect().Sort(KeyComparer.Instance).Bind(out _sortedChildren).Subscribe();
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
