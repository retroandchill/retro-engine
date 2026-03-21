// // @file Tool.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.Serialization;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.RetroEngine.Core;

namespace Dock.Model.RetroEngine.Controls;

public class Tool : DockableBase, ITool, IMdiDocument, IDockingWindowState
{
    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public DockRect MdiBounds
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public MdiWindowState MdiState
    {
        get;
        set => SetProperty(ref field, value);
    } = MdiWindowState.Normal;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int MdiZIndex
    {
        get;
        set => SetProperty(ref field, value);
    }

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
