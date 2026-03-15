// // @file ProjectsTab.axaml.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Serilog;

namespace RetroEngine.Editor.Core.Views.Tabs;

public partial class ProjectsTab : UserControl
{
    public ProjectsTab()
    {
        InitializeComponent();
    }

    private async void NewProjectClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel is null)
            {
                throw new InvalidOperationException("TopLevel is null, cannot create new project.");
            }

            var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(
                new FolderPickerOpenOptions { AllowMultiple = false }
            );
            var folder = folders.FirstOrDefault();
            if (folder is null)
            {
                return;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create new project.");
        }
    }
}
