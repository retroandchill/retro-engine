// // @file MenuItemSeparatorViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace RetroEngine.ToolMenu.ViewModel;

[DebuggerDisplay("----------")]
internal class MenuItemSeparatorViewModel : IMenuItemViewModel
{
    public sealed class Factory(MenuItemBuilder builder) : MenuItemViewModelFactory(builder)
    {
        public override string GetDebuggerDisplay()
        {
            return "----------";
        }

        public override IMenuItemViewModel? Create()
        {
            return Instance;
        }
    }

    public static MenuItemSeparatorViewModel Instance { get; } = new MenuItemSeparatorViewModel();

    bool IMenuItemViewModel.IsEnabled => true;
    object? IMenuItemViewModel.Icon => null;
    object? IMenuItemViewModel.Header => "-";
    object? IMenuItemViewModel.ToolTip => null;
}
