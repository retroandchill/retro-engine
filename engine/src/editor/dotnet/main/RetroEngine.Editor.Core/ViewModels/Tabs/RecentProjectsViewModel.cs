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
public sealed partial class RecentProjectsViewModel : ObservableObject, ILaunchScreenTabViewModel
{
    private const string TextNamespace = "RetroEngine.Editor.Core.Views.Tabs.RecentProjectsViewModel";
    private static readonly Text HeaderText = Text.AsLocalizable(TextNamespace, "Projects", "Projects");

    public Text Header => HeaderText;

    public event Action? NewProjectRequested;
    public event Action? OpenProjectRequested;

    [RelayCommand]
    private void NewProject() => NewProjectRequested?.Invoke();

    [RelayCommand]
    private void OpenProject() => OpenProjectRequested?.Invoke();
}
