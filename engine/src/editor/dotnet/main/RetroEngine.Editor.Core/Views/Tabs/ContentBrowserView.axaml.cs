// // @file ContentBrowserView.axaml.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using RetroEngine.Editor.Core.ViewModels.Tabs;

namespace RetroEngine.Editor.Core.Views.Tabs;

public partial class ContentBrowserView : UserControl
{
    public ContentBrowserView()
    {
        InitializeComponent();
    }

    private void OnTreeViewDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (e.Source is not Visual visual)
            return;

        var treeViewItem = visual.GetSelfAndVisualAncestors().OfType<TreeViewItem>().FirstOrDefault(x => x.IsSelected);

        if (treeViewItem?.DataContext is not ContentBrowserItem item)
            return;

        e.Handled = item.TryOpen();
    }
}
