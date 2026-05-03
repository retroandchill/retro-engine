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
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using IconPacks.Avalonia.Codicons;
using Microsoft.Extensions.Logging;
using RetroEngine.Assets;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.Services;
using RetroEngine.Editor.Core.ViewModels.Dialogs;
using RetroEngine.Editor.Core.Views.Tabs;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Localization.Formatting;
using RetroEngine.Portable.Strings;
using RetroEngine.Utilities;
using RetroEngine.Utils;

namespace RetroEngine.Editor.Core.ViewModels.Tabs;

public sealed partial class ContentBrowserItem : ObservableObject
{
    private static readonly Text NewFolderName = Text.AsLocalizable(
        "ContentBrowserViewModel",
        "NewFolderName",
        "Create Folder"
    );

    private static readonly Text RenameAssetText = Text.AsLocalizable("ContentBrowserViewModel", "Rename", "Rename");

    private static readonly TextFormat NameIsAlreadyTaken = Text.AsLocalizable(
        "ContentBrowserViewModel",
        "NameIsAlreadyTaken",
        "A folder/file with the name '{0}' already exists"
    );
    private static readonly TextFormat ErrorFormat = Text.AsLocalizable(
        "ContentBrowserViewModel",
        "ErrorFormat",
        "The entered text '{0}' is not valid"
    );

    private static readonly TextFormat ExtensionChangeFormat = Text.AsLocalizable(
        "ContentBrowserViewModel",
        "ExtensionChangeFormat",
        "The following rename will change the file extension of asset {0}. It may become unstable. Proceed anyways?"
    );

    private readonly IDialogService _dialogService;
    private readonly IAssetPackage _package;
    private readonly INavigationService _navigationService;
    private readonly ILogger? _logger;
    internal AssetPackageEntryKey Key { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; } = "";

    [ObservableProperty]
    public partial PackIconCodiconsKind Icon { get; set; } = PackIconCodiconsKind.Folder;

    [ObservableProperty]
    public partial bool IsExpanded { get; set; }

    [ObservableProperty]
    public partial bool IsRenamable { get; set; } = true;

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

    public ContentBrowserItem(
        IDialogService dialogService,
        IAssetPackage package,
        INavigationService navigationService,
        ILogger? logger
    )
    {
        _dialogService = dialogService;
        _package = package;
        _navigationService = navigationService;
        _logger = logger;
        ChildrenSource.Connect().Sort(KeyComparer.Instance).Bind(out _sortedChildren).Subscribe();
    }

    [RelayCommand]
    private void NewFolder()
    {
        if (_package is IEditableAssetPackage editablePackage)
        {
            _ = NewFolderAsync(editablePackage);
        }
    }

    private async Task NewFolderAsync(IEditableAssetPackage editablePackage)
    {
        try
        {
            var viewModel = _dialogService.CreateViewModel<TextEntryWindowViewModel>();
            viewModel.WindowTitle = NewFolderName;
            viewModel.ValidationFunc = (str, out error) =>
            {
                var newName = Key.Name.IsNone ? str : $"{Key.Name}/{str}";
                var asName = new Name(newName);
                if (editablePackage.GetEntry(asName) is null)
                    return ValidateValidFileName(str, out error);

                error = Text.Format(NameIsAlreadyTaken, str);
                return false;
            };
            var result = await _dialogService.ShowDialogAsync(_navigationService.MainWindow, viewModel);
            if (result is not true)
            {
                return;
            }

            var newName = Key.Name.IsNone ? viewModel.TextEntry : $"{Key.Name}/{viewModel.TextEntry}";
            var nameKey = new Name(newName);
            await editablePackage.AddFolderAsync(nameKey);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to create new folder.");

            await _dialogService.ShowMessageBoxAsync(
                _navigationService.MainWindow,
                new MessageBoxSettings { Icon = MessageBoxImage.Error, Content = ex.Message }
            );
        }
    }

    private static bool ValidateValidFileName(string str, out Text error)
    {
        if (Path.IsValidPortableFileName(str))
        {
            error = Text.Empty;
            return true;
        }

        error = Text.Format(ErrorFormat, str);
        return false;
    }

    [RelayCommand]
    private void Rename()
    {
        if (_package is IEditableAssetPackage editablePackage)
        {
            _ = RenameAsync(editablePackage);
        }
    }

    private async Task RenameAsync(IEditableAssetPackage editablePackage)
    {
        try
        {
            var viewModel = _dialogService.CreateViewModel<TextEntryWindowViewModel>();
            viewModel.WindowTitle = RenameAssetText;
            var originalName = Key.Name;
            var nameAsString = (string)originalName;
            var splitIndex = nameAsString.LastIndexOf('/');
            var parentName = splitIndex > 0 ? nameAsString.AsMemory(0, splitIndex) : default;
            viewModel.ValidationFunc = (str, out error) =>
            {
                var compositeName = !parentName.IsEmpty ? $"{parentName.Span}/{str}" : str;
                var asName = new Name(compositeName);
                if (asName == originalName || editablePackage.GetEntry(asName) is null)
                    return ValidateValidFileName(str, out error);

                error = Text.Format(NameIsAlreadyTaken, str);
                return false;
            };
            var childName = splitIndex > 0 ? nameAsString[(splitIndex + 1)..] : nameAsString;

            viewModel.TextEntry = childName;
            viewModel.SelectionStart = 0;
            viewModel.SelectionEnd = childName.Length;
            var oldExtension = ReadOnlyMemory<char>.Empty;
            if (_package.GetEntry(originalName) is IAssetPackageFile file)
            {
                var extensionStart = childName.LastIndexOf('.');
                if (extensionStart != -1)
                {
                    viewModel.SelectionEnd = extensionStart;
                    oldExtension = childName.AsMemory(extensionStart + 1);
                }
            }

            var result = await _dialogService.ShowDialogAsync(_navigationService.MainWindow, viewModel);
            if (result is not true)
            {
                return;
            }

            var flagExtensionChange = false;
            if (!oldExtension.IsEmpty)
            {
                var extensionStart = viewModel.TextEntry.LastIndexOf('.');
                if (extensionStart == -1)
                {
                    flagExtensionChange = true;
                }
                else
                {
                    var newExtension = viewModel.TextEntry.AsSpan(extensionStart + 1);
                    flagExtensionChange = !newExtension.Equals(oldExtension.Span, StringComparison.OrdinalIgnoreCase);
                }
            }

            if (flagExtensionChange)
            {
                var confirmPrompt = await _dialogService.ShowMessageBoxAsync(
                    _navigationService.MainWindow,
                    new MessageBoxSettings
                    {
                        Button = MessageBoxButton.YesNo,
                        Icon = MessageBoxImage.Warning,
                        Content = Text.Format(ExtensionChangeFormat, childName),
                    }
                );
                if (confirmPrompt is not true)
                    return;
            }

            var compositeName = !parentName.IsEmpty ? $"{parentName.Span}/{viewModel.TextEntry}" : viewModel.TextEntry;
            var newName = new Name(compositeName);
            await editablePackage.RenameAssetAsync(originalName, newName);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to create new folder.");

            await _dialogService.ShowMessageBoxAsync(
                _navigationService.MainWindow,
                new MessageBoxSettings { Icon = MessageBoxImage.Error, Content = ex.Message }
            );
        }
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
    private readonly IDialogService _dialogService;
    private readonly IAssetPackage _package;
    private readonly INavigationService _navigationService;
    private readonly ILogger? _logger;

    private readonly Dictionary<Name, ContentBrowserItem> _items = new();

    public ContentBrowserItem Item { get; }

    public ContentBrowserPackageRoot(
        IDialogService dialogService,
        IAssetPackage package,
        INavigationService navigationService,
        ILogger? logger
    )
    {
        _dialogService = dialogService;
        _package = package;
        _navigationService = navigationService;
        _logger = logger;

        _package.OnEntriesRefreshed += OnPackageChanged;

        Item = new ContentBrowserItem(dialogService, package, navigationService, logger)
        {
            Name = package.PackageName,
            Icon = PackIconCodiconsKind.Package,
            IsRenamable = false,
            CanEdit = package is IEditableAssetPackage,
            IsDirectory = true,
        };
        Item.ChildrenSource.AddRange(package.TopLevelEntries.Select(CreateContentFolder));
        _items[Name.None] = Item;
    }

    private ContentBrowserItem CreateContentFolder(IAssetPackageEntry entry)
    {
        var children = entry is IAssetPackageFolder folder ? folder.Children.Select(CreateContentFolder) : [];
        var item = new ContentBrowserItem(_dialogService, _package, _navigationService, _logger)
        {
            Name = entry.DisplayName,
            Key = entry.Key,
            Icon = entry.IsDirectory ? PackIconCodiconsKind.Folder : PackIconCodiconsKind.File,
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
