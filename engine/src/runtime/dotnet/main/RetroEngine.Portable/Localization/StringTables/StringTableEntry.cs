// // @file StringTableEntry.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization.StringTables;

public sealed class StringTableEntry(StringTable ownerTable, string sourceString, TextId displayStringId)
{
    private StringTable? _ownerTable = ownerTable;

    public bool IsOwned => _ownerTable is not null;

    public string SourceString { get; } = sourceString;

    public string? DisplayString =>
        LocalizationManager.Instance.GetDisplayString(DisplayStringId.Namespace, DisplayStringId.Key, SourceString);

    public TextId DisplayStringId { get; } = displayStringId;

    public const string PlaceholderSourceString = "<MISSING STRING TABLE ENTRY>";

    public void Disown()
    {
        _ownerTable = null;
    }

    public bool IsOwnedBy(StringTable table)
    {
        return ReferenceEquals(_ownerTable, table);
    }
}
