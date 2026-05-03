// // @file MainEditorViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Controls;
using RetroEngine.Editor.Core.Attributes;
using RetroEngine.Editor.Core.Services;
using RetroEngine.Editor.Core.ViewModels.Menus;
using RetroEngine.Editor.Core.ViewModels.Tabs;
using RetroEngine.Editor.Core.Views;

namespace RetroEngine.Editor.Core.ViewModels;

public enum ToolPosition
{
    Left,
    Right,
    Top,
    Bottom,
}

[ViewModelFor<MainEditorView>]
[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public sealed partial class MainEditorViewModel : ObservableObject
{
    private readonly MainViewDockFactory _factory = new();
    private readonly IToolDock _leftToolDock;
    private readonly IToolDock _rightToolDock;
    private readonly IToolDock _topToolDock;
    private readonly IToolDock _bottomToolDock;
    private readonly IDocumentDock _documentDock;

    public IRootDock Layout { get; }

    [ObservableProperty]
    public partial DynamicMenuViewModel Menu { get; set; } = new() { Items = [] };

    public MainEditorViewModel()
    {
        var layout = _factory.CreateLayout();
        _factory.InitLayout(layout);
        Layout = layout;

        _leftToolDock = _factory.LeftToolDock;
        _rightToolDock = _factory.RightToolDock;
        _topToolDock = _factory.TopToolDock;
        _bottomToolDock = _factory.BottomToolDock;
        _documentDock = _factory.DocumentDock;
    }

    public void AddTool(ITool tool, ToolPosition position = ToolPosition.Left)
    {
        switch (position)
        {
            case ToolPosition.Left:
                _leftToolDock.AddTool(tool);
                break;
            case ToolPosition.Right:
                _rightToolDock.AddTool(tool);
                break;
            case ToolPosition.Top:
                _topToolDock.AddTool(tool);
                break;
            case ToolPosition.Bottom:
                _bottomToolDock.AddTool(tool);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(position), position, null);
        }
    }

    public void AddDocument(IDocument document)
    {
        _documentDock.AddDocument(document);
    }
}
