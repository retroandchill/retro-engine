// // @file Tool.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.RetroEngine.Core;

namespace Dock.Model.RetroEngine.Controls;

public partial class Tool : DockableBase, ITool, IMdiDocument, IDockingWindowState
{
    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial DockRect MdiBounds { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial MdiWindowState MdiState { get; set; } = MdiWindowState.Normal;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial int MdiZIndex { get; set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public bool IsOpen
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            SetProperty(ref field, value);
            NotifyDockingWindowStateChanged(DockingWindowStateProperty.IsOpen);
        }
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public bool IsActive
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            SetProperty(ref field, value);
            NotifyDockingWindowStateChanged(DockingWindowStateProperty.IsActive);
        }
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public bool IsSelected
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            SetProperty(ref field, value);
            NotifyDockingWindowStateChanged(DockingWindowStateProperty.IsSelected);
        }
    }
}
