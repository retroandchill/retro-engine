// // @file ContentBrowserActions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Microsoft.Extensions.Logging;
using RetroEngine.Assets;
using RetroEngine.AssetTools;
using RetroEngine.Editor.Core.ViewModels.Dialogs;
using RetroEngine.Editor.Core.ViewModels.Tabs;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Localization.Formatting;
using RetroEngine.Portable.Strings;
using RetroEngine.Utilities;

namespace RetroEngine.Editor.Core.Services.Actions;

[RegisterSingleton]
public sealed class ContentBrowserActions(
    IDialogService DialogService,
    IAssetTools AssetTools,
    INavigationService NavigationService,
    AssetManager AssetManager,
    ILogger<ContentBrowserActions> Logger
) : IContentBrowserActions
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

    public async Task NewAssetAsync(
        ContentBrowserItem parent,
        Type assetType,
        Text displayName,
        Action<AssetPath, object>? postCreationAction = null
    )
    {
        var viewModel = DialogService.CreateViewModel<TextEntryWindowViewModel>();
        viewModel.WindowTitle = Text.Format(NewAssetFormat, displayName);
        viewModel.ValidationFunc = (str, out error) =>
        {
            var nameWithExtension = AssetTools.GetAssetNameWithExtension(str, assetType);
            var newName = parent.Key.Name.IsNone ? nameWithExtension : $"{parent.Key.Name}/{nameWithExtension}";
            var asName = new Name(newName);
            if (parent.Package.GetEntry(asName) is null)
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
            parent.Key.Name.ToString(),
            parent.Package.PackageName,
            assetType
        );
        var path = AssetManager.GetAssetPath(asset);

        try
        {
            postCreationAction?.Invoke(path, asset);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to open asset");

            await DialogService.ShowMessageBoxAsync(
                NavigationService.MainWindow,
                new MessageBoxSettings { Icon = MessageBoxImage.Error, Content = ex.Message }
            );
        }
    }

    public async Task NewFolderAsync(ContentBrowserItem parent, IEditableAssetPackage editablePackage)
    {
        try
        {
            var viewModel = DialogService.CreateViewModel<TextEntryWindowViewModel>();
            viewModel.WindowTitle = NewFolderName;
            viewModel.ValidationFunc = (str, out error) =>
            {
                var newName = parent.Key.Name.IsNone ? str : $"{parent.Key.Name}/{str}";
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

            var newName = parent.Key.Name.IsNone ? viewModel.TextEntry : $"{parent.Key.Name}/{viewModel.TextEntry}";
            var nameKey = new Name(newName);
            await editablePackage.AddFolderAsync(nameKey);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to create new folder.");

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
}
