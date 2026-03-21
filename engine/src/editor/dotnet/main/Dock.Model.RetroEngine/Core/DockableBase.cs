// // @file DockableBase.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Adapters;
using Dock.Model.Core;
using RetroEngine.Portable.Localization;

namespace Dock.Model.RetroEngine.Core;

public abstract class DockableBase
    : ObservableObject,
        ILocalizedDockable,
        IDockSelectorInfo,
        IDockableDockingRestrictions
{
    private TrackingAdapter TrackingAdapter { get; } = new();

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public string Id
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    string IDockable.Title
    {
        get => Title.ToString();
        set => Title = value;
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public Text Title
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public object? Context
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IDockable? Owner
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public IDockable? OriginalOwner
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public IFactory? Factory
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsEmpty
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsCollapsable
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double Proportion
    {
        get;
        set => SetProperty(ref field, value);
    } = double.NaN;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public DockMode Dock
    {
        get;
        set => SetProperty(ref field, value);
    } = DockMode.Center;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public DockingWindowState DockingState
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            SetProperty(ref field, value);
            NotifyDockingWindowStateChanged(DockingWindowStateProperty.DockingState);
        }
    } = DockingWindowState.Docked;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int Column
    {
        get;
        set => SetProperty(ref field, value);
    } = 0;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int Row
    {
        get;
        set => SetProperty(ref field, value);
    } = 0;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int ColumnSpan
    {
        get;
        set => SetProperty(ref field, value);
    } = 1;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public int RowSpan
    {
        get;
        set => SetProperty(ref field, value);
    } = 1;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsSharedSizeScope
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double CollapsedProportion
    {
        get;
        set => SetProperty(ref field, value);
    } = double.NaN;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double MinWidth
    {
        get;
        set => SetProperty(ref field, value);
    } = double.NaN;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double MaxWidth
    {
        get;
        set => SetProperty(ref field, value);
    } = double.NaN;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double MinHeight
    {
        get;
        set => SetProperty(ref field, value);
    } = double.NaN;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public double MaxHeight
    {
        get;
        set => SetProperty(ref field, value);
    } = double.NaN;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanClose
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanPin
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool KeepPinnedDockableVisible
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public PinnedDockDisplayMode? PinnedDockDisplayModeOverride
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public DockRect? PinnedBounds
    {
        get
        {
            GetPinnedBounds(out var x, out var y, out var width, out var height);
            return IsPinnedBoundsValid(width, height) ? new DockRect(x, y, width, height) : null;
        }
        set
        {
            if (value is null)
            {
                SetPinnedBounds(double.NaN, double.NaN, double.NaN, double.NaN);
                return;
            }

            SetPinnedBounds(value.Value.X, value.Value.Y, value.Value.Width, value.Value.Height);
        }
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanFloat
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanDrag
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanDrop
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool CanDockAsDocument
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public DockCapabilityOverrides? DockCapabilityOverrides
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsModified
    {
        get;
        set => SetProperty(ref field, value);
    } = false;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public string? DockGroup
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public DockOperationMask AllowedDockOperations
    {
        get;
        set => SetProperty(ref field, value);
    } = DockOperationMask.All;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public DockOperationMask AllowedDropOperations
    {
        get;
        set => SetProperty(ref field, value);
    } = DockOperationMask.All;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool ShowInSelector
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public string? SelectorTitle
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    public string GetControlRecyclingId() => Id;

    /// <inheritdoc/>
    public virtual bool OnClose()
    {
        return true;
    }

    /// <summary>
    /// Notifies factory that a docking-window-state property changed.
    /// </summary>
    /// <param name="property">The changed property.</param>
    protected void NotifyDockingWindowStateChanged(DockingWindowStateProperty property)
    {
        if (this is not IDockingWindowState)
        {
            return;
        }

        if (Factory is IDockingWindowStateSync stateSync)
        {
            stateSync.OnDockingWindowStatePropertyChanged(this, property);
        }
    }

    /// <inheritdoc/>
    public virtual void OnSelected() { }

    /// <inheritdoc/>
    public void GetVisibleBounds(out double x, out double y, out double width, out double height)
    {
        TrackingAdapter.GetVisibleBounds(out x, out y, out width, out height);
    }

    /// <inheritdoc/>
    public void SetVisibleBounds(double x, double y, double width, double height)
    {
        TrackingAdapter.SetVisibleBounds(x, y, width, height);
        OnVisibleBoundsChanged(x, y, width, height);
    }

    /// <inheritdoc/>
    public virtual void OnVisibleBoundsChanged(double x, double y, double width, double height) { }

    /// <inheritdoc/>
    public void GetPinnedBounds(out double x, out double y, out double width, out double height)
    {
        TrackingAdapter.GetPinnedBounds(out x, out y, out width, out height);
    }

    /// <inheritdoc/>
    public void SetPinnedBounds(double x, double y, double width, double height)
    {
        TrackingAdapter.SetPinnedBounds(x, y, width, height);
        OnPinnedBoundsChanged(x, y, width, height);
    }

    /// <inheritdoc/>
    public virtual void OnPinnedBoundsChanged(double x, double y, double width, double height) { }

    /// <inheritdoc/>
    public void GetTabBounds(out double x, out double y, out double width, out double height)
    {
        TrackingAdapter.GetTabBounds(out x, out y, out width, out height);
    }

    /// <inheritdoc/>
    public void SetTabBounds(double x, double y, double width, double height)
    {
        TrackingAdapter.SetTabBounds(x, y, width, height);
        OnTabBoundsChanged(x, y, width, height);
    }

    /// <inheritdoc/>
    public virtual void OnTabBoundsChanged(double x, double y, double width, double height) { }

    /// <inheritdoc/>
    public void GetPointerPosition(out double x, out double y)
    {
        TrackingAdapter.GetPointerPosition(out x, out y);
    }

    /// <inheritdoc/>
    public void SetPointerPosition(double x, double y)
    {
        TrackingAdapter.SetPointerPosition(x, y);
        OnPointerPositionChanged(x, y);
    }

    /// <inheritdoc/>
    public virtual void OnPointerPositionChanged(double x, double y) { }

    /// <inheritdoc/>
    public void GetPointerScreenPosition(out double x, out double y)
    {
        TrackingAdapter.GetPointerScreenPosition(out x, out y);
    }

    /// <inheritdoc/>
    public void SetPointerScreenPosition(double x, double y)
    {
        TrackingAdapter.SetPointerScreenPosition(x, y);
        OnPointerScreenPositionChanged(x, y);
    }

    /// <inheritdoc/>
    public virtual void OnPointerScreenPositionChanged(double x, double y) { }

    private static bool IsPinnedBoundsValid(double width, double height)
    {
        return !double.IsNaN(width) && !double.IsNaN(height) && !double.IsInfinity(width) && !double.IsInfinity(height);
    }
}
