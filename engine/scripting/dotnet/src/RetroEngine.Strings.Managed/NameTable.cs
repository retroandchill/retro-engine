// @file $NameTable.cs.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace RetroEngine.Strings;

internal readonly record struct NameHash(int Hash, int Length);

internal readonly struct NameTableSet(StringComparison comparison)
{
    private readonly ConcurrentDictionary<NameHash, NameEntryId> _entryIndexes = new();

    public NameEntryId? Find(ReadOnlySpan<char> str)
    {
        var hash = Hash(str, comparison);
        return _entryIndexes.TryGetValue(hash, out var entryId) ? entryId : null;
    }

    public NameEntryId FindOrAdd(ReadOnlySpan<char> str, Func<ReadOnlySpan<char>, NameEntryId> addFunc)
    {
        var hash = Hash(str, comparison);
        if (_entryIndexes.TryGetValue(hash, out var entryId))
        {
            return entryId;
        }

        var newId = addFunc(str);
        _entryIndexes.TryAdd(hash, newId);
        return newId;
    }

    public NameEntryId Add(ReadOnlySpan<char> str, Func<ReadOnlySpan<char>, NameEntryId> addFunc)
    {
        var hash = Hash(str, comparison);
        var newId = addFunc(str);
        return _entryIndexes.TryAdd(hash, newId) ? newId : throw new InvalidOperationException("Duplicate name");
    }

    private static NameHash Hash(ReadOnlySpan<char> name, StringComparison comparisonType)
    {
        return new NameHash(string.GetHashCode(name, comparisonType), name.Length);
    }
}

#if RETRO_WITH_CASE_PRESERVING_NAME
internal readonly record struct NameIndices(NameEntryId ComparisonIndex, NameEntryId DisplayIndex)
#else
internal readonly record struct NameIndices(NameEntryId ComparisonIndex)
#endif
{
    public static NameIndices None =>
        new()
        {
            ComparisonIndex = NameEntryId.None,
#if RETRO_WITH_CASE_PRESERVING_NAME
            DisplayIndex = NameEntryId.None,
#endif
        };

    public bool IsNone => ComparisonIndex.IsNone;
}

internal class NameTable
{
    private readonly Lock _lock = new();
    private readonly NameTableSet _comparisonEntries = new(StringComparison.OrdinalIgnoreCase);
#if RETRO_WITH_CASE_PRESERVING_NAME
    private readonly NameTableSet _displayEntries = new(StringComparison.Ordinal);
#endif
    private readonly List<string> _entries = [];

    public static NameTable Instance { get; } = new();

    public NameTable()
    {
        GetOrAddEntryInternal(Name.NoneString, FindName.Add);
    }

    public NameIndices GetOrAddEntry(ReadOnlySpan<char> str, FindName findType)
    {
        if (str.Equals(Name.NoneString, StringComparison.OrdinalIgnoreCase))
        {
            return new NameIndices
            {
                ComparisonIndex = NameEntryId.None,
#if RETRO_WITH_CASE_PRESERVING_NAME
                DisplayIndex = NameEntryId.None,
#endif
            };
        }

        return GetOrAddEntryInternal(str, findType);
    }

    public string Get(NameEntryId id)
    {
        return !id.IsNone ? _entries[(int)id.Value] : Name.NoneString;
    }

    public int Compare(NameEntryId left, NameEntryId right, StringComparison comparison)
    {
        return Get(left).CompareTo(Get(right), comparison);
    }

    public int Compare(NameEntryId left, ReadOnlySpan<char> right, StringComparison comparison)
    {
        return Get(left).CompareTo(right, comparison);
    }

    public bool IsWithinBounds(NameEntryId index)
    {
        return index.Value < _entries.Count;
    }

    private NameIndices GetOrAddEntryInternal(ReadOnlySpan<char> str, FindName findType)
    {
        if (findType == FindName.Add)
        {
            var nextId = (uint)_entries.Count;
            var comparisonIndex = _comparisonEntries.FindOrAdd(str, CreateNewEntry);
#if RETRO_WITH_CASE_PRESERVING_NAME
            if (comparisonIndex.Value == nextId)
            {
                var displayIndex = _displayEntries.FindOrAdd(str, _ => comparisonIndex);
                ArgumentOutOfRangeException.ThrowIfNotEqual(displayIndex.Value, comparisonIndex.Value);
                return new NameIndices(comparisonIndex, displayIndex);
            }

            var displayId = _displayEntries.Add(str, CreateNewEntry);
#endif

            return new NameIndices
            {
                ComparisonIndex = comparisonIndex,
#if RETRO_WITH_CASE_PRESERVING_NAME
                DisplayIndex = displayId,
#endif
            };
        }

        var compIndex = _comparisonEntries.Find(str);
        if (compIndex is not null)
        {
            return new NameIndices
            {
                ComparisonIndex = compIndex.Value,
#if RETRO_WITH_CASE_PRESERVING_NAME
                DisplayIndex = _displayEntries.Find(str) ?? compIndex.Value,
#endif
            };
        }

        return new NameIndices
        {
            ComparisonIndex = NameEntryId.None,
#if RETRO_WITH_CASE_PRESERVING_NAME
            DisplayIndex = NameEntryId.None,
#endif
        };
    }

    private NameEntryId CreateNewEntry(ReadOnlySpan<char> str)
    {
        if (str.Length > Name.MaxLength)
            throw new ArgumentException("Name is too long");

        using var locked = _lock.EnterScope();
        var entryId = new NameEntryId((uint)_entries.Count);
        _entries.Add(str.ToString());
        return entryId;
    }
}
