// // @file DockFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.RetroEngine;
using Dock.Model.RetroEngine.Controls;
using Dock.Settings;
using RetroEngine.Editor.Core.Services;
using RetroEngine.Editor.Core.ViewModels.Tabs;
using RetroEngine.Portable.Localization;

namespace RetroEngine.Editor.Core.ViewModels;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public sealed class MainViewDockFactory(ViewModelProvider viewModelProvider) : Factory
{
    private const string TextNamespace = "RetroEngine.Editor.Core.ViewModels.MainViewDockFactory";

    private IRootDock? _rootDock;
    private IDocumentDock? _documentDock;

    public override IRootDock CreateLayout()
    {
        var contentBrowser = viewModelProvider.CreateViewModel<ContentBrowserViewModel>();

        const string levelEditorDockGroup = "LevelEditor";

        var documentDock = new DocumentDock
        {
            IsCollapsable = false,
            ActiveDockable = null,
            VisibleDockables = CreateList<IDockable>(),
            CanCreateDocument = false,
            CanCloseLastDockable = false,
            AllowedDockOperations = DockOperationMask.Fill | DockOperationMask.Window,
            AllowedDropOperations = DockOperationMask.Fill | DockOperationMask.Window,
            DockGroup = "TopLevel",
        };

        var mainLayout = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = CreateList<IDockable>(
                new ToolDock
                {
                    Proportion = 0.25,
                    ActiveDockable = contentBrowser,
                    VisibleDockables = CreateList<IDockable>(contentBrowser),
                    Alignment = Alignment.Top,
                    GripMode = GripMode.Visible,
                    DockGroup = levelEditorDockGroup,
                },
                new ProportionalDockSplitter { ResizePreview = true },
                documentDock
            ),
            DockGroup = levelEditorDockGroup,
        };

        var rootDock = CreateRootDock();
        if (rootDock is IDockableDockingRestrictions dockingRestrictions)
        {
            dockingRestrictions.AllowedDockOperations = DockOperationMask.None;
            dockingRestrictions.AllowedDropOperations = DockOperationMask.None;
        }

        rootDock.ActiveDockable = mainLayout;
        rootDock.DefaultDockable = mainLayout;
        rootDock.VisibleDockables = CreateList<IDockable>(mainLayout);

        rootDock.LeftPinnedDockables = null;
        rootDock.RightPinnedDockables = null;
        rootDock.TopPinnedDockables = null;
        rootDock.BottomPinnedDockables = null;

        rootDock.PinnedDock = null;

        _rootDock = rootDock;
        _documentDock = documentDock;
        return _rootDock;
    }

    public override void InitLayout(IDockable layout)
    {
        DockableLocator = new Dictionary<string, Func<IDockable?>>
        {
            ["Root"] = () => _rootDock,
            ["TopLevel"] = () => _documentDock,
        };

        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
        {
            [nameof(IDockWindow)] = () => DockSettings.UseManagedWindows ? new ManagedHostWindow() : new HostWindow(),
        };

        base.InitLayout(layout);
    }
}
