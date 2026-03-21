// // @file ToolDock.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.RetroEngine.Core;

namespace Dock.Model.RetroEngine.Controls;

public partial class ToolDock : DockBase, IToolDock
{
    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial Alignment Alignment { get; set; } = Alignment.Unset;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool IsExpanded { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool AutoHide { get; set; } = true;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial GripMode GripMode { get; set; } = GripMode.Visible;

    /// <summary>
    /// Adds the specified tool to this dock and makes it active and focused.
    /// </summary>
    /// <param name="tool">The tool to add.</param>
    public virtual void AddTool(IDockable tool)
    {
        Factory?.AddDockable(this, tool);
        Factory?.SetActiveDockable(tool);
        Factory?.SetFocusedDockable(this, tool);
    }
}
