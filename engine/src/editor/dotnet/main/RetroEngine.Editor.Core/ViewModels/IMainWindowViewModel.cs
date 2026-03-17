// // @file IMainWindowViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace RetroEngine.Editor.Core.ViewModels;

public interface IMainWindowViewModel : IViewModel, INotifyPropertyChanged
{
    void ShowProjectOpen();

    void ShowMainEditor();
}
