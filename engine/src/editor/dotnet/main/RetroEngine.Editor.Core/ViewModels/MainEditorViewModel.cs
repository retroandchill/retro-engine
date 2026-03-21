// // @file MainEditorViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Controls;
using Dock.Model.Core;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.ViewModels.Menus;
using RetroEngine.Editor.Core.ViewModels.Tabs;
using RetroEngine.Editor.Core.Views;

namespace RetroEngine.Editor.Core.ViewModels;

[ViewModelFor<MainEditorView>]
[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public partial class MainEditorViewModel : ObservableObject
{
    public MainEditorViewModel()
    {
        var mainViewDockFactory = new MainViewDockFactory();
        var layout = mainViewDockFactory.CreateLayout();
        mainViewDockFactory.InitLayout(layout);
        Layout = layout;
    }

    [ObservableProperty]
    public partial DynamicMenuViewModel Menu { get; set; } = new() { Items = [] };

    [ObservableProperty]
    public partial IRootDock? Layout { get; set; }
}

internal sealed class DesignMainEditorViewModel : MainEditorViewModel;
