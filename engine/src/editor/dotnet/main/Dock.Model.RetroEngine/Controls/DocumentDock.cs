// // @file DocumentDock.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.Serialization;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.RetroEngine.Core;
using RetroEngine.Portable.Localization;

namespace Dock.Model.RetroEngine.Controls;

public partial class DocumentDock : DockBase, IDocumentDock, IDocumentDockFactory
{
    private const string TextNamespace = "Dock.Model.RetroEngine.Controls.DocumentDock";

    /// <summary>
    /// Initializes new instance of the <see cref="DocumentDock"/> class.
    /// </summary>
    public DocumentDock()
    {
        CreateDocument = new RelayCommand(CreateNewDocument);
        CascadeDocuments = new RelayCommand(CascadeDocumentsExecute);
        TileDocumentsHorizontal = new RelayCommand(TileDocumentsHorizontalExecute);
        TileDocumentsVertical = new RelayCommand(TileDocumentsVerticalExecute);
        RestoreDocuments = new RelayCommand(RestoreDocumentsExecute);
    }

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool CanCreateDocument { get; set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand? CreateDocument { get; set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand? CascadeDocuments { get; set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand? TileDocumentsHorizontal { get; set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand? TileDocumentsVertical { get; set; }

    /// <inheritdoc/>
    [IgnoreDataMember]
    public ICommand? RestoreDocuments { get; set; }

    /// <summary>
    /// Gets or sets factory method used to create new documents.
    /// </summary>
    [IgnoreDataMember]
    public Func<IDockable>? DocumentFactory { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial bool EnableWindowDrag { get; set; }

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial DocumentLayoutMode LayoutMode { get; set; } = DocumentLayoutMode.Tabbed;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial DocumentTabLayout TabsLayout { get; set; } = DocumentTabLayout.Top;

    /// <inheritdoc/>
    [ObservableProperty]
    [DataMember(IsRequired = false, EmitDefaultValue = true)]
    public partial DocumentCloseButtonShowMode CloseButtonShowMode { get; set; } = DocumentCloseButtonShowMode.Always;

    /// <inheritdoc/>
    [ObservableProperty]
    [IgnoreDataMember]
    public partial object? EmptyContent { get; set; } =
        Text.AsLocalizable(TextNamespace, "NoDocuments", "No documents open");

    private void CreateNewDocument()
    {
        if (DocumentFactory is not { } factory)
            return;
        var document = factory();
        AddDocument(document);
    }

    private void CascadeDocumentsExecute()
    {
        if (LayoutMode != DocumentLayoutMode.Mdi)
        {
            return;
        }

        MdiLayoutHelper.CascadeDocuments(this);
    }

    private void TileDocumentsHorizontalExecute()
    {
        if (LayoutMode != DocumentLayoutMode.Mdi)
        {
            return;
        }

        MdiLayoutHelper.TileDocumentsHorizontal(this);
    }

    private void TileDocumentsVerticalExecute()
    {
        if (LayoutMode != DocumentLayoutMode.Mdi)
        {
            return;
        }

        MdiLayoutHelper.TileDocumentsVertical(this);
    }

    private void RestoreDocumentsExecute()
    {
        if (LayoutMode != DocumentLayoutMode.Mdi)
        {
            return;
        }

        MdiLayoutHelper.RestoreDocuments(this);
    }

    /// <summary>
    /// Adds the specified document to this dock and makes it active and focused.
    /// </summary>
    /// <param name="document">The document to add.</param>
    public virtual void AddDocument(IDockable document)
    {
        Factory?.AddDockable(this, document);
        Factory?.SetActiveDockable(document);
        Factory?.SetFocusedDockable(this, document);
    }

    /// <summary>
    /// Adds the specified tool to this dock and makes it active and focused.
    /// </summary>
    /// <param name="tool">The tool to add.</param>
    public virtual void AddTool(IDockable tool)
    {
        Factory?.AddDockable(this, tool);
        Factory?.SetActiveDockable(tool);
        Factory?.SetFocusedDockable(this, tool);
    }
}
