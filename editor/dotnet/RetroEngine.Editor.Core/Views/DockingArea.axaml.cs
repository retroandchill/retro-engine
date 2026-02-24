// // @file MainDockingArea.axaml.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RetroEngine.Editor.Core.Views;

public partial class MainDockingArea : UserControl
{
    public MainDockingArea()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
