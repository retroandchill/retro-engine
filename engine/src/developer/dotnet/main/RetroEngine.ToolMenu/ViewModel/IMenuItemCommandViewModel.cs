// // @file IMenuItemCommandViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Windows.Input;

namespace RetroEngine.ToolMenu.ViewModel;

public interface IMenuItemCommandViewModel : IMenuItemViewModel
{
    string IMenuItemViewModel.StyleTag => "Command";
    public ICommand Command { get; }
    public void Execute()
    {
        Command.Execute(null);
    }
}
