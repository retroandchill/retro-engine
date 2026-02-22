// // @file LocalizationManager.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using RetroEngine.Portable.Async;
using RetroEngine.Portable.Concurrency;
using RetroEngine.Portable.Localization.Cultures;
using Serilog;
using Zomp.SyncMethodGenerator;

namespace RetroEngine.Portable.Localization;

internal enum RequestedCultureOverrideLevel : byte
{
    CommandLine,
    EditorSettings,
    GameUserSettings,
    GameSettings,
    EngineSettings,
    Defaults,
}

public sealed partial class LocalizationManager
{
    private readonly record struct DisplayStringEntry(string DisplayString, uint SourceStringHash);

    private readonly ReaderWriterLockSlim _displayTableLock = new();
    private readonly Dictionary<TextId, DisplayStringEntry> _displayTable = new();

    private readonly ReaderWriterLockSlim _textRevisionLock = new();
    private readonly Dictionary<TextId, ushort> _localTextRevisions = new();
    private ushort _textRevisionCounter = 1;

    private readonly ReaderWriterLockSlim _localizedTextSourcesLock = new();
    private readonly List<ILocalizedTextSource> _localizedTextSources = [];
    private readonly AsyncSerialQueue _updateQueue = new();

    public IThreadSync? ThreadSync
    {
        get;
        set => Interlocked.Exchange(ref field, value);
    }

    private LocalizationManager()
    {
        CultureManager.Instance.OnCultureChanged += OnCultureChanged;
    }

    public static LocalizationManager Instance { get; } = new();

    public void AddOrUpdateDisplayStringInLiveTable(
        string @namespace,
        string key,
        string displayString,
        string? sourceString = null
    )
    {
        var textId = new TextId(@namespace, key);

        using var scope = _displayTableLock.EnterWriteScope();

        if (_displayTable.TryGetValue(textId, out var liveEntry))
        {
            _displayTable[textId] = liveEntry with { DisplayString = displayString };
            DirtyLocalRevisionForTextId(textId);

            Log.Information(
                "Updated string for Namespace='{Namespace}', Key='{Key}' to DisplayString='{DisplayString}'",
                @namespace,
                key,
                displayString
            );
        }
        else if (sourceString is not null)
        {
            var newLiveEntry = new DisplayStringEntry
            {
                DisplayString = displayString,
                SourceStringHash = TextLocalizationResource.HashString(displayString),
            };

            _displayTable.Add(textId, newLiveEntry);

            Log.Information(
                "Added string for Namespace='{Namespace}', Key='{Key}' to DisplayString='{DisplayString}'",
                @namespace,
                key,
                displayString
            );
        }
        else
        {
            Log.Information("No source string found for Namespace='{Namespace}', Key='{Key}'", @namespace, key);
        }
    }

    public string RequestedLanguageName =>
        GetRequestedCulture("LANGUAGE=", "Language", CultureManager.Instance.DefaultLanguage.Name, out _);

    public string RequestedLocaleName =>
        GetRequestedCulture("LOCALE=", "Locale", CultureManager.Instance.DefaultLocale.Name, out _);

    private string GetRequestedCulture(
        string? commandLineKey,
        string? configKey,
        string? defaultCulture,
        out RequestedCultureOverrideLevel overrideLevel
    )
    {
        var requestedCulture = "";
        overrideLevel = RequestedCultureOverrideLevel.CommandLine;

        // TODO: Actually read from the CLI and Config files
        if (string.IsNullOrEmpty(requestedCulture) && defaultCulture is not null)
        {
            requestedCulture = defaultCulture;
            overrideLevel = RequestedCultureOverrideLevel.Defaults;
        }

        return requestedCulture;
    }

    public string GetNativeCultureName
    {
        get
        {
            using var scope = _localizedTextSourcesLock.EnterReadScope();
            return _localizedTextSources.Select(x => x.NativeCultureName).FirstOrDefault(x => x is not null) ?? "";
        }
    }

    public List<string> GetLocalizedCultureNames(LocalizationLoadFlags flags)
    {
        using var scope = _localizedTextSourcesLock.EnterReadScope();
        var localizedCultureNameSet = new HashSet<string>();
        foreach (var localizedTextSource in _localizedTextSources)
        {
            localizedTextSource.GetLocalizedCultureNames(flags, localizedCultureNameSet);
        }

        return localizedCultureNameSet.Where(CultureManager.Instance.IsCultureAllowed).OrderBy(x => x).ToList();
    }

    public void RegisterTextSource(ILocalizedTextSource textSource, bool refreshResources = true)
    {
        using (_localizedTextSourcesLock.EnterWriteScope())
        {
            _localizedTextSources.Add(textSource);
            _localizedTextSources.Sort((x, y) => -x.Priority.CompareTo(y.Priority));
        }

        if (refreshResources)
        {
            _updateQueue.Enqueue(() => RefreshResourcesAsync());
        }
    }

    public string? FindDisplayString(TextKey @namespace, TextKey key, string? sourceString = null)
    {
        return !key.IsEmpty ? FindDisplayStringInternal(new TextId(@namespace, key), sourceString) : null;
    }

    public string? GetDisplayString(TextKey @namespace, TextKey key, string? sourceString)
    {
        if (key.IsEmpty)
            return null;

        var textId = new TextId(@namespace, key);

        var fullNamespace = textId.Namespace.ToString();
        var displayNamespace = TextNamespaceUtil.StripPackageNamespace(fullNamespace);
        if (!displayNamespace.Equals(fullNamespace, StringComparison.OrdinalIgnoreCase))
        {
            textId = textId with { Namespace = displayNamespace };
        }

        return FindDisplayStringInternal(textId, sourceString);
    }

    public void UpdateFromLocalizationResourceAsync(TextLocalizationResource localizationResource)
    {
        UpdateLiveTable(localizationResource);
    }

    public void WaitForPendingUpdates()
    {
        _updateQueue.WhenIdle().Wait();
    }

    public async ValueTask WaitForPendingUpdatesAsync()
    {
        await _updateQueue.WhenIdle();
    }

    public ushort TextRevision
    {
        get
        {
            using var scope = _textRevisionLock.EnterReadScope();
            return _textRevisionCounter;
        }
    }

    public ushort GetLocalRevisionForTextId(TextId textId)
    {
        if (textId.IsEmpty)
            return 0;

        using var scope = _textRevisionLock.EnterReadScope();
        return _localTextRevisions.GetValueOrDefault(textId);
    }

    public TextRevisions GetTextRevisions(TextId textId)
    {
        using var scope = _textRevisionLock.EnterReadScope();

        return new TextRevisions(
            _localTextRevisions.GetValueOrDefault(textId),
            !textId.IsEmpty ? _textRevisionCounter : (ushort)0
        );
    }

    public event Action? OnTextRevisionChanged;

    public void OnCultureChanged()
    {
        _updateQueue.Enqueue(() => RefreshResourcesAsync());
    }

    [CreateSyncVersion]
    public async Task RefreshResourcesAsync(CancellationToken cancellationToken = default)
    {
        var locLoadFlags = LocalizationLoadFlags.None;
        locLoadFlags |= LocalizationLoadFlags.Editor;
        locLoadFlags |= App.IsGame ? LocalizationLoadFlags.Game : LocalizationLoadFlags.None;
        locLoadFlags |= LocalizationLoadFlags.Engine;
        locLoadFlags |= LocalizationLoadFlags.Native;
        locLoadFlags |= LocalizationLoadFlags.Additional;

        await LoadLocalizationResourcesForCultureAsync(
            SnapshotLocalizedTextSources(),
            CultureManager.Instance.CurrentLanguage.Name,
            locLoadFlags,
            cancellationToken
        );
    }

    private ImmutableArray<ILocalizedTextSource> SnapshotLocalizedTextSources()
    {
        using var scope = _localizedTextSourcesLock.EnterReadScope();
        return [.. _localizedTextSources];
    }

    [CreateSyncVersion]
    private async ValueTask LoadLocalizationResourcesForCultureAsync(
        ImmutableArray<ILocalizedTextSource> availableTextSources,
        string cultureName,
        LocalizationLoadFlags loadFlags,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrEmpty(cultureName))
            return;

        var culture = CultureManager.Instance.GetCulture(cultureName);
        if (culture is null)
            return;

        var cultureNames = CultureManager.Instance.GetPrioritizedCultureNames(cultureName);

        await LoadLocalizationResourcesForPrioritizedCulturesAsync(
            availableTextSources,
            cultureNames,
            loadFlags,
            cancellationToken
        );
    }

    [CreateSyncVersion]
    private async ValueTask LoadLocalizationResourcesForPrioritizedCulturesAsync(
        ImmutableArray<ILocalizedTextSource> availableTextSources,
        List<string> prioritizedCultureNames,
        LocalizationLoadFlags loadFlags,
        CancellationToken cancellationToken = default
    )
    {
        if (prioritizedCultureNames.Count == 0)
            return;

        var textLocalizationResource = new TextLocalizationResource();

        foreach (var localizedTextSource in availableTextSources)
        {
            await localizedTextSource.LoadLocalizedResourcesAsync(
                loadFlags,
                prioritizedCultureNames,
                textLocalizationResource,
                textLocalizationResource,
                cancellationToken
            );
        }

        UpdateLiveTable(
            textLocalizationResource,
            replaceExisting: loadFlags.HasFlag(LocalizationLoadFlags.SkipExisting)
        );
    }

    private void UpdateLiveTable(
        TextLocalizationResource textLocalizationResource,
        bool dirtyTextRevision = true,
        bool replaceExisting = true
    )
    {
        using (_displayTableLock.EnterWriteScope())
        {
            _displayTable.EnsureCapacity(textLocalizationResource.Entries.Count);

            foreach (var (textId, newEntry) in textLocalizationResource.Entries)
            {
                if (!_displayTable.TryGetValue(textId, out _))
                {
                    var newLiveEntry = new DisplayStringEntry
                    {
                        SourceStringHash = newEntry.SourceStringHash,
                        DisplayString = newEntry.LocalizedString,
                    };

                    _displayTable[textId] = newLiveEntry;
                }
                else if (replaceExisting)
                {
                    _displayTable[textId] = new DisplayStringEntry(
                        SourceStringHash: newEntry.SourceStringHash,
                        DisplayString: newEntry.LocalizedString
                    );
                }
            }
        }

        if (dirtyTextRevision)
        {
            DirtyTextRevision();
        }
    }

    private void DirtyLocalRevisionForTextId(TextId textId)
    {
        using var scope = _textRevisionLock.EnterWriteScope();

        if (_localTextRevisions.TryGetValue(textId, out var localRevision))
        {
            while (++(localRevision) == 0) { }
        }
        else
        {
            _localTextRevisions.Add(textId, 1);
        }
    }

    private string? FindDisplayStringInternal(TextId textId, string? sourceString)
    {
        using var scope = _displayTableLock.EnterReadScope();
        if (!_displayTable.TryGetValue(textId, out var liveEntry))
            return null;

        if (
            string.IsNullOrEmpty(sourceString)
            || liveEntry.SourceStringHash == TextLocalizationResource.HashString(sourceString)
        )
        {
            return liveEntry.DisplayString;
        }

        return null;
    }

    private void DirtyTextRevision()
    {
        using (_textRevisionLock.EnterWriteScope())
        {
            while (++_textRevisionCounter == 0) { }
            _localTextRevisions.Clear();
        }

        if (ThreadSync is null || ThreadSync.SyncThreadId == Environment.CurrentManagedThreadId)
        {
            OnTextRevisionChanged?.Invoke();
        }
        else if (OnTextRevisionChanged is not null)
        {
            ThreadSync.RunOnPrimaryThread(OnTextRevisionChanged);
        }
    }
}
