// // @file TextLocalizationResource.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Zomp.SyncMethodGenerator;

namespace RetroEngine.Portable.Localization;

public partial struct TextLocalizationMetaDataResource()
{
    public string NativeCulture { get; set; } = "";
    public string NativeLocRes { get; set; } = "";
    public List<string> CompiledCultures { get; } = [];
    public bool IsUGC { get; set; }

    [CreateSyncVersion]
    public ValueTask<bool> LoadFromFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    [CreateSyncVersion]
    public ValueTask<bool> LoadFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    [CreateSyncVersion]
    public ValueTask<bool> SaveToFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    [CreateSyncVersion]
    public ValueTask<bool> SaveToStreamAsync(Stream stream, CancellationToken cancellationToken = default)
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
        throw new NotImplementedException();
    }

    public void AddEntry(
        TextKey @namespace,
        TextKey key,
        string sourceString,
        string localizedString,
        int priority,
        TextKey locResId = default
    )
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    public bool IsEmpty => throw new NotImplementedException();

    [CreateSyncVersion]
    public ValueTask LoadFromDirectoryAsync(
        string directoryPath,
        int priority,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    [CreateSyncVersion]
    public ValueTask<bool> LoadFromFileAsync(
        string filePath,
        int priority,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
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
    public ValueTask<bool> SaveToFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }
}
