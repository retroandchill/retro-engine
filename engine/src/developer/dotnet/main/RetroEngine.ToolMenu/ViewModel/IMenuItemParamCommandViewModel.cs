// // @file IMenuItemParamCommandViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.ToolMenu.ViewModel;

public interface IMenuItemParamCommandViewModel : IMenuItemCommandViewModel
{
    string IMenuItemViewModel.StyleTag => "CommandWithParam";

    public object? CommandParameter { get; }

    public new void Execute()
    {
        Command?.Execute(CommandParameter);
    }
}
