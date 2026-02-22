// // @file TextLocalizationResource.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Hashing;
using System.Runtime.InteropServices;
using Serilog;
using Zomp.SyncMethodGenerator;

namespace RetroEngine.Portable.Localization;

public partial struct TextLocalizationMetaDataResource()
{
    public string NativeCulture { get; set; } = "";
    public string NativeLocRes { get; set; } = "";
    public List<string> CompiledCultures { get; } = [];
    public bool IsUGC { get; set; }

    [CreateSyncVersion]
    public async ValueTask<bool> LoadFromFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(filePath);
        return await LoadFromStreamAsync(stream, cancellationToken);
    }

    [CreateSyncVersion]
    public ValueTask<bool> LoadFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    [CreateSyncVersion]
    public async ValueTask<bool> SaveToFileAsync(
        string filePath,
        string locMetaId,
        CancellationToken cancellationToken = default
    )
    {
        await using var stream = File.OpenWrite(filePath);
        return await SaveToStreamAsync(stream, locMetaId, cancellationToken);
    }

    [CreateSyncVersion]
    public ValueTask<bool> SaveToStreamAsync(
        Stream stream,
        string locMetaId,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }
}

public sealed partial class TextLocalizationResource
{
    public readonly record struct Entry(
        string LocalizedString,
        TextKey LocResId,
        int LocalizationTargetPathId = -1,
        uint SourceStringHash = 0,
        int Priority = 0
    );

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
            LocResId = locResId,
            LocalizationTargetPathId = TextLocalizationResourceUtil.GetLocalizationTargetPathIdFromLocResId(locResId),
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

    [CreateSyncVersion]
    public async ValueTask LoadFromDirectoryAsync(
        string directoryPath,
        int priority,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var filePath in Directory.EnumerateFiles(directoryPath, "*.locres"))
        {
            await LoadFromFileAsync(filePath, priority, cancellationToken);
        }
    }

    [CreateSyncVersion]
    public async ValueTask<bool> LoadFromFileAsync(
        string filePath,
        int priority,
        CancellationToken cancellationToken = default
    )
    {
        await using var stream = File.OpenRead(filePath);
        return await LoadFromStreamAsync(stream, new TextKey(filePath), priority, cancellationToken);
    }

    [CreateSyncVersion]
    public ValueTask<bool> LoadFromStreamAsync(
        Stream stream,
        TextKey locResId,
        int priority,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    [CreateSyncVersion]
    public async ValueTask<bool> SaveToFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenWrite(filePath);
        return await SaveToStreamAsync(stream, new TextKey(filePath), cancellationToken);
    }

    [CreateSyncVersion]
    public ValueTask<bool> SaveToStreamAsync(
        Stream stream,
        TextKey locResId,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

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
