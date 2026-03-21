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

public abstract partial class DockableBase
    : ObservableObject,
        ILocalizedDockable,
        IDockSelectorInfo,
        IDockableDockingRestrictions
{
    private TrackingAdapter TrackingAdapter { get; } = new();

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial string Id { get; set; } = string.Empty;

    string IDockable.Title
    {
        get => Title.ToString();
        set => Title = value;
    }

    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial Text Title { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [IgnoreDataMember]
    public partial object? Context { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial IDockable? Owner { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [IgnoreDataMember]
    public partial IDockable? OriginalOwner { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [IgnoreDataMember]
    public partial IFactory? Factory { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool IsEmpty { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool IsCollapsable { get; set; } = true;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial double Proportion { get; set; } = double.NaN;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial DockMode Dock { get; set; } = DockMode.Center;

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
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial int Column { get; set; } = 0;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial int Row { get; set; } = 0;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial int ColumnSpan { get; set; } = 1;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial int RowSpan { get; set; } = 1;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool IsSharedSizeScope { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial double CollapsedProportion { get; set; } = double.NaN;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial double MinWidth { get; set; } = double.NaN;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial double MaxWidth { get; set; } = double.NaN;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial double MinHeight { get; set; } = double.NaN;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial double MaxHeight { get; set; } = double.NaN;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool CanClose { get; set; } = true;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool CanPin { get; set; } = true;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool KeepPinnedDockableVisible { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial PinnedDockDisplayMode? PinnedDockDisplayModeOverride { get; set; }

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
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool CanFloat { get; set; } = true;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool CanDrag { get; set; } = true;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool CanDrop { get; set; } = true;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool CanDockAsDocument { get; set; } = true;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public partial DockCapabilityOverrides? DockCapabilityOverrides { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool IsModified { get; set; } = false;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial string? DockGroup { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial DockOperationMask AllowedDockOperations { get; set; } = DockOperationMask.All;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial DockOperationMask AllowedDropOperations { get; set; } = DockOperationMask.All;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool ShowInSelector { get; set; } = true;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial string? SelectorTitle { get; set; }

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
