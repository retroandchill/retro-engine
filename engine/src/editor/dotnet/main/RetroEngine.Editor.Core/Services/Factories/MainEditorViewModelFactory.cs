// @file MainViewViewModelFactory.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using RetroEngine.AssetTools;
using RetroEngine.AssetTools.ViewModels;
using RetroEngine.Editor.Core.ViewModels;
using RetroEngine.Editor.Core.ViewModels.Tabs;

namespace RetroEngine.Editor.Core.Services.Factories;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public sealed class MainEditorViewModelFactory(
    IViewModelFactory<ContentBrowserViewModel> factory,
    IAssetDocumentManager assetDocumentManager,
    ILogger<MainEditorViewModel> logger
) : ViewModelFactory<MainEditorViewModel>
{
    public override MainEditorViewModel CreateViewModel()
    {
        var viewModel = new MainEditorViewModel();
        var contentBrowser = factory.CreateViewModel();
        viewModel.AddTool(contentBrowser);

        foreach (var openAsset in assetDocumentManager.GetOpenDocuments())
        {
            viewModel.AddDocument(openAsset);
        }

        contentBrowser.AssetOpenRequested += (path, asset) =>
        {
            var (document, isNew) = assetDocumentManager.OpenDocument(path, asset);
            if (isNew)
                viewModel.AddDocument(document);
            else
                viewModel.SetActiveDocument(document);
        };

        viewModel.ItemClosed += dockable =>
        {
            if (dockable is IAssetViewModel assetViewModel)
                _ = assetDocumentManager.CloseDocumentAsync(assetViewModel);
        };

        return viewModel;
    }
}
