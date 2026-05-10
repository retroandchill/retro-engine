// // @file IStorageCommandFactoryService.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Windows.Input;

namespace RetroEngine.ToolMenu.Services;

public readonly struct StorageDialogConfiguration(object configuration)
{
    public object Configuration { get; } = configuration;
}

/// <summary>
/// <see cref="ICommand"/> factory used by <see cref="MenuBuilder"/>
/// </summary>
public interface IStorageCommandFactoryService
{
    /// <summary>
    /// Creates a command when executed, it displays a folder pick dialog, then executes <paramref name="folderPickAsyncAction"/>
    /// </summary>
    /// <typeparam name="T">Valid types are: <see cref="string"/>, <see cref="Uri"/>, <see cref="DINFO"/> or Avalonia's IStorageFolder</typeparam>
    /// <param name="cmdf">A MVVM <see cref="ICommand"/> factory.</param>
    /// <param name="folderPickAsyncAction">The action called after the user picks a folder.</param>
    /// <returns>A command instance.</returns>
    ICommand CreateFolderPickerCommand<T>(
        ICommandFactoryService cmdf,
        Action<StorageDialogConfiguration> configure,
        Func<T, Task> folderPickAsyncAction
    );

    /// <summary>
    /// Creates a command when executed, it displays a file open pick dialog, then executes <paramref name="openFileAsyncAction"/>
    /// </summary>
    /// <typeparam name="T">Valid types are: <see cref="string"/>, <see cref="Uri"/>, <see cref="FINFO"/> or Avalonia's IStorageFile</typeparam>
    /// <param name="cmdf">A MVVM <see cref="ICommand"/> factory.</param>
    /// <param name="openFileAsyncAction">The action called after the user picks a file.</param>
    /// <returns>A command instance.</returns>
    ICommand CreateFileOpenCommand<T>(
        ICommandFactoryService cmdf,
        Action<StorageDialogConfiguration> configure,
        Func<T, Task> openFileAsyncAction
    );

    /// <summary>
    /// Creates a command when executed, it displays a file save pick dialog, then executes <paramref name="saveFileAsyncAction"/>
    /// </summary>
    /// <typeparam name="T">Valid types are: <see cref="string"/>, <see cref="Uri"/>, <see cref="FINFO"/> or Avalonia's IStorageFile</typeparam>
    /// <param name="cmdf">A MVVM <see cref="ICommand"/> factory.</param>
    /// <param name="saveFileAsyncAction">The action called after the user picks a file.</param>
    /// <returns>A command instance.</returns>
    ICommand CreateFileSaveCommand<T>(
        ICommandFactoryService cmdf,
        Action<StorageDialogConfiguration> configure,
        Func<T, Task> saveFileAsyncAction
    );
}
