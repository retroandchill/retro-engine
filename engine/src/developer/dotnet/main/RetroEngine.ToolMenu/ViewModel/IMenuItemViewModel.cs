// // @file IMenuItemViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace RetroEngine.ToolMenu.ViewModel;

public interface IMenuItemViewModel
{
    /// <summary>
    /// This is used to determine which style to apply to the MenuItem
    /// </summary>
    /// <remarks>
    /// This is a hack used by Avalonia to define the MenuItem styles.
    /// Other frameworks may not require using this and could rely on the DataType.
    /// </remarks>
    internal string StyleTag => "Default";

    bool IsEnabled { get; }
    object? Icon { get; }
    object? Header { get; }
    object? ToolTip { get; }

    static IMenuItemViewModel Separator => MenuItemSeparatorViewModel.Instance;

    bool TryFindInTree(Predicate<IMenuItemViewModel> predicate, [NotNullWhen(true)] out IMenuItemViewModel? result)
    {
        return TryFindInTree<IMenuItemViewModel>(predicate, out result);
    }

    bool TryFindInTree<T>(Predicate<IMenuItemViewModel> predicate, [NotNullWhen(true)] out T? result)
        where T : IMenuItemViewModel
    {
        if (this is T typed && predicate(this))
        {
            result = typed;
            return true;
        }

        if (this is IMenuItemGroupViewModel collection)
        {
            return collection.TryFindInTree(predicate, out result);
        }

        result = default;
        return false;
    }
}
