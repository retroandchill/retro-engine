// // @file RecentProjectsViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.Views.Tabs;
using RetroEngine.Portable.Localization;

namespace RetroEngine.Editor.Core.ViewModels.Tabs;

[ViewModelFor<ProjectsTab>]
public sealed partial class RecentProjectsViewModel : ObservableObject
{
    public event Action? NewProjectRequested;
    public event Action? OpenProjectRequested;

    [RelayCommand]
    private void NewProject() => NewProjectRequested?.Invoke();

    [RelayCommand]
    private void OpenProject() => OpenProjectRequested?.Invoke();
}
