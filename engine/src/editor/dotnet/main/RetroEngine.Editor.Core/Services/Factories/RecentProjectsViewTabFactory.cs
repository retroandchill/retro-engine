// // @file RecentProjectsViewTabFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia.Utilities;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using RetroEngine.Editor.Core.ViewModels;
using RetroEngine.Editor.Core.ViewModels.Tabs;
using RetroEngine.Portable.Localization;

namespace RetroEngine.Editor.Core.Services.Factories;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public sealed class RecentProjectsViewTabFactory(
    IProjectManagementService projectManagementService,
    IDialogService dialogService
) : IProjectOpenViewTabFactory
{
    private const string TextNamespace = "RetroEngine.Editor.Core.Services.Factories.RecentProjectsViewTabFactory";

    private static readonly Text TabText = Text.AsLocalizable(TextNamespace, "ProjectsTabHeader", "Projects");

    public ProjectOpenViewTab Create()
    {
        var viewModel = new RecentProjectsViewModel();
        viewModel.NewProjectRequested += () => _ = TryCreateNewProject();
        return new ProjectOpenViewTab(TabText, viewModel);
    }

    private static readonly Text CreateNewProjectText = Text.AsLocalizable(
        TextNamespace,
        "CreateNewProjectText",
        "Create New Project"
    );

    private async Task TryCreateNewProject()
    {
        var folder = await dialogService.ShowOpenFolderDialogAsync(
            null,
            new OpenFolderDialogSettings { Title = CreateNewProjectText.ToString(), AllowMultiple = false }
        );

        if (folder is null)
        {
            return;
        }
    }
}
