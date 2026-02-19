// // @file LocalizationManager.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Globalization;
using RetroEngine.Portable.Concurrency;

namespace RetroEngine.Portable.Localization;

public sealed class LocalizationManager
{
    private readonly record struct LocalizationKeyEntry(string String, int Hash);

    private readonly ReaderWriterLockSlim _lookupLock = new();
    private readonly Dictionary<TextId, LocalizationKeyEntry> _stringTable = new();

    private readonly ReaderWriterLockSlim _revisionLock = new();
    private readonly Dictionary<TextId, ushort> _localRevisions = new();
    private ushort _globalRevision = 1;

    private readonly ReaderWriterLockSlim _cultureLock = new();
    private CultureHandle _currentCulture;
    public CultureHandle InvariantCulture { get; } = new(CultureInfo.InvariantCulture.Name);

    private readonly List<ILocalizedTextSource> _sources = [];

    public event Action? OnRevisionChanged;

    private LocalizationManager()
    {
        _currentCulture = new CultureHandle(CultureInfo.CurrentCulture.Name);
    }

    public static LocalizationManager Instance { get; } = new();

    internal ushort GlobalRevision
    {
        get
        {
            using var scope = _lookupLock.EnterReadScope();
            return _globalRevision;
        }
    }

    internal string? GetDisplayString(TextKey ns, TextKey key, string? fallback = null)
    {
        if (key.IsEmpty)
        {
            return !string.IsNullOrEmpty(fallback) ? fallback : null;
        }

        var textId = new TextId(ns, key);
        using var scope = _lookupLock.EnterReadScope();
        var currentLocale = CurrentCulture;

        var result = _sources
            .Select(x => x.GetLocalizedString(textId, currentLocale))
            .FirstOrDefault(x => x is not null);
        if (result is null)
            return !string.IsNullOrEmpty(fallback) ? fallback : null;

        CacheLocalizedString(textId, result, result.GetHashCode());
        return result;
    }

    internal TextRevision GetTextRevision(TextId textId)
    {
        if (textId.IsEmpty)
            return new TextRevision();

        using var scope = _lookupLock.EnterReadScope();
        return new TextRevision(GlobalRevision, _localRevisions.GetValueOrDefault(textId));
    }

    public void RegisterSource(ILocalizedTextSource source)
    {
        using var scope = _lookupLock.EnterWriteScope();
        _sources.Add(source);
        _sources.Sort((a, b) => -a.Priority.CompareTo(b.Priority));
    }

    public CultureHandle CurrentCulture
    {
        get
        {
            using var scope = _cultureLock.EnterReadScope();
            return _currentCulture;
        }
    }

    public string CurrentCultureName
    {
        get => CurrentCulture.Name;
        set
        {
            using var scope = _cultureLock.EnterWriteScope();
            _currentCulture = new CultureHandle(value);
        }
    }

    private void CacheLocalizedString(TextId textId, string textData, int sourceHash)
    {
        // Assumes the caller holds lookup write lock
        _stringTable.Add(textId, new LocalizationKeyEntry(textData, sourceHash));

        using var scope = _revisionLock.EnterWriteScope();
        var localRevision = _localRevisions[textId];
        if (++localRevision == 0)
        {
            ++localRevision;
        }
        _localRevisions[textId] = localRevision;
    }
}
