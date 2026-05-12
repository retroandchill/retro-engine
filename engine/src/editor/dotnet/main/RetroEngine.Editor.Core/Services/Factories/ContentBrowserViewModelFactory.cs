// // @file ContentBrowserViewModelFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using HanumanInstitute.MvvmDialogs;
using Microsoft.Extensions.Logging;
using RetroEngine.Assets;
using RetroEngine.AssetTools;
using RetroEngine.Editor.Core.ViewModels.Tabs;

namespace RetroEngine.Editor.Core.Services.Factories;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public sealed class ContentBrowserViewModelFactory(
    AssetManager assetManager,
    IAssetTools assetTools,
    AssetFactorySource assetFactorySource,
    IDialogService dialogService,
    INavigationService navigationService,
    ILogger<ContentBrowserViewModel> logger
) : ViewModelFactory<ContentBrowserViewModel>
{
    public override ContentBrowserViewModel CreateViewModel()
    {
        var model = new ContentBrowserViewModel
        {
            DialogService = dialogService,
            NavigationService = navigationService,
            AssetTools = assetTools,
            AssetFactorySource = assetFactorySource,
            Logger = logger,
        };

        model.Packages.AddRange(assetManager.LoadedPackages.Select(x => new ContentBrowserPackageRoot(x)));

        return model;
    }
}
