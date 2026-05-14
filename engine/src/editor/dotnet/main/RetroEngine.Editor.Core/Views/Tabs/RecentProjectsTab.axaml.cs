// // @file ProjectsTab.axaml.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia.Controls;
using RetroEngine.Editor.Core.Services.Actions;
using RetroEngine.Editor.Core.ViewModels.Tabs;

namespace RetroEngine.Editor.Core.Views.Tabs;

public partial class RecentProjectsTab : UserControl
{
    public RecentProjectsTab()
    {
        if (Design.IsDesignMode)
        {
            var viewModel = new RecentProjectsViewModel(new DesignTimeRecentProjectsActions());
            _ = viewModel.OnDisplayedAsync(CancellationToken.None);
            DataContext = viewModel;
        }
        InitializeComponent();
    }
}
