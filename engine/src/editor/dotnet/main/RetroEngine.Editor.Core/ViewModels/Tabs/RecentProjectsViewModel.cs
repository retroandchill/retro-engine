// // @file RecentProjectsViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Abstractions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using Microsoft.Extensions.Logging;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.Services;
using RetroEngine.Editor.Core.ViewModels.Dialogs;
using RetroEngine.Editor.Core.Views.Tabs;
using RetroEngine.Portable.Localization;

namespace RetroEngine.Editor.Core.ViewModels.Tabs;

[ViewModelFor<ProjectsTab>]
[RegisterTransient(Duplicate = DuplicateStrategy.Append)]
public sealed partial class RecentProjectsViewModel(
    IDialogService dialogService,
    IFileSystem fileSystem,
    IProjectManagementService projectManagementService,
    MainWindowViewModel mainWindow,
    ILogger<RecentProjectsViewModel> logger
) : ObservableObject, ILaunchScreenTabViewModel
{
    private const string TextNamespace = "RetroEngine.Editor.Core.Views.Tabs.RecentProjectsViewModel";
    private static readonly Text HeaderText = Text.AsLocalizable(TextNamespace, "Projects", "Projects");

    public Text Header => HeaderText;

    [RelayCommand]
    private void NewProject()
    {
        NewProjectAsync()
            .ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    logger.LogError(t.Exception, "Failed to create new project.");
                }
            });
    }

    private async Task NewProjectAsync()
    {
        var viewModel = dialogService.CreateViewModel<NewProjectWindowViewModel>();
        var result = await dialogService.ShowDialogAsync(mainWindow, viewModel);
        if (result is not true)
        {
            return;
        }

        var targetFolder = fileSystem.Path.Combine(viewModel.ProjectFolder, viewModel.ProjectName);
        if (!fileSystem.Directory.Exists(targetFolder))
        {
            fileSystem.Directory.CreateDirectory(targetFolder);
        }

        var projectFileName = $"{viewModel.ProjectName}.reproj";
        var projectPath = fileSystem.Path.Combine(targetFolder, projectFileName);
        await projectManagementService.CreateNewProjectAsync(projectPath);
    }

    [RelayCommand]
    private void OpenProject() { }
}
