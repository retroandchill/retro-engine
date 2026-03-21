// // @file NavigationService.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Injectio.Attributes;
using RetroEngine.Editor.Core.Services;
using RetroEngine.Editor.Core.ViewModels;
using RetroEngine.Editor.ViewModels;

namespace RetroEngine.Editor.Services;

[RegisterSingleton]
public sealed class NavigationService(ViewModelProvider viewModelProvider) : INavigationService
{
    public IMainWindowViewModel MainWindow { get; } = new MainWindowViewModel();

    public void ShowProjectOpen()
    {
        var launchScreen = viewModelProvider.CreateViewModel<LaunchScreenViewModel>();
        _ = launchScreen.OnDisplayedAsync();
        MainWindow.Content = launchScreen;
    }

    public void ShowMainEditor()
    {
        var mainEditor = viewModelProvider.CreateViewModel<MainEditorViewModel>();
        MainWindow.Content = mainEditor;
    }
}
