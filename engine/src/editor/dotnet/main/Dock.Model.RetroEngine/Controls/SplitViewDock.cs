// // @file SplitViewDock.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.RetroEngine.Core;

namespace Dock.Model.RetroEngine.Controls;

public class SplitViewDock : DockBase, ISplitViewDock
{
    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double CompactPaneLength
    {
        get;
        set => SetProperty(ref field, value);
    } = 48.0;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public SplitViewDisplayMode DisplayMode
    {
        get;
        set => SetProperty(ref field, value);
    } = SplitViewDisplayMode.Overlay;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsPaneOpen
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double OpenPaneLength
    {
        get;
        set => SetProperty(ref field, value);
    } = 320.0;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public SplitViewPanePlacement PanePlacement
    {
        get;
        set => SetProperty(ref field, value);
    } = SplitViewPanePlacement.Left;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool UseLightDismissOverlayMode
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IDockable? PaneDockable
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IDockable? ContentDockable
    {
        get;
        set => SetProperty(ref field, value);
    }

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
