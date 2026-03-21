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
using RetroEngine.Utils;

namespace RetroEngine.Editor.Core.ViewModels.Tabs;

[ViewModelFor<RecentProjectsTab>]
public sealed partial class RecentProjectsViewModel : ObservableObject, ILaunchScreenTabViewModel
{
    private const string TextNamespace = "RetroEngine.Editor.Core.Views.Tabs.RecentProjectsViewModel";
    private static readonly Text HeaderText = Text.AsLocalizable(TextNamespace, "Projects", "Projects");

    public ObservableCollection<RecentProjectInfo> RecentProjects { get; } = [];

    public required IProjectManagementService ProjectManagementService { get; init; }

    public required IDialogService DialogService { get; init; }

    public IFileSystem FileSystem { get; init; } = IFileSystem.Default;

    public required INavigationService NavigationService { get; init; }

    public ILogger? Logger { get; init; }

    public Text Header => HeaderText;

    public async Task OnDisplayedAsync(CancellationToken cancellationToken)
    {
        var projects = await ProjectManagementService.GetRecentProjectsAsync(cancellationToken: cancellationToken);
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
            var viewModel = DialogService.CreateViewModel<NewProjectWindowViewModel>();
            var result = await DialogService.ShowDialogAsync(NavigationService.MainWindow, viewModel);
            if (result is not true)
            {
                return;
            }

            var targetFolder = FileSystem.Path.Combine(viewModel.ProjectFolder, viewModel.ProjectName);
            if (!FileSystem.Directory.Exists(targetFolder))
            {
                FileSystem.Directory.CreateDirectory(targetFolder);
            }

            var projectFileName = $"{viewModel.ProjectName}.reproj";
            var projectPath = FileSystem.Path.Combine(targetFolder, projectFileName);
            await ProjectManagementService.CreateNewProjectAsync(projectPath);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Failed to create new project.");

            await DialogService.ShowMessageBoxAsync(
                NavigationService.MainWindow,
                new MessageBoxSettings() { Icon = MessageBoxImage.Error, Content = "Failed to create new project." }
            );

            return;
        }

        NavigationService.ShowMainEditor();
    }

    [RelayCommand]
    private async Task OpenProject()
    {
        var file = await DialogService.ShowOpenFileDialogAsync(
            NavigationService.MainWindow,
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
            await ProjectManagementService.OpenProjectAsync(path);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Failed to open project.");

            await DialogService.ShowMessageBoxAsync(
                NavigationService.MainWindow,
                new MessageBoxSettings { Icon = MessageBoxImage.Error, Content = "Failed to open project." }
            );

            return;
        }

        NavigationService.ShowMainEditor();
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
            var selection = await DialogService.ShowMessageBoxAsync(
                NavigationService.MainWindow,
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
            await ProjectManagementService.RemoveRecentProjectAsync(project.Path);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Failed to delete recent project.");
        }
    }
}
