// // @file MainViewViewModelFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Editor.Core.ViewModels;

namespace RetroEngine.Editor.Core.Services.Factories;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public sealed class MainEditorViewModelFactory(ViewModelProvider viewModelProvider)
    : ViewModelFactory<MainEditorViewModel>
{
    public override MainEditorViewModel CreateViewModel()
    {
        return new MainEditorViewModel { ViewModelProvider = viewModelProvider };
    }
}
