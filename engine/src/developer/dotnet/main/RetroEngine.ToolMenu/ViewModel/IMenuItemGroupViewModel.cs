// // @file IMenuItemGroupViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace RetroEngine.ToolMenu.ViewModel;

public interface IMenuItemGroupViewModel : IMenuItemViewModel
{
    public static IMenuItemGroupViewModel CreateGroup(object? icon, object? header, params IMenuItemViewModel[] items)
    {
        return CreateGroup(icon, header, items.AsEnumerable());
    }

    public static IMenuItemGroupViewModel CreateGroup(
        object? icon,
        object? header,
        IEnumerable<IMenuItemViewModel> items
    )
    {
        return new MenuItemGroupViewModel(icon, header, items);
    }

    string IMenuItemViewModel.StyleTag => "Group";

    IReadOnlyList<IMenuItemViewModel> Children { get; }

    new bool TryFindInTree<T>(Predicate<IMenuItemViewModel> predicate, [NotNullWhen(true)] out T? result)
        where T : IMenuItemViewModel
    {
        if (this is T typed && predicate(this))
        {
            result = typed;
            return true;
        }

        foreach (var item in Children)
        {
            if (item.TryFindInTree(predicate, out result))
                return true;
        }

        result = default;
        return false;
    }
}
