// // @file MainDockingAreaViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using RetroEngine.Editor.Core.Model;
using RetroEngine.Portable.Localization;

namespace RetroEngine.Editor.Core.ViewModels;

public sealed class MainDockingAreaViewModel
{
    public ObservableCollection<SampleDocumentModel> Documents { get; } = [];

    public MainDockingAreaViewModel()
    {
        // Add some initial documents
        AddDocument(
            "Welcome",
            "Welcome to the ItemsSource example!",
            "This demonstrates automatic document creation from a collection."
        );
        AddDocument(
            "Documentation",
            "How to use ItemsSource",
            "Bind your collection to DocumentDock.ItemsSource and define a DocumentTemplate."
        );
    }

    private void AddDocument(Text title, Text content, Text editableContent)
    {
        Documents.Add(
            new SampleDocumentModel
            {
                Title = title,
                Content = content,
                EditableContent = editableContent,
                Status = $"Created at {DateTime.Now:HH:mm:ss}",
                CanClose = true,
            }
        );
    }
}
