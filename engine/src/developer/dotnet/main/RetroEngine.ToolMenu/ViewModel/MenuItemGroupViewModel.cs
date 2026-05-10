// // @file MenuItemGroupViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace RetroEngine.ToolMenu.ViewModel;

[DebuggerDisplay("{Icon} {Header}")]
internal sealed class MenuItemGroupViewModel : IMenuItemGroupViewModel
{
    #region lifecycle

    public sealed class Factory(MenuItemBuilder builder, MenuBuilder subMenu) : MenuItemViewModelFactory(builder)
    {
        public MenuBuilder Menu { get; } = subMenu;

        public override IMenuItemViewModel? Create()
        {
            var items = Menu.EnumerateMenuItems().ToList();

            if (items.Count == 0)
                return null;

            // todo: if items.Count == 1 we can skip grouping

            return new MenuItemGroupViewModel(this, items);
        }
    }

    private MenuItemGroupViewModel(MenuItemViewModelFactory builder, IEnumerable<IMenuItemViewModel> items)
    {
        Icon = builder.Builder.Icon;
        Header = builder.Builder.Header;
        ToolTip = builder.Builder.ToolTip;
        _Items.AddRange(items);
    }

    public MenuItemGroupViewModel(Object? icon, Object? header, IEnumerable<IMenuItemViewModel> items)
    {
        this.Icon = icon;
        this.Header = header;
        _Items.AddRange(items);
    }

    #endregion

    #region data

    [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
    private readonly List<IMenuItemViewModel> _Items = new List<IMenuItemViewModel>();

    #endregion

    #region properties

    public bool IsEnabled => true;
    public object? Icon { get; }
    public object? Header { get; }
    public object? ToolTip { get; }

    [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
    public IReadOnlyList<IMenuItemViewModel> Children => _Items;

    #endregion
}
