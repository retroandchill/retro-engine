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

    private string _name = "";
    private string _parentPath = "";
    private string _fullPath = "";

    public string Name
    {
        get => _name;
        set
        {
            if (_name == value)
                return;

            var oldFullPath = _fullPath;
            var newFullPath = _fileSystem.Path.Combine(_parentPath, value);
            try
            {
                _fileSystem.Directory.Move(oldFullPath, newFullPath);
            }
            catch
            {
                return;
            }

            SetProperty(ref _name, value);
            SetProperty(ref _fullPath, newFullPath, nameof(Path));
        }
    }

    public required string Path
    {
        get => _fullPath;
        set
        {
            SetProperty(ref _fullPath, value);
            SetProperty(ref _name, _fileSystem.Path.GetFileName(value), nameof(Name));
            _parentPath = _fileSystem.Path.GetDirectoryName(value) ?? "";
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
    public partial bool IsRenaming { get; set; }

    [ObservableProperty]
    public partial bool IsExpanded { get; set; }

    [ObservableProperty]
    public partial bool CanEdit { get; set; } = true;

    public ObservableCollection<ContentBrowserFolder> Children { get; } = [];

    private ContentBrowserFolder CreateContentBrowserFolder(string directory)
    {
        return new ContentBrowserFolder(_fileSystem) { Path = directory };
    }

    private IEnumerable<ContentBrowserFolder> GetContentBrowserFolders()
    {
        return _fileSystem.Directory.GetDirectories(Path).Select(CreateContentBrowserFolder);
    }

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
    public partial ContentBrowserFolder? SelectedFolder { get; internal set; }

    public ObservableCollection<ContentBrowserFolder> Folders { get; } = [];

    public ContentBrowserViewModel()
    {
        Title = Text.AsLocalizable(TextNamespace, "ContentBrowser", "Content Browser");
    }
}
