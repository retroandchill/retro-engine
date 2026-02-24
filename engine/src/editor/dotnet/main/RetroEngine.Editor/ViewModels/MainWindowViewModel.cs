using System.Collections.ObjectModel;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.ViewModels.Menus;
using RetroEngine.Editor.Core.ViewModels.Tabs;
using RetroEngine.Editor.Views;
using RetroEngine.Portable.Localization;

namespace RetroEngine.Editor.ViewModels;

[ViewModelFor<MainWindow>]
public partial class MainWindowViewModel : ObservableObject
{
    private const string TextNamespace = "RetroEngine.Editor.ViewModels.MainWindowViewModel";

    public DynamicMenuViewModel Toolbar { get; init; } =
        new()
        {
            Items =
            [
                new DynamicSubMenuItem
                {
                    Header = Text.AsLocalizable(TextNamespace, "FileMenuHeader", "File"),
                    Items =
                    [
                        new DynamicMenuCommand
                        {
                            Header = Text.AsLocalizable(TextNamespace, "OpenFileCommandHeader", "Open"),
                            Command = new RelayCommand(Open),
                            Gesture = new KeyGesture(Key.O, KeyModifiers.Control),
                        },
                        new DynamicMenuCommand
                        {
                            Header = Text.AsLocalizable(TextNamespace, "SaveFileCommandHeader", "Save"),
                            Command = new RelayCommand(Save),
                            Gesture = new KeyGesture(Key.S, KeyModifiers.Control),
                        },
                        new DynamicMenuSeparator(),
                        new DynamicMenuCommand
                        {
                            Header = Text.AsLocalizable(TextNamespace, "ExitCommandHeader", "Exit"),
                            Command = new RelayCommand(Exit),
                        },
                    ],
                },
                new DynamicSubMenuItem
                {
                    Header = Text.AsLocalizable(TextNamespace, "EditMenuHeader", "Edit"),
                    Items =
                    [
                        new DynamicMenuCommand
                        {
                            Header = Text.AsLocalizable(TextNamespace, "UndoCommandHeader", "Undo"),
                            Command = new RelayCommand(Undo),
                            Gesture = new KeyGesture(Key.Z, KeyModifiers.Control),
                        },
                        new DynamicMenuCommand
                        {
                            Header = Text.AsLocalizable(TextNamespace, "RedoCommandHeader", "Redo"),
                            Command = new RelayCommand(Redo),
                            Gesture = new KeyGesture(Key.Y, KeyModifiers.Control),
                        },
                    ],
                },
            ],
        };

    private static void Open() { }

    private static void Save() { }

    private static void Exit() { }

    private static void Undo() { }

    private static void Redo() { }
}
