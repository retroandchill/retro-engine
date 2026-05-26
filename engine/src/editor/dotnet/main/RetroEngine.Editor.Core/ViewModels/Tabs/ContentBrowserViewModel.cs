// @file ContentBrowserViewModel.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using AutoViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.RetroEngine.Controls;
using DynamicData;
using FluentIcons.Common;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using RetroEngine.Assets;
using RetroEngine.AssetTools;
using RetroEngine.Editor.Core.Services;
using RetroEngine.Editor.Core.ViewModels.Dialogs;
using RetroEngine.Editor.Core.Views.Tabs;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Localization.Formatting;
using RetroEngine.Portable.Strings;
using RetroEngine.ToolMenu.ViewModel;
using RetroEngine.Utilities;
using Serilog;

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

[ViewModelFor<ContentBrowserView>]
public sealed partial class ContentBrowserViewModel : Tool
{
    private const string TextNamespace = "RetroEngine.Editor.Core.ViewModels.Tabs.ContentBrowserViewModel";

    private static readonly TextFormat NewAssetFormat = Text.AsLocalizable(
        "ContentBrowserViewModel",
        "NewFolderName",
        "Create {0}"
    );

    private static readonly Text NewFolderName = Text.AsLocalizable(
        "ContentBrowserViewModel",
        "NewFolderName",
        "Create Folder"
    );

    private static readonly Text RenameAssetText = Text.AsLocalizable("ContentBrowserViewModel", "Rename", "Rename");

    private static readonly Text DeleteAssetWarning = Text.AsLocalizable(
        "ContentBrowserViewModel",
        "DeleteAssetWarning",
        "You are about to delete one or more assets, proceed?"
    );

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

    private static readonly Text CreateLabel = Text.AsLocalizable("ContentBrowserViewModel", "Create", "Create");
    private static readonly Text NewFolderLabel = Text.AsLocalizable(
        "ContentBrowserViewModel",
        "NewFolder",
        "New Folder"
    );
    private static readonly Text CommonLabel = Text.AsLocalizable("ContentBrowserViewModel", "Common", "Common");
    private static readonly Text RefreshLabel = Text.AsLocalizable("ContentBrowserViewModel", "Refresh", "Refresh");
    private static readonly Text RenameLabel = Text.AsLocalizable("ContentBrowserViewModel", "Rename", "Rename");
    private static readonly Text DeleteLabel = Text.AsLocalizable("ContentBrowserViewModel", "DeleteLabel", "Delete");

    [ObservableProperty]
    public partial ContentBrowserItem? SelectedItem { get; internal set; }

    private readonly SourceList<ContentBrowserItem> _items = new();
    private readonly ReadOnlyObservableCollection<ContentBrowserItem> _sortedItems;
    public ReadOnlyObservableCollection<ContentBrowserItem> Items => _sortedItems;

    public ObservableList<ContentBrowserPackageRoot> Packages { get; } = [];

    private readonly ObservableList<IMenuItemEntry> _contextActions = [];
    public NotifyCollectionChangedSynchronizedViewList<IMenuItemEntry> ContextActions { get; }

    public event Action<AssetPath, object>? AssetOpenRequested;

    public required IDialogService DialogService { get; init; }
    public required INavigationService NavigationService { get; init; }

    public required IAssetManager AssetManager { get; init; }
    public required IAssetTools AssetTools { get; init; }

    public ContentBrowserViewModel()
    {
        Title = Text.AsLocalizable(TextNamespace, "ContentBrowser", "Content Browser");
        _items.Connect().Sort(ContentBrowserItem.KeyComparer.Instance).Bind(out _sortedItems).Subscribe();
        Packages.CollectionChanged += (in c) =>
        {
            foreach (var item in c.OldItems)
            {
                item.Dispose();
                _items.Remove(item.Item);
            }

            foreach (var item in c.NewItems)
            {
                _items.Add(item.Item);
            }
        };
        ContextActions = _contextActions.ToNotifyCollectionChanged();
    }

    public override bool OnClose()
    {
        var result = base.OnClose();

        foreach (var item in Packages)
        {
            item.Dispose();
        }

        return result;
    }

    partial void OnSelectedItemChanged(ContentBrowserItem? value)
    {
        if (value is null)
        {
            _contextActions.Clear();
            return;
        }

        var newContextActions = new List<IMenuItemEntry>();
        if (value.IsDirectory)
        {
            newContextActions.AddRange(
                new MenuSectionHeader("Create", CreateLabel),
                new ParameterizedMenuCommand("NewFolder", NewFolderLabel, NewFolderCommand, value)
                {
                    IsEnabled = value.CanEdit,
                }
            );

            var sectionsToAdd = AssetTools
                .AdvancedAssetCategories.OrderBy(x => x.CategoryName)
                .Select(x =>
                {
                    var factories = AssetTools.Factories.Where(f => f.Categories.HasFlag(x.Category)).ToArray();

                    return (Category: x, Factories: factories);
                })
                .Where(x => x.Factories.Length > 0)
                .Select(x =>
                {
                    var subMenu = new SubMenu(x.Category.CategoryKey, x.Category.CategoryName);

                    subMenu.AddRange(
                        x.Factories.Select(f => new ParameterizedMenuCommand(
                            f.AssetType.Name,
                            f.DisplayName,
                            NewAssetCommand,
                            new NewAssetArgs(value, f.AssetType, f.DisplayName)
                        ))
                    );
                    return subMenu;
                })
                .ToArray();
            if (sectionsToAdd.Length > 0)
            {
                newContextActions.Add(IMenuSeparator.Instance);
                newContextActions.AddRange(sectionsToAdd);
            }
        }

        newContextActions.AddRange(
            new MenuSectionHeader("Common", CommonLabel),
            new ParameterizedMenuCommand("Refresh", RefreshLabel, RefreshCommand, value),
            new ParameterizedMenuCommand("Rename", RenameLabel, RenameCommand, value)
            {
                IsEnabled = value.CanRenameOrDelete,
            },
            new ParameterizedMenuCommand("Delete", DeleteLabel, DeleteCommand, value)
            {
                IsEnabled = value.CanRenameOrDelete,
            }
        );

        _contextActions.Clear();
        _contextActions.AddRange(newContextActions);
    }

    public sealed record NewAssetArgs(ContentBrowserItem Parent, Type AssetType, Text DisplayName);

    [RelayCommand]
    private void NewAsset(NewAssetArgs args)
    {
        _ = NewAssetAsync(args);
    }

    private async Task NewAssetAsync(NewAssetArgs args)
    {
        var viewModel = DialogService.CreateViewModel<TextEntryWindowViewModel>();
        viewModel.WindowTitle = Text.Format(NewAssetFormat, args.DisplayName);
        viewModel.ValidationFunc = (str, out error) =>
        {
            var nameWithExtension = AssetTools.GetAssetNameWithExtension(str, args.AssetType);
            var newName = args.Parent.Key.Name.IsNone
                ? nameWithExtension
                : $"{args.Parent.Key.Name}/{nameWithExtension}";
            var asName = new Name(newName);
            if (args.Parent.Package.GetEntry(asName) is null)
                return ValidateValidFileName(str, out error);

            error = Text.Format(NameIsAlreadyTaken, nameWithExtension.ToString());
            return false;
        };
        var result = await DialogService.ShowDialogAsync(NavigationService.MainWindow, viewModel);
        if (result is not true)
        {
            return;
        }

        var asset = await AssetTools.CreateAssetAsync(
            viewModel.TextEntry,
            args.Parent.Key.Name.ToString(),
            args.Parent.Package.PackageName,
            args.AssetType
        );
        var path = AssetManager.GetAssetPath(asset);

        try
        {
            AssetOpenRequested?.Invoke(path, asset);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open asset");

            await DialogService.ShowMessageBoxAsync(
                NavigationService.MainWindow,
                new MessageBoxSettings { Icon = MessageBoxImage.Error, Content = ex.Message }
            );
        }
    }

    [RelayCommand]
    private void NewFolder(ContentBrowserItem value)
    {
        if (value.Package is IEditableAssetPackage editablePackage)
        {
            _ = NewFolderAsync(value, editablePackage);
        }
    }

    private async Task NewFolderAsync(ContentBrowserItem item, IEditableAssetPackage editablePackage)
    {
        try
        {
            var viewModel = DialogService.CreateViewModel<TextEntryWindowViewModel>();
            viewModel.WindowTitle = NewFolderName;
            viewModel.ValidationFunc = (str, out error) =>
            {
                var newName = item.Key.Name.IsNone ? str : $"{item.Key.Name}/{str}";
                var asName = new Name(newName);
                if (editablePackage.GetEntry(asName) is null)
                    return ValidateValidFileName(str, out error);

                error = Text.Format(NameIsAlreadyTaken, str);
                return false;
            };
            var result = await DialogService.ShowDialogAsync(NavigationService.MainWindow, viewModel);
            if (result is not true)
            {
                return;
            }

            var newName = item.Key.Name.IsNone ? viewModel.TextEntry : $"{item.Key.Name}/{viewModel.TextEntry}";
            var nameKey = new Name(newName);
            await editablePackage.AddFolderAsync(nameKey);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create new folder.");

            await DialogService.ShowMessageBoxAsync(
                NavigationService.MainWindow,
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

    public bool TryOpen(ContentBrowserItem item)
    {
        if (item.IsDirectory)
            return false;

        _ = OpenAsync(new AssetPath(item.Package.PackageName, item.Key.Name));
        return true;
    }

    private async Task OpenAsync(AssetPath path)
    {
        try
        {
            var asset = await AssetManager.LoadAssetAsync(path);
            AssetOpenRequested?.Invoke(path, asset);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open asset");

            await DialogService.ShowMessageBoxAsync(
                NavigationService.MainWindow,
                new MessageBoxSettings { Icon = MessageBoxImage.Error, Content = ex.Message }
            );
        }
    }

    [RelayCommand]
    private void Open(ContentBrowserItem item)
    {
        TryOpen(item);
    }

    [RelayCommand]
    private void Rename(ContentBrowserItem item)
    {
        if (item.Package is IEditableAssetPackage editablePackage)
        {
            _ = RenameAsync(item, editablePackage);
        }
    }

    private async Task RenameAsync(ContentBrowserItem item, IEditableAssetPackage editablePackage)
    {
        try
        {
            var viewModel = DialogService.CreateViewModel<TextEntryWindowViewModel>();
            viewModel.WindowTitle = RenameAssetText;
            var originalName = item.Key.Name;
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
            if (editablePackage.GetEntry(originalName) is IAssetPackageFile)
            {
                var extensionStart = childName.LastIndexOf('.');
                if (extensionStart != -1)
                {
                    viewModel.SelectionEnd = extensionStart;
                    oldExtension = childName.AsMemory(extensionStart + 1);
                }
            }

            var result = await DialogService.ShowDialogAsync(NavigationService.MainWindow, viewModel);
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
                var confirmPrompt = await DialogService.ShowMessageBoxAsync(
                    NavigationService.MainWindow,
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
            Log.Error(ex, "Failed to create new folder.");

            await DialogService.ShowMessageBoxAsync(
                NavigationService.MainWindow,
                new MessageBoxSettings { Icon = MessageBoxImage.Error, Content = ex.Message }
            );
        }
    }

    [RelayCommand]
    private void Delete(ContentBrowserItem item)
    {
        if (item.Package is IEditableAssetPackage editablePackage)
        {
            _ = DeleteAsync(item, editablePackage);
        }
    }

    private async Task DeleteAsync(ContentBrowserItem item, IEditableAssetPackage editablePackage)
    {
        try
        {
            if (editablePackage.GetEntry(item.Key.Name) is { IsOrContainsAsset: true })
            {
                var selection = await DialogService.ShowMessageBoxAsync(
                    NavigationService.MainWindow,
                    new MessageBoxSettings
                    {
                        Button = MessageBoxButton.YesNo,
                        Icon = MessageBoxImage.Warning,
                        Content = DeleteAssetWarning,
                    }
                );
                if (selection is not true)
                    return;
            }

            await editablePackage.DeleteAssetAsync(item.Key.Name, true);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create new folder.");

            await DialogService.ShowMessageBoxAsync(
                NavigationService.MainWindow,
                new MessageBoxSettings { Icon = MessageBoxImage.Error, Content = ex.Message }
            );
        }
    }

    [RelayCommand]
    private static void Refresh(ContentBrowserItem item)
    {
        _ = item.Package.RefreshAsync(item.Key.Name);
    }
}
