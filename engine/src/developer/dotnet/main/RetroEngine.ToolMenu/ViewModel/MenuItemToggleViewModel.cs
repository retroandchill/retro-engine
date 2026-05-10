// // @file MenuItemToggleViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace RetroEngine.ToolMenu.ViewModel;

[DebuggerDisplay("{IsChecked} {Header}")]
internal abstract class MenuItemToggleViewModel(MenuItemViewModelFactory args, Func<bool> getter, Action<bool> setter)
    : IMenuItemViewModel
{
    public bool IsChecked
    {
        get => getter.Invoke();
        set => setter.Invoke(value);
    }

    bool IMenuItemViewModel.IsEnabled => true;

    public object? Icon { get; } = args.Builder.Icon;

    public object? Header { get; } = args.Builder.Header;

    public object? ToolTip { get; } = args.Builder.ToolTip;
}
