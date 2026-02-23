// // @file MainToolbarViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.Views;
using RetroEngine.Portable.Localization;

namespace RetroEngine.Editor.Core.ViewModels.Menus;

[ViewModelFor<MainToolbar>]
public sealed partial class MenuBar : ObservableObject
{
    public Text OpenText { get; } = Text.AsLocalizable("Editor.MenuBar", "Open", "_Open");
    public ICommand OpenCommand { get; } = new RelayCommand(Open);

    public Text SaveText { get; } = Text.AsLocalizable("Editor.MenuBar", "Save", "_Save");
    public ICommand SaveCommand { get; } = new RelayCommand(Save);

    public Text ExitText { get; } = Text.AsLocalizable("Editor.MenuBar", "Exit", "_Exit");
    public ICommand ExitCommand { get; } = new RelayCommand(Exit);

    public Text UndoText { get; } = Text.AsLocalizable("Editor.MenuBar", "Undo", "_Undo");
    public ICommand UndoCommand { get; } = new RelayCommand(Undo);

    public Text RedoText { get; } = Text.AsLocalizable("Editor.MenuBar", "Redo", "_Redo");
    public ICommand RedoCommand { get; } = new RelayCommand(Redo);

    private static void Open() { }

    private static void Save() { }

    private static void Exit() { }

    private static void Undo() { }

    private static void Redo() { }
}
