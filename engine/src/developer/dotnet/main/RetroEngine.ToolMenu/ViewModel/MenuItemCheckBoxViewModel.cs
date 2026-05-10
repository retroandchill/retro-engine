// // @file MenuItemCheckBoxViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace RetroEngine.ToolMenu.ViewModel;

[DebuggerDisplay("{IsChecked} {Header}")]
internal sealed class MenuItemCheckBoxViewModel : MenuItemToggleViewModel, IMenuItemCheckBoxViewModel
{
    [DebuggerDisplay("☑ {Builder.Header}")]
    public sealed class Factory(MenuItemBuilder builder, Func<bool> getter, Action<bool> setter)
        : MenuItemViewModelFactory(builder)
    {
        public Func<bool> Getter { get; } = getter;
        public Action<bool> Setter { get; } = setter;

        public override IMenuItemViewModel? Create()
        {
            return new MenuItemCheckBoxViewModel(this);
        }
    }

    private MenuItemCheckBoxViewModel(Factory args)
        : base(args, args.Getter, args.Setter) { }
}
