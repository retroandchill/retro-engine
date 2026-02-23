// // @file MenuItem.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Input;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Utils;

namespace RetroEngine.Editor.Core.ViewModels.Menus;

public abstract record MenuItemViewModel(bool IsSeparator = false);

public sealed record MenuSeparatorViewModel() : MenuItemViewModel(true)
{
    public string? Header { get; init; }
}

public sealed record MenuCommandViewModel : MenuItemViewModel
{
    public required Text Header { get; init; }
    public required ICommand Command { get; init; }
    public object? CommandParameter { get; init; }
    public KeyGesture? InputGesture { get; init; }
    public bool IsEnabled { get; init; } = true;
}

public sealed record SubMenuViewModel : MenuItemViewModel
{
    public required Text Header { get; init; }
    public required ObservableCollection<MenuItemViewModel> Children { get; init; }
    public bool IsEnabled { get; init; } = true;
}
