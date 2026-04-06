// // @file StringTableRegistry.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Strings;
using RetroEngine.Utilities.Collections;
using Serilog;

namespace RetroEngine.Portable.Localization.StringTables;

public enum StringTableLoadingPolicy : byte
{
    Find,
    FindOrLoad,
    FindOrFullyLoad,
}

public sealed class StringTableRegistry
{
    private readonly Dictionary<Name, StringTable> _registeredStringTables = new();
    private readonly Lock _registeredStringTablesLock = new();

    private readonly Dictionary<Name, HashSet<TextKey>> _loggedMissingEntries = new();
    private readonly Lock _loggedMissingEntriesLock = new();

    private StringTableRegistry() { }

    public static StringTableRegistry Instance { get; } = new();

    public void RegisterStringTable(Name tableId, StringTable table)
    {
        if (tableId.IsNone)
            throw new ArgumentException("Table ID cannot be none", nameof(tableId));

        using var scope = _registeredStringTablesLock.EnterScope();

        var existingTable = _registeredStringTables.GetValueOrDefault(tableId);
        if (existingTable is not null)
            throw new InvalidOperationException($"String table with ID {tableId} already exists.");

        _registeredStringTables.Add(tableId, table);
    }

    public bool UnregisterStringTable(Name tableId)
    {
        using var scope = _registeredStringTablesLock.EnterScope();
        return _registeredStringTables.Remove(tableId);
    }

    public StringTable? FindStringTable(Name tableId)
    {
        using var scope = _registeredStringTablesLock.EnterScope();
        return _registeredStringTables.GetValueOrDefault(tableId);
    }

    public IEnumerable<KeyValuePair<Name, StringTable>> StringTables =>
        _registeredStringTables.Lock(_registeredStringTablesLock);

    public void LogMissingStringTable(Name tableId, TextKey key)
    {
        using var scope = _loggedMissingEntriesLock.EnterScope();
        if (_loggedMissingEntries.TryGetValue(tableId, out var missingKeys))
        {
            if (missingKeys.Contains(key))
                return;
        }
        else
        {
            missingKeys = [];
            _loggedMissingEntries.Add(tableId, missingKeys);
        }

        missingKeys.Add(key);
        Log.Warning(
            "Failed to find string table entry for '{TableId}' '{Key}'. Did you forget to add a string table redirector?",
            tableId,
            key
        );
    }
}
