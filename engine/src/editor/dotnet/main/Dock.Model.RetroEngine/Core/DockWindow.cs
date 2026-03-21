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

public class DockWindow : ObservableObject, ILocalizedDockWindow
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
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public string Id
    {
        get;
        set => SetProperty(ref field, value);
    } = nameof(IDockWindow);

    /// <inheritdoc/>
    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    public double X
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    public double Y
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    public double Width
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = true, EmitDefaultValue = true)]
    public double Height
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public DockWindowState WindowState
    {
        get;
        set => SetProperty(ref field, value);
    } = DockWindowState.Normal;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool Topmost
    {
        get;
        set => SetProperty(ref field, value);
    }

    string IDockWindow.Title
    {
        get => Title.ToString();
        set => Title = value;
    }

    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public Text Title
    {
        get;
        set => SetProperty(ref field, value);
    } = nameof(IDockWindow);

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public DockWindowOwnerMode OwnerMode
    {
        get;
        set => SetProperty(ref field, value);
    } = DockWindowOwnerMode.Default;

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public IDockWindow? ParentWindow
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool IsModal
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public bool? ShowInTaskbar
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public IDockable? Owner
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
    public IRootDock? Layout
    {
        get;
        set => SetProperty(ref field, value);
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public IHostWindow? Host
    {
        get;
        set => SetProperty(ref field, value);
    }

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
