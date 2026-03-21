// // @file DockWindow.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Adapters;
using Dock.Model.Controls;
using Dock.Model.Core;
using RetroEngine.Portable.Localization;

namespace Dock.Model.RetroEngine.Core;

public partial class DockWindow : ObservableObject, ILocalizedDockWindow
{
    private readonly HostAdapter _hostAdapter;

    /// <summary>
    /// Initializes new instance of the <see cref="DockWindow"/> class.
    /// </summary>
    public DockWindow()
    {
        _hostAdapter = new HostAdapter(this);
    }

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial string Id { get; set; } = nameof(IDockWindow);

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    public partial double X { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    public partial double Y { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    public partial double Width { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    public partial double Height { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial DockWindowState WindowState { get; set; } = DockWindowState.Normal;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool Topmost { get; set; }

    string IDockWindow.Title
    {
        get => Title.ToString();
        set => Title = value;
    }

    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial Text Title { get; set; } = nameof(IDockWindow);

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial DockWindowOwnerMode OwnerMode { get; set; } = DockWindowOwnerMode.Default;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial IDockWindow? ParentWindow { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool IsModal { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool? ShowInTaskbar { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [IgnoreDataMember]
    public partial IDockable? Owner { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [IgnoreDataMember]
    public partial IFactory? Factory { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial IRootDock? Layout { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [IgnoreDataMember]
    public partial IHostWindow? Host { get; set; }

    /// <inheritdoc/>
    public virtual bool OnClose()
    {
        return true;
    }

    /// <inheritdoc/>
    public virtual bool OnMoveDragBegin()
    {
        return true;
    }

    /// <inheritdoc/>
    public virtual void OnMoveDrag() { }

    /// <inheritdoc/>
    public virtual void OnMoveDragEnd() { }

    /// <inheritdoc/>
    public void Save()
    {
        _hostAdapter.Save();
    }

    /// <inheritdoc/>
    public void Present(bool isDialog)
    {
        _hostAdapter.Present(isDialog);
    }

    /// <inheritdoc/>
    public void Exit()
    {
        _hostAdapter.Exit();
    }

    /// <inheritdoc/>
    public void SetActive()
    {
        _hostAdapter.SetActive();
    }
}
