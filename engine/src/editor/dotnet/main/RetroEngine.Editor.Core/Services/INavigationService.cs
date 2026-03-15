// // @file INavigationService.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using RetroEngine.Editor.Core.ViewModels;

namespace RetroEngine.Editor.Core.Services;

public readonly record struct ViewModelChangedEventArgs(IViewModel? ViewModel);

public interface INavigationService
{
    IViewModel? CurrentViewModel { get; }

    event EventHandler<ViewModelChangedEventArgs>? ViewModelChanged;

    void ShowProjectOpen();

    void ShowMainEditor();
}
