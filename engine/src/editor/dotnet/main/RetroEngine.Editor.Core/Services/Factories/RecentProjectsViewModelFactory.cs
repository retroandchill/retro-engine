// // @file RecentProjectsViewModelFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Editor.Core.Services.Actions;
using RetroEngine.Editor.Core.ViewModels;
using RetroEngine.Editor.Core.ViewModels.Tabs;

namespace RetroEngine.Editor.Core.Services.Factories;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public sealed class RecentProjectsViewModelFactory(IRecentProjectsActions actions)
    : ViewModelFactory<RecentProjectsViewModel>,
        IViewModelFactory<ILaunchScreenTabViewModel>
{
    public override RecentProjectsViewModel CreateViewModel()
    {
        return new RecentProjectsViewModel(actions);
    }

    ILaunchScreenTabViewModel IViewModelFactory<ILaunchScreenTabViewModel>.CreateViewModel() => CreateViewModel();
}
