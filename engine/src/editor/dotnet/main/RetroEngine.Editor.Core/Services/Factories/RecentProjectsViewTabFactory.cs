// // @file RecentProjectsViewTabFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Abstractions;
using Avalonia;
using Avalonia.Utilities;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using RetroEngine.Editor.Core.ViewModels;
using RetroEngine.Editor.Core.ViewModels.Dialogs;
using RetroEngine.Editor.Core.ViewModels.Tabs;
using RetroEngine.Portable.Localization;

namespace RetroEngine.Editor.Core.Services.Factories;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public sealed class RecentProjectsViewTabFactory(
    IProjectManagementService projectManagementService,
    IDialogService dialogService,
    IFileSystem fileSystem
) : ViewModelFactory<RecentProjectsViewModel>, IViewModelFactory<ILaunchScreenTabViewModel>
{
    private const string TextNamespace = "RetroEngine.Editor.Core.Services.Factories.RecentProjectsViewTabFactory";

    private static readonly Text CreateNewProjectText = Text.AsLocalizable(
        TextNamespace,
        "CreateNewProjectText",
        "Create New Project"
    );

    ILaunchScreenTabViewModel IViewModelFactory<ILaunchScreenTabViewModel>.Create() => Create();

    public override RecentProjectsViewModel Create()
    {
        var viewModel = new RecentProjectsViewModel();
        viewModel.NewProjectRequested += () => _ = TryCreateNewProject();
        return viewModel;
    }

    private async Task TryCreateNewProject()
    {
        var viewModel = dialogService.CreateViewModel<NewProjectWindowViewModel>();
        var mainWindow = Application.MainWindowViewModel;
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
}
