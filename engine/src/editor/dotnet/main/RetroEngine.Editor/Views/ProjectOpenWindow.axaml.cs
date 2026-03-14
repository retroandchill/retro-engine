// // @file ProjectOpenWindow.axaml.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System;
using Avalonia.Controls;
using RetroEngine.Editor.ViewModels;

namespace RetroEngine.Editor.Views;

public partial class ProjectOpenWindow : Window
{
    public ProjectOpenWindow()
    {
        InitializeComponent();
        DataContext = new ProjectOpenWindowViewModel();
        RequestAnimationFrame(Tick);
    }

    private void Tick(TimeSpan time)
    {
        if (Engine.IsInitialized)
        {
            Engine.Instance.PollPlatformEvents();
        }

        RequestAnimationFrame(Tick);
    }
}
