// // @file DockFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using RetroEngine.Editor.Core.ViewModels.Tabs;

namespace RetroEngine.Editor.Core.ViewModels;

public class DockFactory : Factory
{
    private IRootDock? _rootDock;

    public override IRootDock CreateLayout()
    {
        var levelEditor1 = new LevelEditorViewModel { Id = "LevelEditor 1", Title = "Level Editor 1" };
        var levelEditor2 = new LevelEditorViewModel { Id = "LevelEditor2", Title = "Level Editor 2" };

        var documentDock = new DocumentDock
        {
            IsCollapsable = false,
            ActiveDockable = levelEditor1,
            VisibleDockables = CreateList<IDockable>(levelEditor1, levelEditor2),
            CanCreateDocument = false,
        };

        var rootDock = CreateRootDock();
        rootDock.ActiveDockable = documentDock;
        rootDock.DefaultDockable = documentDock;
        rootDock.VisibleDockables = CreateList<IDockable>(documentDock);

        rootDock.LeftPinnedDockables = CreateList<IDockable>();
        rootDock.RightPinnedDockables = CreateList<IDockable>();
        rootDock.TopPinnedDockables = CreateList<IDockable>();
        rootDock.BottomPinnedDockables = CreateList<IDockable>();

        rootDock.PinnedDock = null;

        _rootDock = rootDock;
        return _rootDock;
    }
}
