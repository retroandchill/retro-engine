// // @file MenuItemRadioButtonViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace RetroEngine.ToolMenu.ViewModel;

[DebuggerDisplay("{IsChecked} {Header}")]
internal sealed class MenuItemRadioButtonViewModel : MenuItemToggleViewModel, IMenuItemRadioButtonViewModel
{
    [DebuggerDisplay("🔘 {Builder.Header}")]
    public sealed class Factory(MenuItemBuilder builder, string groupName, Func<bool> getter, Action<bool> setter)
        : MenuItemViewModelFactory(builder)
    {
        public string GroupName { get; } = groupName;
        public Func<bool> Getter { get; } = getter;
        public Action<bool> Setter { get; } = setter;

        public override IMenuItemViewModel Create()
        {
            return new MenuItemRadioButtonViewModel(this);
        }
    }

    private MenuItemRadioButtonViewModel(Factory args)
        : base(args, args.Getter, args.Setter)
    {
        GroupName = args.GroupName;
    }

    public string GroupName { get; }
}
