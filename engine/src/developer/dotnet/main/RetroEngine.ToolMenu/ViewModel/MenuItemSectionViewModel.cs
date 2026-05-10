// // @file MenuItemSectionViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.ToolMenu.ViewModel;

internal sealed class MenuItemSectionViewModel : IMenuItemSectionViewModel
{
    public sealed class Factory(MenuItemBuilder builder, string? sectionName) : MenuItemViewModelFactory(builder)
    {
        public string? SectionName { get; } = sectionName;

        public override IMenuItemViewModel? Create()
        {
            return new MenuItemSectionViewModel(this);
        }
    }

    private MenuItemSectionViewModel(Factory args)
    {
        SectionName = args.SectionName;
    }

    public string? SectionName { get; }

    bool IMenuItemViewModel.IsEnabled => true;
    object? IMenuItemViewModel.Icon => null;
    object? IMenuItemViewModel.Header => null;
    object? IMenuItemViewModel.ToolTip => null;
}
