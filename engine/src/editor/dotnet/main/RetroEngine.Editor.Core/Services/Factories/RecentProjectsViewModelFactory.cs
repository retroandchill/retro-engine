// // @file RecentProjectsViewModelFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Abstractions;
using HanumanInstitute.MvvmDialogs;
using Microsoft.Extensions.Logging;
using RetroEngine.Editor.Core.ViewModels;
using RetroEngine.Editor.Core.ViewModels.Tabs;

namespace RetroEngine.Editor.Core.Services.Factories;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public sealed class RecentProjectsViewModelFactory(
    IProjectManagementService projectManagementService,
    IDialogService dialogService,
    IFileSystem fileSystem,
    INavigationService navigationService,
    ILogger<RecentProjectsViewModel> logger
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
            Logger = logger,
        };
    }

    ILaunchScreenTabViewModel IViewModelFactory<ILaunchScreenTabViewModel>.CreateViewModel() => CreateViewModel();
}
