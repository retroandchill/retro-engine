// // @file RootDock.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.Serialization;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.RetroEngine.Core;

namespace Dock.Model.RetroEngine.Controls;

public class RootDock : DockBase, IRootDock, ILocalTarget
{
    /// <summary>
    /// Initializes new instance of the <see cref="RootDock"/> class.
    /// </summary>
    public RootDock()
    {
        ShowWindows = new RelayCommand(() => NavigateAdapter.ShowWindows());
        ExitWindows = new RelayCommand(() => NavigateAdapter.ExitWindows());
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsFocusableRoot
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IDockable>? HiddenDockables
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IDockable>? LeftPinnedDockables
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IDockable>? RightPinnedDockables
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IDockable>? TopPinnedDockables
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IDockable>? BottomPinnedDockables
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IToolDock? PinnedDock
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public PinnedDockDisplayMode PinnedDockDisplayMode
    {
        get;
        set => SetProperty(ref field, value);
    } = PinnedDockDisplayMode.Overlay;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IDockWindow? Window
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IList<IDockWindow>? Windows
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public DockFloatingWindowHostMode FloatingWindowHostMode
    {
        get;
        set => SetProperty(ref field, value);
    } = DockFloatingWindowHostMode.Default;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public DockCapabilityPolicy? RootDockCapabilityPolicy
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand ShowWindows { get; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand ExitWindows { get; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool EnableAdaptiveGlobalDockTargets
    {
        get;
        set => SetProperty(ref field, value);
    }
}
