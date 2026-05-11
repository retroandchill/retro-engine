// // @file TextInputViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ObservableCollections;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Localization.StringTables;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Editor.Core.ViewModels.Properties;

internal sealed partial class TextInputViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string SourceString { get; set; } = "";

    [ObservableProperty]
    public partial bool IsLocalized { get; set; }

    [ObservableProperty]
    public partial bool CanEditSourceString { get; private set; }

    public ObservableList<Name> StringTables { get; } = [];

    [ObservableProperty]
    public partial Name SelectedStringTable { get; set; }

    public ObservableList<TextKey> StringTableKeys { get; } = [];

    [ObservableProperty]
    public partial TextKey SelectedStringTableKey { get; set; }

    [ObservableProperty]
    public partial string Namespace { get; set; } = "";

    [ObservableProperty]
    public partial string Key { get; set; } = "";

    public TextInputViewModel()
    {
        StringTables.AddRange(StringTableRegistry.Instance.StringTables.Select(x => x.Key));
    }

    public void ImportFromText(Text text)
    {
        SourceString = text.ToString();
        IsLocalized = !text.IsCultureInvariant;
        var (ns, key) = text.TextData.History.TextId;
        if (ns == TextKey.Empty && key == TextKey.Empty)
            return;

        Namespace = ns.ToString();
        Key = key.ToString();
    }

    public Text ExportToText()
    {
        if (string.IsNullOrEmpty(SourceString))
        {
            return Text.Empty;
        }

        if (!IsLocalized)
        {
            return Text.AsCultureInvariant(SourceString);
        }

        if (!SelectedStringTable.IsNone && !SelectedStringTableKey.IsEmpty)
        {
            return Text.FromStringTable(SelectedStringTable, SelectedStringTableKey);
        }

        return Text.AsLocalizable(Namespace, Key, SourceString);
    }

    partial void OnIsLocalizedChanged(bool value)
    {
        CanEditSourceString = !value || SelectedStringTable.IsNone;
    }

    partial void OnSelectedStringTableChanged(Name value)
    {
        SelectedStringTableKey = TextKey.Empty;
        StringTableKeys.Clear();
        if (value.IsNone)
        {
            CanEditSourceString = true;
            return;
        }

        var stringTable = StringTableRegistry.Instance.FindStringTable(value);
        if (stringTable is null)
        {
            CanEditSourceString = true;
            return;
        }

        StringTableKeys.AddRange(stringTable.EnumerateSourceStrings().Select(x => x.Key));
        if (StringTableKeys.Count > 0)
        {
            SelectedStringTableKey = StringTableKeys[0];
            CanEditSourceString = !IsLocalized;
        }
        else
        {
            CanEditSourceString = true;
        }
    }

    [RelayCommand]
    public void ResetSelectedStringTable()
    {
        SelectedStringTable = Name.None;
    }
}
