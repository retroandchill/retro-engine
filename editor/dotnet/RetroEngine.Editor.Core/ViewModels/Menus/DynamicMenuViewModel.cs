// // @file MainToolbarViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using RetroEngine.Editor.Core.Views;
using RetroEngine.Portable.Localization;

namespace RetroEngine.Editor.Core.ViewModels.Menus;

public interface IDynamicMenuItem
{
    public string Header { get; }
    public ICommand? Command => null;
    public object? CommandParameter => null;
    public KeyGesture? Gesture => null;
    public ObservableCollection<IDynamicMenuItem>? Items => null;
}

public sealed record DynamicMenuCommand : IDynamicMenuItem
{
    string IDynamicMenuItem.Header => Header.ToString();
    public required Text Header { get; init; }
    public required ICommand Command { get; init; }
    public object? CommandParameter { get; init; }
    public KeyGesture? Gesture { get; init; }
}

public sealed record DynamicSubMenuItem : IDynamicMenuItem
{
    string IDynamicMenuItem.Header => Header.ToString();
    public required Text Header { get; init; }
    public required ObservableCollection<IDynamicMenuItem> Items { get; init; }
}

public sealed record DynamicMenuSeparator : IDynamicMenuItem
{
    public string Header => "-";
}

public sealed class DynamicMenuViewModel : ObservableObject
{
    public required ObservableCollection<DynamicSubMenuItem> Items { get; init; }
}
