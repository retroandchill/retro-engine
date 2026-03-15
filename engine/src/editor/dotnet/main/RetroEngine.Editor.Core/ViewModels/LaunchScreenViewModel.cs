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

public interface ILaunchScreenTabViewModel
{
    Text Header { get; }
}

[ViewModelFor<LaunchScreenView>]
[RegisterTransient(Registration = RegistrationStrategy.Self)]
public partial class LaunchScreenViewModel(IEnumerable<ILaunchScreenTabViewModel> tabs) : ObservableObject
{
    public ObservableCollection<ILaunchScreenTabViewModel> Tabs { get; } = [.. tabs];
}
