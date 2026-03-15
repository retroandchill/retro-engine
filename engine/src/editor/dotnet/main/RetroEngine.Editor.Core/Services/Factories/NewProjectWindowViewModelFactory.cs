// // @file NewProjectWindowViewModelFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Abstractions;
using HanumanInstitute.MvvmDialogs;
using Microsoft.Extensions.Logging;
using RetroEngine.Editor.Core.ViewModels.Dialogs;

namespace RetroEngine.Editor.Core.Services.Factories;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public class NewProjectWindowViewModelFactory(
    IFileSystem fileSystem,
    IDialogService dialogService,
    ILogger<NewProjectWindowViewModelFactory> logger
) : ViewModelFactory<NewProjectWindowViewModel>
{
    public override NewProjectWindowViewModel Create()
    {
        var viewModel = new NewProjectWindowViewModel(fileSystem);
        viewModel.OnProjectFolderSelectRequested += () =>
        {
            SelectProjectFolderAsync(viewModel)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        logger.LogError(t.Exception, "Failed to select project folder.");
                    }
                });
        };
        return viewModel;
    }

    private async Task SelectProjectFolderAsync(NewProjectWindowViewModel viewModel)
    {
        var targetFolder = await dialogService.ShowOpenFolderDialogAsync(viewModel);
        if (targetFolder is null)
            return;

        viewModel.ProjectFolder = targetFolder.Path.ToString();
    }
}
