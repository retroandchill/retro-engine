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

public readonly record struct ProjectOpenViewTab(Text Header, object? Content);

[ViewModelFor<ProjectOpenView>]
public partial class ProjectOpenViewModel : ObservableObject
{
    public ObservableCollection<ProjectOpenViewTab> Tabs { get; } = [];
}
