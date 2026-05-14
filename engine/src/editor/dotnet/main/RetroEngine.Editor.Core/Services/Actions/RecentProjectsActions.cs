// // @file RecentProjectsActions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Abstractions;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Microsoft.Extensions.Logging;
using RetroEngine.Editor.Core.Model.ProjectStructure;
using RetroEngine.Editor.Core.ViewModels.Dialogs;

namespace RetroEngine.Editor.Core.Services.Actions;

[RegisterSingleton]
public sealed class RecentProjectsActions(
    IDialogService dialogService,
    INavigationService navigationService,
    IFileSystem fileSystem,
    IProjectManagementService projectManagementService,
    ILogger<RecentProjectsActions> logger
) : IRecentProjectsActions
{
    public event Action<RecentProjectInfo>? OnProjectDeleted;

    public async Task<IEnumerable<RecentProjectInfo>> GetRecentProjectsAsync(
        string? searchTerm = null,
        CancellationToken cancellationToken = default
    )
    {
        return await projectManagementService.GetRecentProjectsAsync(cancellationToken: cancellationToken);
    }

    public async Task CreateNewProjectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var viewModel = dialogService.CreateViewModel<NewProjectWindowViewModel>();
            var result = await dialogService.ShowDialogAsync(navigationService.MainWindow, viewModel);
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
            await projectManagementService.CreateNewProjectAsync(projectPath, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create new project.");

            await dialogService.ShowMessageBoxAsync(
                navigationService.MainWindow,
                new MessageBoxSettings { Icon = MessageBoxImage.Error, Content = "Failed to create new project." }
            );

            return;
        }

        navigationService.ShowMainEditor();
    }

    public async Task OpenProjectAsync(CancellationToken cancellationToken = default)
    {
        var file = await dialogService.ShowOpenFileDialogAsync(
            navigationService.MainWindow,
            new OpenFileDialogSettings { Filters = [new FileFilter("RetroEngine Project", "reproj")] }
        );

        if (file is null)
        {
            return;
        }

        await OpenProjectAsync(file.LocalPath, cancellationToken);
    }

    private async Task OpenProjectAsync(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            await projectManagementService.OpenProjectAsync(path, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to open project.");

            await dialogService.ShowMessageBoxAsync(
                navigationService.MainWindow,
                new MessageBoxSettings { Icon = MessageBoxImage.Error, Content = "Failed to open project." }
            );

            return;
        }

        navigationService.ShowMainEditor();
    }

    public async Task OpenRecentProjectAsync(RecentProjectInfo project, CancellationToken cancellationToken = default)
    {
        if (project.Exists)
        {
            await OpenProjectAsync(project.Path, cancellationToken);
        }
        else
        {
            var selection = await dialogService.ShowMessageBoxAsync(
                navigationService.MainWindow,
                new MessageBoxSettings
                {
                    Icon = MessageBoxImage.Error,
                    Content = "Project file not found, would you like to delete it from the recent projects list?",
                    Button = MessageBoxButton.YesNo,
                }
            );

            if (selection is not true)
                return;

            await DeleteRecentProjectAsync(project, cancellationToken);
        }
    }

    public async Task DeleteRecentProjectAsync(RecentProjectInfo project, CancellationToken cancellationToken = default)
    {
        try
        {
            await projectManagementService.RemoveRecentProjectAsync(project.Path, cancellationToken);
            OnProjectDeleted?.Invoke(project);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete recent project.");
        }
    }
}
