// // @file TextLocalizationResource.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Hashing;
using System.Runtime.InteropServices;
using Serilog;

namespace RetroEngine.Portable.Localization;

public sealed class TextLocalizationResource
{
    public readonly record struct Entry(string LocalizedString, uint SourceStringHash = 0, int Priority = 0);

    private readonly Dictionary<TextId, Entry> _entries = new();
    public IReadOnlyDictionary<TextId, Entry> Entries => _entries;

    public static uint HashString(ReadOnlySpan<char> str)
    {
        var crc = new Crc32();
        crc.Append(MemoryMarshal.AsBytes(str));
        ReadOnlySpan<char> hash = ['\0'];
        crc.Append(MemoryMarshal.AsBytes(hash));
        return crc.GetCurrentHashAsUInt32();
    }

    public void AddEntry(
        TextKey @namespace,
        TextKey key,
        ReadOnlySpan<char> sourceString,
        string localizedString,
        int priority,
        TextKey locResId = default
    )
    {
        AddEntry(@namespace, key, HashString(sourceString), localizedString, priority, locResId);
    }

    public void AddEntry(
        TextKey @namespace,
        TextKey key,
        uint sourceStringHash,
        string localizedString,
        int priority,
        TextKey locResId = default
    )
    {
        var newEntry = new Entry
        {
            SourceStringHash = sourceStringHash,
            LocalizedString = localizedString,
            Priority = priority,
        };

        var textId = new TextId(@namespace, key);
        if (_entries.TryGetValue(textId, out var existingEntry))
        {
            if (ShouldReplaceEntry(@namespace, key, existingEntry, newEntry))
            {
                _entries[textId] = newEntry;
            }
        }
        else
        {
            _entries.Add(textId, newEntry);
        }
    }

    public bool IsEmpty => _entries.Count == 0;

    private static bool ShouldReplaceEntry(TextKey @namespace, TextKey key, in Entry currentEntry, in Entry newEntry)
    {
        if (newEntry.Priority < currentEntry.Priority)
            return true;

        if (newEntry.Priority > currentEntry.Priority)
            return false;

        Log.Warning("Duplicate localization entry found for {Namespace}.{Key}. Using the first one.", @namespace, key);
        return false;
    }
}
