// // @file MenuItemSelfCommandViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Windows.Input;

namespace RetroEngine.ToolMenu.ViewModel;

[DebuggerDisplay("{Icon} {Header}")]
internal class MenuItemSelfCommandViewModel : IMenuItemSelfCommandViewModel
{
    #region lifecycle

    public sealed class Factory(MenuItemBuilder builder, ICommand command) : MenuItemViewModelFactory(builder)
    {
        public ICommand Command { get; } = command;

        public override IMenuItemViewModel? Create()
        {
            return new MenuItemSelfCommandViewModel(this);
        }
    }

    private MenuItemSelfCommandViewModel(Factory args)
    {
        Icon = args.Builder.Icon;
        Header = args.Builder.Header;
        ToolTip = args.Builder.ToolTip;
        Command = args.Command;
    }

    #endregion

    #region properties

    public ICommand Command { get; }

    public bool IsEnabled => true;

    public object? Icon { get; }

    public object? Header { get; }

    public object? ToolTip { get; }

    #endregion
}
