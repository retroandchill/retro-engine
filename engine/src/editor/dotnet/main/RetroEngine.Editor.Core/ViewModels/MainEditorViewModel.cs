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
using RetroEngine.Portable.Localization;

namespace RetroEngine.Editor.Core.ViewModels;

public interface IMainEditorTabViewModel : IViewModel
{
    Text Title { get; }
}

[ViewModelFor<MainEditorView>]
[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public partial class MainEditorViewModel : ObservableObject
{
    private readonly DockFactory _dockFactory;

    public MainEditorViewModel()
    {
        _dockFactory = new DockFactory();
        var layout = _dockFactory.CreateLayout();
        _dockFactory.InitLayout(layout);
        Layout = layout;
    }

    public DynamicMenuViewModel Menu
    {
        get;
        private set => SetProperty(ref field, value);
    } = new() { Items = [] };

    public IRootDock? Layout
    {
        get;
        set => SetProperty(ref field, value);
    }
}

internal sealed class DesignMainEditorViewModel : MainEditorViewModel;
