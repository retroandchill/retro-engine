// // @file ContentBrowserViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using AutoViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.RetroEngine.Controls;
using DynamicData;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using RetroEngine.Assets;
using RetroEngine.AssetTools;
using RetroEngine.Editor.Core.Services;
using RetroEngine.Editor.Core.Services.Actions;
using RetroEngine.Editor.Core.ViewModels.Dialogs;
using RetroEngine.Editor.Core.Views.Tabs;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Localization.Formatting;
using RetroEngine.Portable.Strings;
using RetroEngine.ToolMenu.ViewModel;
using RetroEngine.Utilities;

namespace RetroEngine.Editor.Core.ViewModels.Tabs;

public sealed record NewAssetArgs(ContentBrowserItem Parent, Type AssetType, Text DisplayName);

[ViewModelFor<ContentBrowserView>]
public sealed partial class ContentBrowserViewModel : Tool
{
    private readonly IContentBrowserContextMenuBuilder _contextMenuBuilder;
    private readonly IContentBrowserActions _contentBrowserActions;

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

    [ObservableProperty]
    public partial ContentBrowserItem? SelectedItem { get; internal set; }

    private readonly SourceList<ContentBrowserItem> _items = new();
    private readonly ReadOnlyObservableCollection<ContentBrowserItem> _sortedItems;
    public ReadOnlyObservableCollection<ContentBrowserItem> Items => _sortedItems;

    public ObservableList<ContentBrowserPackageRoot> Packages { get; } = [];

    private readonly ObservableList<IMenuItemEntry> _contextActions = [];
    public NotifyCollectionChangedSynchronizedViewList<IMenuItemEntry> ContextActions { get; }

    public event Action<AssetPath, object>? AssetOpenRequested;

    public IDialogService DialogService { get; init; }
    public INavigationService NavigationService { get; init; }

    public AssetManager AssetManager { get; init; }
    public IAssetTools AssetTools { get; init; }

    public ILogger? Logger { get; init; }

    public ContentBrowserViewModel(
        IContentBrowserContextMenuBuilder contextMenuBuilder,
        IContentBrowserActions contentBrowserActions
    )
    {
        _contextMenuBuilder = contextMenuBuilder;
        _contentBrowserActions = contentBrowserActions;
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
        _contextActions.Clear();
        _contextActions.AddRange(
            _contextMenuBuilder.Build(
                value,
                new ContentBrowserCommands(
                    NewFolderCommand,
                    NewAssetCommand,
                    RefreshCommand,
                    RenameCommand,
                    DeleteCommand
                )
            )
        );
    }

    [RelayCommand]
    private void NewAsset(NewAssetArgs args)
    {
        _ = _contentBrowserActions.NewAssetAsync(args.Parent, args.AssetType, args.DisplayName, AssetOpenRequested);
    }

    [RelayCommand]
    private void NewFolder(ContentBrowserItem value)
    {
        if (value.Package is IEditableAssetPackage editablePackage)
        {
            _ = _contentBrowserActions.NewFolderAsync(value, editablePackage);
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
            Logger?.LogError(ex, "Failed to open asset");

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
            Logger?.LogError(ex, "Failed to create new folder.");

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
            Logger?.LogError(ex, "Failed to create new folder.");

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
