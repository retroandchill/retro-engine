// // @file IMenuItemSelfCommandViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.ToolMenu.ViewModel;

public interface IMenuItemSelfCommandViewModel : IMenuItemCommandViewModel
{
    string IMenuItemViewModel.StyleTag => "CommandWithSelf";

    public new void Execute()
    {
        throw new NotSupportedException("Requires being called from UI");
    }
}
