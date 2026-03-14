// // @file NavigationService.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Editor.Core.ViewModels;

namespace RetroEngine.Editor.Core.Services;

[RegisterSingleton]
public sealed class NavigationService : INavigationService
{
    public IViewModel? CurrentViewModel
    {
        get;
        private set
        {
            field = value;
            ViewModelChanged?.Invoke(this, new ViewModelChangedEventArgs(value));
        }
    }

    public event EventHandler<ViewModelChangedEventArgs>? ViewModelChanged;

    public void ShowProjectOpen()
    {
        CurrentViewModel = new ProjectOpenViewModel();
    }

    public void ShowMainEditor()
    {
        throw new NotImplementedException();
    }
}
