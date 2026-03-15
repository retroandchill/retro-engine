// // @file ApplicationExtensions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace RetroEngine.Editor.Core;

public static class ApplicationExtensions
{
    extension(Application)
    {
        public static INotifyPropertyChanged MainWindowViewModel =>
            Application.Current?.ApplicationLifetime
                is IClassicDesktopStyleApplicationLifetime { MainWindow.DataContext: INotifyPropertyChanged viewModel }
                ? viewModel
                : throw new InvalidOperationException("No main window view model found.");
    }
}
