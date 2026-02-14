// // @file MainToolbarViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.Views;

namespace RetroEngine.Editor.Core.ViewModels.Menus;

[ViewModelFor<MainToolbar>]
public sealed partial class MainToolbarViewModel : ObservableObject
{
    public ObservableCollection<MenuNode> MenuItems { get; } = [];
}
