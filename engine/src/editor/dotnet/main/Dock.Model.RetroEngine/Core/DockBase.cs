// // @file DockBase.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.Serialization;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Adapters;
using Dock.Model.Core;

namespace Dock.Model.RetroEngine.Core;

public partial class DockBase : DockableBase, IDock
{
    internal INavigateAdapter NavigateAdapter { get; }

    /// <summary>
    /// Initializes new instance of the <see cref="DockBase"/> class.
    /// </summary>
    protected DockBase()
    {
        NavigateAdapter = new NavigateAdapter(this);
        GoBack = new RelayCommand(() => NavigateAdapter.GoBack());
        GoForward = new RelayCommand(() => NavigateAdapter.GoForward());
        Navigate = new RelayCommand<object>(root => NavigateAdapter.Navigate(root, true));
        Close = new RelayCommand(() => NavigateAdapter.Close());
    }

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial IList<IDockable>? VisibleDockables { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IDockable? ActiveDockable
    {
        get;
        set
        {
            SetProperty(ref field, value);
            Factory?.InitActiveDockable(value, this);
            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(CanGoForward));
        }
    }

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial IDockable? DefaultDockable { get; set; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IDockable? FocusedDockable
    {
        get;
        set
        {
            SetProperty(ref field, value);
            Factory?.OnFocusedDockableChanged(value);
        }
    }

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool IsActive { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial int OpenedDockablesCount { get; set; } = 0;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool CanCloseLastDockable { get; set; } = true;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = false)]
    public partial DockCapabilityPolicy? DockCapabilityPolicy { get; set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public bool CanGoBack => NavigateAdapter.CanGoBack;

    /// <inheritdoc/>
    [IgnoreDataMember]
    public bool CanGoForward => NavigateAdapter.CanGoForward;

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand GoBack { get; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand GoForward { get; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand Navigate { get; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand Close { get; }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool EnableGlobalDocking
    {
        get;
        set => SetProperty(ref field, value);
    } = true;
}
