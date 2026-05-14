// // @file ContentBrowserViewModelFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using HanumanInstitute.MvvmDialogs;
using Microsoft.Extensions.Logging;
using RetroEngine.Assets;
using RetroEngine.AssetTools;
using RetroEngine.Editor.Core.Services.Actions;
using RetroEngine.Editor.Core.ViewModels.Tabs;

namespace RetroEngine.Editor.Core.Services.Factories;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public sealed class ContentBrowserViewModelFactory(
    IContentBrowserContextMenuBuilder contextMenuBuilder,
    IContentBrowserActions actions,
    AssetManager assetManager
) : ViewModelFactory<ContentBrowserViewModel>
{
    public override ContentBrowserViewModel CreateViewModel()
    {
        var model = new ContentBrowserViewModel(contextMenuBuilder, actions);

        model.Packages.AddRange(assetManager.LoadedPackages.Select(x => new ContentBrowserPackageRoot(x)));

        return model;
    }
}
