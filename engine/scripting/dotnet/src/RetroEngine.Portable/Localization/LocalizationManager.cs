// // @file LocalizationManager.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization;

public sealed class LocalizationManager
{
    private readonly record struct LocalizationKeyEntry(ITextData String, int Hash);

    private readonly ReaderWriterLockSlim _lookupLock = new();
    private readonly Dictionary<TextId, LocalizationKeyEntry> _stringTable = new();

    private readonly ReaderWriterLockSlim _revisionLock = new();
    private readonly Dictionary<TextId, ushort> _localRevisions = new();
    private ushort _globalRevision = 1;

    private Locale _currentLocale = new("en_US");
    private List<ILocalizedTextSource> _sources = [];

    public event Action? OnRevisionChanged;

    private LocalizationManager() { }

    public static LocalizationManager Instance { get; } = new();

    public ushort GlobalRevision
    {
        get
        {
            _revisionLock.EnterReadLock();
            try
            {
                return _globalRevision;
            }
            finally
            {
                _revisionLock.ExitReadLock();
            }
        }
    }

    public string? GetDisplayString(TextKey ns, TextKey key, string fallback = "")
    {
        if (key.IsEmpty)
        {
            if (!string.IsNullOrEmpty(fallback))
            {
                return fallback;
            }

            return null;
        }

        var textId = new TextId(ns, key);
        _lookupLock.EnterReadLock();
        try
        {
            var currentLocale = _currentLocale;

            var result = _sources
                .Select(x => x.GetLocalizedString(textId, currentLocale))
                .FirstOrDefault(x => x is not null);
            if (result is not null)
            {
                return result;
            }
        }
        finally
        {
            _lookupLock.ExitReadLock();
        }

        return !string.IsNullOrEmpty(fallback) ? fallback : null;
    }

    public TextRevision GetTextRevision(TextId textId)
    {
        if (textId.IsEmpty)
            return new TextRevision();

        _revisionLock.EnterReadLock();
        try
        {
            return new TextRevision(GlobalRevision, _localRevisions.GetValueOrDefault(textId));
        }
        finally
        {
            _revisionLock.ExitReadLock();
        }
    }

    public void RegisterSource(ILocalizedTextSource source)
    {
        _lookupLock.EnterWriteLock();
        try
        {
            _sources.Add(source);
            _sources.Sort((a, b) => -a.Priority.CompareTo(b.Priority));
        }
        finally
        {
            _lookupLock.ExitWriteLock();
        }
    }

    public Locale CurrentLocale
    {
        get
        {
            _lookupLock.EnterReadLock();
            try
            {
                return _currentLocale;
            }
            finally
            {
                _lookupLock.ExitReadLock();
            }
        }
        set
        {
            _lookupLock.EnterWriteLock();
            try
            {
                if (_currentLocale == value)
                    return;

                _currentLocale = value;
            }
            finally
            {
                _lookupLock.ExitWriteLock();
            }

            _revisionLock.EnterWriteLock();
            try
            {
                if (++_globalRevision == 0)
                {
                    ++_globalRevision;
                }

                _localRevisions.Clear();
            }
            finally
            {
                _revisionLock.ExitWriteLock();
            }

            OnRevisionChanged?.Invoke();
        }
    }

    private void CacheLocalizedString(TextId textId, ITextData textData, int sourceHash)
    {
        // Assumes the caller holds lookup write lock
        _stringTable.Add(textId, new LocalizationKeyEntry(textData, sourceHash));

        _revisionLock.EnterWriteLock();
        try
        {
            var localRevision = _localRevisions[textId];
            if (++localRevision == 0)
            {
                ++localRevision;
            }
            _localRevisions[textId] = localRevision;
        }
        finally
        {
            _revisionLock.ExitWriteLock();
        }
    }
}
