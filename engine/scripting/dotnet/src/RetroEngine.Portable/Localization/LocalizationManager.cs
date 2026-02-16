// // @file LocalizationManager.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization;

public sealed class LocalizationManager
{
    private readonly record struct LocalizationKeyEntry(ILocalizedString String, int Hash);

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

    private void CacheLocalizedString(TextId textId, ILocalizedString localizedString, int sourceHash)
    {
        // Assumes the caller holds lookup write lock
        _stringTable.Add(textId, new LocalizationKeyEntry(localizedString, sourceHash));

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
