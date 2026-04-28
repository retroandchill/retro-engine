// // @file ContentBrowserViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.IO.Abstractions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.RetroEngine.Controls;
using IconPacks.Avalonia.Codicons;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.Views.Tabs;
using RetroEngine.Portable.Localization;
using RetroEngine.Utils;

namespace RetroEngine.Editor.Core.ViewModels.Tabs;

public sealed partial class ContentBrowserItem : ObservableObject
{
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

    public required ObservableCollection<ContentBrowserItem> Children { get; init; }

    [RelayCommand]
    private void NewFolder() { }

    [RelayCommand]
    private void Rename()
    {
        IsRenaming = true;
    }
}

[ViewModelFor<ContentBrowserView>]
public sealed partial class ContentBrowserViewModel : Tool
{
    private const string TextNamespace = "RetroEngine.Editor.Core.ViewModels.Tabs.ContentBrowserViewModel";

    public IFileSystem FileSystem { get; init; } = IFileSystem.Default;

    [ObservableProperty]
    public partial ContentBrowserItem? SelectedFolder { get; internal set; }

    public required ObservableCollection<ContentBrowserItem> Items { get; init; }

    public ContentBrowserViewModel()
    {
        Title = Text.AsLocalizable(TextNamespace, "ContentBrowser", "Content Browser");
    }
}
