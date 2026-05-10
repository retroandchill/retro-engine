// // @file MenuItemViewModelFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.ToolMenu.ViewModel;

/// <summary>
/// actual factory of <see cref="IMenuItemViewModel"/>
/// </summary>
/// <remarks>
/// Implemented by:<br/>
/// - <see cref="MenuItemSeparatorViewModel.Factory"/><br/>
/// - <see cref="MenuItemGroupViewModel.Factory"/><br/>
/// - <see cref="MenuItemCommandViewModel.Factory"/><br/>
/// - <see cref="MenuItemSelfCommandViewModel.Factory"/><br/>
/// </remarks>
public abstract class MenuItemViewModelFactory(MenuItemBuilder builder)
{
    public MenuItemBuilder Builder { get; } = builder;

    public virtual string GetDebuggerDisplay()
    {
        return Builder.Icon == null ? $"{Builder.Header}" : $"{Builder.Icon} {Builder.Header}";
    }

    public abstract IMenuItemViewModel? Create();
}
