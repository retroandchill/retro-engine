// // @file ProjectOpenWindowViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.ViewModels.Tabs;
using RetroEngine.Editor.Core.Views;
using RetroEngine.Portable.Localization;

namespace RetroEngine.Editor.Core.ViewModels;

public sealed record ProjectOpenWindowTab(Text Header, object? Content);

[ViewModelFor<ProjectOpenView>]
public partial class ProjectOpenWindowViewModel : ObservableObject
{
    private const string TextNamespace = "RetroEngine.Editor.ViewModels.ProjectOpenWindowViewModel";

    public ObservableCollection<ProjectOpenWindowTab> Tabs { get; } =
    [new(Text.AsLocalizable(TextNamespace, "ProjectsTabHeader", "Projects"), new RecentProjectsViewModel())];
}
