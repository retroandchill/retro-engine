// // @file ICommandFactoryService.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Windows.Input;

namespace RetroEngine.ToolMenu.Services;

/// <summary>
/// <see cref="ICommand"/> factory used by <see cref="MenuBuilder"/>
/// </summary>
public interface ICommandFactoryService
{
    ICommand CreateCommand(Action action);

    ICommand CreateCommand<T>(Action<T?> action);

    ICommand CreateCommand(Func<Task> action);

    ICommand CreateCommand<T>(Func<T?, Task> action);
}
