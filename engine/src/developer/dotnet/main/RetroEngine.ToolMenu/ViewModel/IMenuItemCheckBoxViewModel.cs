// // @file IMenuItemCheckBoxViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.ToolMenu.ViewModel;

public interface IMenuItemCheckBoxViewModel : IMenuItemViewModel
{
    string IMenuItemViewModel.StyleTag => "CheckBox";
    object? IMenuItemViewModel.Icon => null;
    public bool IsChecked { get; set; }
}
