// // @file RecentProjectsViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.IO.Abstractions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Microsoft.Extensions.Logging;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.Model.ProjectStructure;
using RetroEngine.Editor.Core.Services;
using RetroEngine.Editor.Core.ViewModels.Dialogs;
using RetroEngine.Editor.Core.Views.Tabs;
using RetroEngine.Portable.Localization;

namespace RetroEngine.Editor.Core.ViewModels.Tabs;

[ViewModelFor<RecentProjectsTab>]
[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public partial class RecentProjectsViewModel(
    IProjectManagementService projectManagementService,
    IDialogService dialogService,
    IFileSystem fileSystem,
    IMainWindowViewModel mainWindow,
    ILogger<RecentProjectsViewModel> logger
) : ObservableObject, ILaunchScreenTabViewModel
{
    private const string TextNamespace = "RetroEngine.Editor.Core.Views.Tabs.RecentProjectsViewModel";
    private static readonly Text HeaderText = Text.AsLocalizable(TextNamespace, "Projects", "Projects");

    public ObservableCollection<RecentProjectInfo> RecentProjects { get; } = [];

    public Text Header => HeaderText;

    public async Task OnDisplayedAsync(CancellationToken cancellationToken)
    {
        var projects = await projectManagementService.GetRecentProjectsAsync(cancellationToken: cancellationToken);
        RecentProjects.Clear();
        foreach (var project in projects)
        {
            RecentProjects.Add(project);
        }
    }

    [RelayCommand]
    private async Task NewProject()
    {
        try
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create new project.");

            await dialogService.ShowMessageBoxAsync(
                mainWindow,
                new MessageBoxSettings() { Icon = MessageBoxImage.Error, Content = "Failed to create new project." }
            );

            return;
        }

        mainWindow.ShowMainEditor();
    }

    [RelayCommand]
    private async Task OpenProject()
    {
        var file = await dialogService.ShowOpenFileDialogAsync(
            mainWindow,
            new OpenFileDialogSettings { Filters = [new FileFilter("RetroEngine Project", "reproj")] }
        );

        if (file is null)
        {
            return;
        }

        await OpenProject(file.LocalPath);
    }

    private async Task OpenProject(string path)
    {
        try
        {
            await projectManagementService.OpenProjectAsync(path);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to open project.");

            await dialogService.ShowMessageBoxAsync(
                mainWindow,
                new MessageBoxSettings { Icon = MessageBoxImage.Error, Content = "Failed to open project." }
            );

            return;
        }

        mainWindow.ShowMainEditor();
    }

    [RelayCommand]
    private async Task OpenRecentProject(RecentProjectInfo project)
    {
        if (project.Exists)
        {
            await OpenProject(project.Path);
        }
        else
        {
            var selection = await dialogService.ShowMessageBoxAsync(
                mainWindow,
                new MessageBoxSettings
                {
                    Icon = MessageBoxImage.Error,
                    Content = "Project file not found, would you like to delete it from the recent projects list?",
                    Button = MessageBoxButton.YesNo,
                }
            );

            if (selection is not true)
                return;

            await DeleteRecentProject(project);
        }
    }

    [RelayCommand]
    private async Task DeleteRecentProject(RecentProjectInfo project)
    {
        try
        {
            RecentProjects.Remove(project);
            await projectManagementService.RemoveRecentProjectAsync(project.Path);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete recent project.");
        }
    }
}

internal sealed class DesignRecentProjectsViewModel : RecentProjectsViewModel
{
    public DesignRecentProjectsViewModel()
        : base(null!, null!, new FileSystem(), null!, null!)
    {
        RecentProjects.Add(
            new RecentProjectInfo
            {
                Id = 1,
                Name = "Design Project",
                Path = "C:\\DesignProject.reproj",
            }
        );
    }
}
