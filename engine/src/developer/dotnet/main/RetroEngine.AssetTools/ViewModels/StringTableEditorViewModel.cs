// // @file StringTableEditorViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using AutoViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.RetroEngine.Controls;
using DynamicData;
using RetroEngine.Assets;
using RetroEngine.AssetTools.Views;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Localization.StringTables;

namespace RetroEngine.AssetTools.ViewModels;

public sealed partial class StringTableEntryViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Key { get; set; } = "";

    [ObservableProperty]
    public partial string SourceString { get; set; } = "";
}

[ViewModelFor<StringTableEditor>]
public sealed partial class StringTableEditorViewModel : Document, IAssetViewModel<StringTable>
{
    public required AssetPath Path { get; init; }
    public required StringTable Asset
    {
        get;
        init
        {
            field = value;
            Namespace = field.Namespace.ToString();
            _entries.Clear();
            _entries.AddRange(
                field
                    .EnumerateSourceStrings()
                    .Select(x => new StringTableEntryViewModel
                    {
                        Key = x.Key.ToString(),
                        SourceString = x.SourceString,
                    })
            );
        }
    }

    public bool IsReadOnly => false;

    [ObservableProperty]
    public partial string Namespace { get; set; } = "";

    [ObservableProperty]
    public partial string SearchTerm { get; set; } = "";

    private readonly HashSet<string> _existingKeys = new(StringComparer.OrdinalIgnoreCase);
    private readonly SourceList<StringTableEntryViewModel> _entries = new();
    private readonly ReadOnlyObservableCollection<StringTableEntryViewModel> _filteredEntries;
    public ReadOnlyObservableCollection<StringTableEntryViewModel> Entries => _filteredEntries;

    [ObservableProperty]
    public partial string PendingKey { get; set; } = "";

    [ObservableProperty]
    public partial bool EditingPendingKey { get; set; }

    [ObservableProperty]
    public partial Text PendingKeyValidationError { get; private set; }

    [ObservableProperty]
    public partial bool ShowKeyValidationError { get; private set; }

    [ObservableProperty]
    public partial string PendingSourceString { get; set; } = "";

    public StringTableEditorViewModel()
    {
        _entries
            .Connect()
            .OnItemAdded(entry =>
            {
                _existingKeys.Add(entry.Key);
                Asset!.SetSourceString(entry.Key, entry.SourceString);
            })
            .AutoRefresh(entry => entry.SourceString)
            .OnItemRefreshed(entry => Asset!.SetSourceString(entry.Key, entry.SourceString))
            .OnItemRemoved(entry =>
            {
                _existingKeys.Remove(entry.Key);
                Asset!.RemoveSourceString(entry.Key);
            })
            .Filter(entry =>
                string.IsNullOrEmpty(SearchTerm)
                || entry.Key.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
                || entry.SourceString.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)
            )
            .Bind(out _filteredEntries)
            .Subscribe();
    }

    partial void OnNamespaceChanged(string value)
    {
        Asset.Namespace = value;
    }

    [RelayCommand]
    private void AddEntry()
    {
        if (
            string.IsNullOrEmpty(PendingKey)
            || string.IsNullOrEmpty(PendingSourceString)
            || _existingKeys.Contains(PendingKey)
        )
            return;

        _entries.Add(new StringTableEntryViewModel { Key = PendingKey, SourceString = PendingSourceString });
        PendingKey = "";
        PendingSourceString = "";
    }

    [RelayCommand]
    private void RemoveEntry(StringTableEntryViewModel entry)
    {
        _entries.Remove(entry);
    }

    private static readonly Text KeyAlreadyExists = Text.AsLocalizable(
        "StringTableEditor",
        "KeyAlreadyExists",
        "Key already exists in the table."
    );

    private static readonly Text KeyCannotBeEmpty = Text.AsLocalizable(
        "StringTableEditor",
        "KeyCannotBeEmpty",
        "Key cannot be empty."
    );

    partial void OnPendingKeyChanged(string value)
    {
        if (_existingKeys.Contains(value))
        {
            PendingKeyValidationError = KeyAlreadyExists;
        }
        else if (string.IsNullOrEmpty(value))
        {
            PendingKeyValidationError = KeyCannotBeEmpty;
        }
        else
        {
            PendingKeyValidationError = Text.Empty;
        }

        ShowKeyValidationError = EditingPendingKey && !PendingKeyValidationError.IsEmpty;
    }

    partial void OnEditingPendingKeyChanged(bool value)
    {
        if (value)
        {
            ShowKeyValidationError = !PendingKeyValidationError.IsEmpty;
        }
        else
        {
            ShowKeyValidationError = false;
        }
    }
}
