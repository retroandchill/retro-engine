// // @file MainViewViewModelFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using RetroEngine.Editor.Core.ViewModels;
using RetroEngine.Editor.Core.ViewModels.Tabs;

namespace RetroEngine.Editor.Core.Services.Factories;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public sealed class MainEditorViewModelFactory(
    IViewModelFactory<ContentBrowserViewModel> factory,
    AssetViewModelProvider assetViewModelProvider,
    ILogger<MainEditorViewModel> logger
) : ViewModelFactory<MainEditorViewModel>
{
    public override MainEditorViewModel CreateViewModel()
    {
        var viewModel = new MainEditorViewModel();
        var contentBrowser = factory.CreateViewModel();
        viewModel.AddTool(contentBrowser);

        contentBrowser.AssetOpenRequested += path =>
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                try
                {
                    var document = await assetViewModelProvider.GetViewModelAsync(path);
                    viewModel.AddDocument(document);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to load asset: {Path}", path);
                }
            });
        };

        return viewModel;
    }
}
