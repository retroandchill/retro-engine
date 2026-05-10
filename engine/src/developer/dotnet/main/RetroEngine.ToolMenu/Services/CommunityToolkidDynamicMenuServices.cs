// // @file CommunityToolkidDynamicMenuServices.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace RetroEngine.ToolMenu.Services;

public sealed class CommunityToolkitDynamicMenuServices : ICommandFactoryService
{
    public static CommunityToolkitDynamicMenuServices Instance { get; } = new();

    private CommunityToolkitDynamicMenuServices() { }

    public ICommand CreateCommand(Action action)
    {
        return new RelayCommand(action);
    }

    public ICommand CreateCommand<T>(Action<T?> action)
    {
        return new RelayCommand<T?>(action);
    }

    public ICommand CreateCommand(Func<Task> action)
    {
        return new AsyncRelayCommand(action);
    }

    public ICommand CreateCommand<T>(Func<T?, Task> action)
    {
        return new AsyncRelayCommand<T?>(action);
    }
}
