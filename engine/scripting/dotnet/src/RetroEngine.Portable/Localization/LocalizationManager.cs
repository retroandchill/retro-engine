// // @file LocalizationManager.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Globalization;
using RetroEngine.Portable.Concurrency;

namespace RetroEngine.Portable.Localization;

public sealed class LocalizationManager
{
    private readonly record struct LocalizationKeyEntry(ITextData String, int Hash);

    private readonly ReaderWriterLockSlim _lookupLock = new();
    private readonly Dictionary<TextId, LocalizationKeyEntry> _stringTable = new();

    private readonly ReaderWriterLockSlim _revisionLock = new();
    private readonly Dictionary<TextId, ushort> _localRevisions = new();
    private ushort _globalRevision = 1;

    private CultureInfo _currentLocale = CultureInfo.CurrentCulture;
    private readonly List<ILocalizedTextSource> _sources = [];

    private ICultureProvider _cultureProvider;

    public event Action? OnRevisionChanged;

    private LocalizationManager()
    {
        _cultureProvider = new DotnetCultureProvider();
        _cultureProvider.CurrentCultureChanged += HandleProviderCultureChanged;

        HandleProviderCultureChanged(_cultureProvider.CurrentCulture);
    }

    public static LocalizationManager Instance { get; } = new();

    public ushort GlobalRevision
    {
        get
        {
            using var scope = _lookupLock.EnterReadScope();
            return _globalRevision;
        }
    }

    public string? GetDisplayString(TextKey ns, TextKey key, string? fallback = null)
    {
        if (key.IsEmpty)
        {
            return !string.IsNullOrEmpty(fallback) ? fallback : null;
        }

        var textId = new TextId(ns, key);
        using (_lookupLock.EnterReadScope())
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

        return !string.IsNullOrEmpty(fallback) ? fallback : null;
    }

    public TextRevision GetTextRevision(TextId textId)
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

    public ICultureProvider CultureProvider
    {
        get
        {
            using var scope = _lookupLock.EnterReadScope();
            return _cultureProvider;
        }
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            using (_lookupLock.EnterWriteScope())
            {
                if (ReferenceEquals(_cultureProvider, value))
                    return;

                _cultureProvider.CurrentCultureChanged -= HandleProviderCultureChanged;
                _cultureProvider = value;
                _cultureProvider.CurrentCultureChanged += HandleProviderCultureChanged;
            }

            HandleProviderCultureChanged(_cultureProvider.CurrentCulture);
        }
    }

    private void CacheLocalizedString(TextId textId, ITextData textData, int sourceHash)
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

    private void HandleProviderCultureChanged(CultureInfo culture)
    {
        using (_lookupLock.EnterWriteScope())
        {
            _currentLocale = culture;
        }

        using (_revisionLock.EnterWriteScope())
        {
            if (++_globalRevision == 0)
            {
                ++_globalRevision;
            }

            _localRevisions.Clear();
        }

        OnRevisionChanged?.Invoke();
    }
}
