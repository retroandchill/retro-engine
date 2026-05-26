// @file RecentProjectsViewModelFactory.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Abstractions;
using HanumanInstitute.MvvmDialogs;
using RetroEngine.Editor.Core.ViewModels;
using RetroEngine.Editor.Core.ViewModels.Tabs;

namespace RetroEngine.Editor.Core.Services.Factories;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public sealed class RecentProjectsViewModelFactory(
    IProjectManagementService projectManagementService,
    IDialogService dialogService,
    IFileSystem fileSystem,
    INavigationService navigationService
) : ViewModelFactory<RecentProjectsViewModel>, IViewModelFactory<ILaunchScreenTabViewModel>
{
    public override RecentProjectsViewModel CreateViewModel()
    {
        return new RecentProjectsViewModel
        {
            ProjectManagementService = projectManagementService,
            DialogService = dialogService,
            FileSystem = fileSystem,
            NavigationService = navigationService,
        };
    }

    ILaunchScreenTabViewModel IViewModelFactory<ILaunchScreenTabViewModel>.CreateViewModel() => CreateViewModel();
}
