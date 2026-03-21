// // @file SplitViewDock.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.RetroEngine.Core;

namespace Dock.Model.RetroEngine.Controls;

public partial class SplitViewDock : DockBase, ISplitViewDock
{
    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial double CompactPaneLength { get; set; } = 48.0;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial SplitViewDisplayMode DisplayMode { get; set; } = SplitViewDisplayMode.Overlay;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool IsPaneOpen { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial double OpenPaneLength { get; set; } = 320.0;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial SplitViewPanePlacement PanePlacement { get; set; } = SplitViewPanePlacement.Left;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool UseLightDismissOverlayMode { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial IDockable? PaneDockable { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial IDockable? ContentDockable { get; set; }

    /// <inheritdoc/>
    public virtual void OpenPane()
    {
        IsPaneOpen = true;
    }

    /// <inheritdoc/>
    public virtual void ClosePane()
    {
        IsPaneOpen = false;
    }

    /// <inheritdoc/>
    public virtual void TogglePane()
    {
        IsPaneOpen = !IsPaneOpen;
    }
}
