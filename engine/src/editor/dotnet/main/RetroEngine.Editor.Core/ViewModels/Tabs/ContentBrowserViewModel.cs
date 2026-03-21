// // @file ContentBrowserViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.IO.Abstractions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.RetroEngine.Controls;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.Views.Tabs;
using RetroEngine.Portable.Localization;
using RetroEngine.Utils;

namespace RetroEngine.Editor.Core.ViewModels.Tabs;

public sealed partial class ContentBrowserFolder(IFileSystem? fileSystem = null) : ObservableObject
{
    private readonly IFileSystem _fileSystem = fileSystem ?? IFileSystem.Default;

    private IFileSystemWatcher? _fileSystemWatcher;

    [ObservableProperty]
    public partial string? Name { get; private set; }

    public required string Path
    {
        get;
        set
        {
            SetProperty(ref field, value);
            Name = _fileSystem.Path.GetFileName(value);

            _fileSystemWatcher?.Dispose();
            _fileSystemWatcher = _fileSystem.FileSystemWatcher.New(value);
            _fileSystemWatcher.IncludeSubdirectories = false;
            _fileSystemWatcher.NotifyFilter = NotifyFilters.DirectoryName;
            _fileSystemWatcher.EnableRaisingEvents = true;

            foreach (var subdirectory in GetContentBrowserFolders())
            {
                Children.Add(subdirectory);
            }

            _fileSystemWatcher.Created += (_, args) =>
            {
                Children.Add(CreateContentBrowserFolder(args.FullPath));
            };
            _fileSystemWatcher.Deleted += (_, args) =>
            {
                Children.Remove(Children.First(x => x.Path == args.FullPath));
            };
            _fileSystemWatcher.Renamed += (_, args) =>
            {
                var folder = Children.FirstOrDefault(x => x.Path == args.OldFullPath);
                if (folder is null)
                    return;
                folder.Path = args.FullPath;
            };
        }
    }

    [ObservableProperty]
    public partial bool IsExpanded { get; set; }

    public ObservableCollection<ContentBrowserFolder> Children { get; } = [];

    private ContentBrowserFolder CreateContentBrowserFolder(string directory)
    {
        return new ContentBrowserFolder(fileSystem) { Path = directory };
    }

    private IEnumerable<ContentBrowserFolder> GetContentBrowserFolders()
    {
        return _fileSystem.Directory.GetDirectories(Path).Select(CreateContentBrowserFolder);
    }
}

[ViewModelFor<ContentBrowserView>]
public sealed partial class ContentBrowserViewModel : Tool
{
    private const string TextNamespace = "RetroEngine.Editor.Core.ViewModels.Tabs.ContentBrowserViewModel";

    public IFileSystem FileSystem { get; init; } = IFileSystem.Default;

    [ObservableProperty]
    public partial bool NavigationPanelOpen { get; set; }

    [ObservableProperty]
    public partial ContentBrowserFolder? SelectedFolder { get; internal set; }

    public ObservableCollection<ContentBrowserFolder> Folders { get; } = [];

    public ContentBrowserViewModel()
    {
        Title = Text.AsLocalizable(TextNamespace, "ContentBrowser", "Content Browser");
    }

    [RelayCommand]
    private void ToggleNavigationPanel()
    {
        NavigationPanelOpen = !NavigationPanelOpen;
    }
}
