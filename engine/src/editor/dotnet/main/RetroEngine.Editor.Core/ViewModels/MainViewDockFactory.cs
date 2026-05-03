// // @file DockFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.RetroEngine;
using Dock.Model.RetroEngine.Controls;
using Dock.Settings;

namespace RetroEngine.Editor.Core.ViewModels;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public sealed class MainViewDockFactory : Factory
{
    private const string RootDockId = "Root";
    private const string DocumentDockId = "TopLevel";

    public IToolDock? LeftToolDock { get; private set; }
    public IToolDock? RightToolDock { get; private set; }
    public IToolDock? TopToolDock { get; private set; }
    public IToolDock? BottomToolDock { get; private set; }
    private IRootDock? _rootDock;
    public IDocumentDock? DocumentDock { get; private set; }

    [MemberNotNull(
        nameof(DocumentDock),
        nameof(LeftToolDock),
        nameof(_rootDock),
        nameof(RightToolDock),
        nameof(TopToolDock),
        nameof(BottomToolDock)
    )]
    public override IRootDock CreateLayout()
    {
        var documentDock = new DocumentDock
        {
            IsCollapsable = false,
            ActiveDockable = null,
            VisibleDockables = CreateList<IDockable>(),
            CanCreateDocument = false,
            CanCloseLastDockable = false,
            AllowedDockOperations = DockOperationMask.Fill | DockOperationMask.Window,
            AllowedDropOperations = DockOperationMask.Fill | DockOperationMask.Window,
            DockGroup = DocumentDockId,
        };

        var leftDock = new ToolDock
        {
            Proportion = 0.25,
            VisibleDockables = CreateList<IDockable>(),
            Alignment = Alignment.Top,
            GripMode = GripMode.Visible,
        };

        var rightDock = new ToolDock()
        {
            Proportion = 0.25,
            VisibleDockables = CreateList<IDockable>(),
            Alignment = Alignment.Top,
            GripMode = GripMode.Visible,
        };

        var topDock = new ToolDock()
        {
            Proportion = 0.25,
            VisibleDockables = CreateList<IDockable>(),
            Alignment = Alignment.Top,
            GripMode = GripMode.Visible,
        };

        var bottomDock = new ToolDock()
        {
            Proportion = 0.25,
            VisibleDockables = CreateList<IDockable>(),
            Alignment = Alignment.Top,
            GripMode = GripMode.Visible,
        };

        var rightSplitter = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = CreateList<IDockable>(
                documentDock,
                new ProportionalDockSplitter { ResizePreview = true },
                bottomDock
            ),
            DockGroup = RootDockId,
        };

        var topSplitter = new ProportionalDock()
        {
            Orientation = Orientation.Vertical,
            VisibleDockables = CreateList<IDockable>(
                topDock,
                new ProportionalDockSplitter { ResizePreview = true },
                rightSplitter
            ),
        };

        var mainLayout = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = CreateList<IDockable>(
                leftDock,
                new ProportionalDockSplitter { ResizePreview = true },
                topSplitter
            ),
            DockGroup = RootDockId,
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
        DocumentDock = documentDock;
        LeftToolDock = leftDock;
        RightToolDock = rightDock;
        TopToolDock = topDock;
        BottomToolDock = bottomDock;
        return _rootDock;
    }

    public override void InitLayout(IDockable layout)
    {
        DockableLocator = new Dictionary<string, Func<IDockable?>>
        {
            [RootDockId] = () => _rootDock,
            [DocumentDockId] = () => DocumentDock,
        };

        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
        {
            [nameof(IDockWindow)] = () => DockSettings.UseManagedWindows ? new ManagedHostWindow() : new HostWindow(),
        };

        base.InitLayout(layout);
    }
}
