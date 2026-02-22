// // @file LocalizationManager.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Portable.Async;
using RetroEngine.Portable.Concurrency;
using RetroEngine.Portable.Localization.Cultures;
using Serilog;
using ZLinq;
using Zomp.SyncMethodGenerator;

namespace RetroEngine.Portable.Localization;

[Flags]
public enum LocalizationManagerInitializedFlags : byte
{
    None = 0,
    Engine = 1 << 0,
    Editor = 1 << 1,
}

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
    private sealed class DisplayStringEntry
    {
        public required string DisplayString { get; set; }
        public TextKey LocResId { get; set; }
        public int LocalizationTargetPathId { get; set; } = -1;
        public required uint SourceStringHash { get; set; }
    }

    private sealed class DisplayStringsForLocalizationTarget
    {
        public required string LocalizationTargetPath { get; init; }

        public HashSet<TextId> TextIds { get; } = [];

        public bool IsMounted { get; set; } = false;
    }

    private readonly struct DisplayStringsByLocalizationTargetId()
    {
        private readonly OrderedDictionary<string, DisplayStringsForLocalizationTarget> _localizationTargets = [];

        public DisplayStringsForLocalizationTarget FindOrAdd(string localizationTargetPath)
        {
            return FindOrAdd(localizationTargetPath, out _);
        }

        public DisplayStringsForLocalizationTarget FindOrAdd(
            string localizationTargetPath,
            out int localizationTargetPathId
        )
        {
            var normalizedLocalizationTargetPath = Path.GetDirectoryName(Path.GetFullPath(localizationTargetPath))!;

            var pathId = _localizationTargets.IndexOf(normalizedLocalizationTargetPath);
            if (pathId == -1)
            {
                pathId = _localizationTargets.Count;
                _localizationTargets.Add(
                    normalizedLocalizationTargetPath,
                    new DisplayStringsForLocalizationTarget
                    {
                        LocalizationTargetPath = normalizedLocalizationTargetPath,
                    }
                );
            }

            localizationTargetPathId = pathId;
            return _localizationTargets.GetAt(pathId).Value;
        }

        public DisplayStringsForLocalizationTarget? Find(int localizationTargetPathId)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(localizationTargetPathId);
            return _localizationTargets.Count > localizationTargetPathId
                ? _localizationTargets.GetAt(localizationTargetPathId).Value
                : null;
        }

        public void TrackTextId(int currentLocalizationPathId, int newLocalizationPathId, TextId textId)
        {
            if (currentLocalizationPathId != newLocalizationPathId)
                return;

            var currentTarget = Find(currentLocalizationPathId);
            if (currentTarget is not null && currentTarget.IsMounted)
            {
                currentTarget.TextIds.Remove(textId);
            }

            var newTarget = Find(newLocalizationPathId);
            if (newTarget is not null && !newTarget.IsMounted)
            {
                newTarget.TextIds.Add(textId);
            }
        }
    }

    private LocalizationManagerInitializedFlags _initializedFlags = LocalizationManagerInitializedFlags.None;

    public bool IsInitialized => _initializedFlags != LocalizationManagerInitializedFlags.None;

    private readonly ReaderWriterLockSlim _displayTableLock = new();
    private readonly Dictionary<TextId, DisplayStringEntry> _displayTable = new();
    private readonly DisplayStringsByLocalizationTargetId _displayStringsByLocalizationTargetId = new();

    private readonly ReaderWriterLockSlim _textRevisionLock = new();
    private readonly Dictionary<TextId, ushort> _localTextRevisions = new();
    private ushort _textRevisionCounter = 1;

    private byte _gameLocalizationPreviewAutoEnableCount;

    private readonly List<ILocalizedTextSource> _localizedTextSources = [];
    private readonly LocalizationResourceTextSource _locResTextSource = new();
    private readonly PolyglotTextSource _polyglotTextSource = new();

    private readonly AsyncSerialQueue _asyncLocalizationTaskQueue = new();

    private LocalizationManager()
    {
        const bool refreshResources = false;
        RegisterTextSource(_locResTextSource, refreshResources);
        RegisterTextSource(_polyglotTextSource, refreshResources);
    }

    public static LocalizationManager Instance { get; } = new();

    public static void BeginInitTextLocalization()
    {
        CultureManager.Instance.OnCultureChanged += Instance.OnCultureChanged;
    }

    [CreateSyncVersion]
    public static async Task InitEngineTextLocalizationAsync(CancellationToken cancellationToken = default)
    {
        await Instance.WaitForTasksAsync();

        // TODO: Actually fill this out

        Interlocked.Exchange(
            ref Instance._initializedFlags,
            Instance._initializedFlags | LocalizationManagerInitializedFlags.Engine
        );
    }

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
            liveEntry.DisplayString = displayString;
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

    public string GetNativeCultureName(LocalizedTextSourceCategory category)
    {
        return _localizedTextSources.Select(x => x.GetNativeCultureName(category)).FirstOrDefault(x => x is not null)
            ?? "";
    }

    public List<string> GetLocalizedCultureNames(LocalizationLoadFlags flags)
    {
        var localizedCultureNameSet = new HashSet<string>();
        foreach (var localizedTextSource in _localizedTextSources)
        {
            localizedTextSource.GetLocalizedCultureNames(flags, localizedCultureNameSet);
        }

        return localizedCultureNameSet.Where(CultureManager.Instance.IsCultureAllowed).OrderBy(x => x).ToList();
    }

    public int GetLocalizationTargetPathId(string localizationTargetPath)
    {
        using var scope = _displayTableLock.EnterWriteScope();
        _displayStringsByLocalizationTargetId.FindOrAdd(localizationTargetPath, out var localizationTargetPathId);
        return localizationTargetPathId;
    }

    public void RegisterTextSource(ILocalizedTextSource textSource, bool refreshResources = true)
    {
        _localizedTextSources.Add(textSource);
        _localizedTextSources.Sort((x, y) => -x.Priority.CompareTo(y.Priority));

        if (refreshResources)
        {
            _ = RefreshResourcesAsync();
        }
    }

    public void RegisterPolyglotTextData(PolyglotTextData polyglotTextData, bool addDisplayString = true)
    {
        RegisterPolyglotTextData([polyglotTextData], addDisplayString);
    }

    public void RegisterPolyglotTextData(ReadOnlySpan<PolyglotTextData> polyglotTextData, bool addDisplayString = true)
    {
        foreach (var textData in polyglotTextData.AsValueEnumerable().Where(x => x.IsValid()))
        {
            _polyglotTextSource.RegisterPolyglotTextData(textData);
        }

        if (!addDisplayString)
            return;

        var textLocalizationResource = new TextLocalizationResource();

        foreach (var textData in polyglotTextData.AsValueEnumerable().Where(x => x.IsValid()))
        {
            var localizedString = GetLocalizedStringForPolyglotData(textData);
            if (localizedString is not null)
            {
                textLocalizationResource.AddEntry(
                    textData.Namespace,
                    textData.Key,
                    textData.NativeString,
                    localizedString,
                    0
                );
            }
        }

        if (!textLocalizationResource.IsEmpty)
        {
            UpdateLiveTable(textLocalizationResource);
        }

        return;

        string? GetLocalizedStringForPolyglotData(PolyglotTextData textData)
        {
            string? cultureName;
            if (textData.Category == LocalizedTextSourceCategory.Game)
            {
                if (IsGameLocalizationPreviewEnabled)
                {
                    cultureName = ConfiguredGameLocalizationPreviewLanguage;
                }
                else if (!App.IsEditor || ShouldForceLoadGameLocalization)
                {
                    cultureName = CultureManager.Instance.CurrentLanguage.Name;
                }
                else
                {
                    cultureName = null;
                }
            }
            else
            {
                cultureName = CultureManager.Instance.CurrentLanguage.Name;
            }

            if (string.IsNullOrEmpty(cultureName))
                return !textData.IsMinimalPatch ? textData.NativeString : null;

            foreach (
                var localizedString in CultureManager
                    .Instance.GetPrioritizedCultureNames(cultureName)
                    .Select(textData.GetLocalizedString)
                    .OfType<string>()
            )
            {
                return localizedString;
            }

            return !textData.IsMinimalPatch ? textData.NativeString : null;
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

    public string? GetLocResId(TextKey @namespace, TextKey key)
    {
        using var scope = _displayTableLock.EnterReadScope();

        var textId = new TextId(@namespace, key);
        if (_displayTable.TryGetValue(textId, out var liveEntry) && !liveEntry.LocResId.IsEmpty)
        {
            return liveEntry.LocResId.ToString();
        }

        return null;
    }

    [CreateSyncVersion]
    public async ValueTask UpdateFromLocalizationResourceAsync(
        string localizationResourceFilePath,
        CancellationToken cancellationToken = default
    )
    {
        var localizationResource = new TextLocalizationResource();
        await localizationResource.LoadFromFileAsync(localizationResourceFilePath, 0, cancellationToken);
        UpdateLiveTable(localizationResource);
    }

    public void UpdateFromLocalizationResourceAsync(TextLocalizationResource localizationResource)
    {
        UpdateLiveTable(localizationResource);
    }

    public void WaitForTasks()
    {
        WaitForTasksAsync().Wait();
    }

    public Task WaitForTasksAsync()
    {
        return _asyncLocalizationTaskQueue.WhenIdle();
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
            CultureManager.Instance.CurrentLanguage.Name,
            locLoadFlags,
            cancellationToken
        );
    }

    public async Task HandleLocalizationTargetsMounted(
        ReadOnlyMemory<string> localizationTargetPaths,
        CancellationToken cancellationToken = default
    )
    {
        if (!IsInitialized || localizationTargetPaths.IsEmpty)
            return;

        using (_displayTableLock.EnterWriteScope())
        {
            foreach (var path in localizationTargetPaths.Span)
            {
                var displayStringsForLocalizationTarget = _displayStringsByLocalizationTargetId.FindOrAdd(path);
                displayStringsForLocalizationTarget.IsMounted = true;
            }
        }

        var locLoadFlags = LocalizationLoadFlags.None;
        locLoadFlags |= LocalizationLoadFlags.Editor;
        locLoadFlags |= App.IsGame ? LocalizationLoadFlags.Game : LocalizationLoadFlags.None;
        locLoadFlags |= LocalizationLoadFlags.Engine;
        locLoadFlags |= LocalizationLoadFlags.Native;
        locLoadFlags |= LocalizationLoadFlags.Additional;

        var prioritizedCultureNames = CultureManager.Instance.GetPrioritizedCultureNames(
            CultureManager.Instance.CurrentLanguage.Name
        );
        var targetPaths = localizationTargetPaths.ToArray();
        var availableTextSources = _localizedTextSources.ToArray();

        await QueueAsyncTask(async () =>
        {
            foreach (var localizationTargetPath in targetPaths.Where(Directory.Exists))
            {
                var locMetaResource = new TextLocalizationMetaDataResource();
                var locMetaFilename = $"{Path.GetDirectoryName(localizationTargetPath)}.locmeta";
                await locMetaResource.LoadFromFileAsync(
                    Path.Join(localizationTargetPath, locMetaFilename),
                    cancellationToken
                );

                var finalLocLoadFlags =
                    locLoadFlags
                    | (locMetaResource.IsUGC ? LocalizationLoadFlags.SkipExisting : LocalizationLoadFlags.None);
                LoadLocalizationTargetsForPrioritizedCultures(
                    availableTextSources,
                    [localizationTargetPath],
                    CollectionsMarshal.AsSpan(prioritizedCultureNames),
                    finalLocLoadFlags
                );
            }
        });
    }

    public async Task HandleLocalizationTargetsUnmounted(
        ReadOnlyMemory<string> localizationTargetPaths,
        CancellationToken cancellationToken = default
    )
    {
        if (!IsInitialized || localizationTargetPaths.IsEmpty)
            return;

        await QueueAsyncTask(() =>
        {
            var textCache = TextCache.Instance;

            using var scope = _displayTableLock.EnterWriteScope();

            foreach (var localizationTargetPath in localizationTargetPaths.Span)
            {
                var displayStringsForLocalizationTarget = _displayStringsByLocalizationTargetId.FindOrAdd(
                    localizationTargetPath
                );
                if (!displayStringsForLocalizationTarget.IsMounted)
                    continue;
                foreach (var textId in displayStringsForLocalizationTarget.TextIds)
                {
                    _displayTable.Remove(textId);
                }
                textCache.RemoveCache(displayStringsForLocalizationTarget.TextIds);

                displayStringsForLocalizationTarget.TextIds.Clear();
                displayStringsForLocalizationTarget.IsMounted = false;
            }

            DirtyTextRevision();
        });
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

    public void EnableGameLocalizationPreview()
    {
        EnableGameLocalizationPreview(ConfiguredGameLocalizationPreviewLanguage);
    }

    private static bool IsLocalizationLockedByConfig => false;

    public void EnableGameLocalizationPreview(string cultureName)
    {
        if (!App.IsEditor)
            return;

        var nativeGameCulture = GetNativeCultureName(LocalizedTextSourceCategory.Game);
        if (string.IsNullOrEmpty(nativeGameCulture))
            return;

        var previewCulture = string.IsNullOrEmpty(cultureName) ? nativeGameCulture : cultureName;
        IsGameLocalizationPreviewEnabled = previewCulture != nativeGameCulture;
        IsLocalizationLocked = IsLocalizationLockedByConfig || IsGameLocalizationPreviewEnabled;

        var prioritizedCultureNames = IsGameLocalizationPreviewEnabled
            ? CultureManager.Instance.GetPrioritizedCultureNames(previewCulture).ToArray()
            : [previewCulture];
        var locLoadFlags = LocalizationLoadFlags.Game | LocalizationLoadFlags.Additional;
        locLoadFlags |= IsGameLocalizationPreviewEnabled ? LocalizationLoadFlags.Native : LocalizationLoadFlags.None;

        _ = LoadLocalizationResourcesForPrioritizedCulturesAsync(prioritizedCultureNames, locLoadFlags);
    }

    public void DisableGameLocalizationPreview()
    {
        EnableGameLocalizationPreview(GetNativeCultureName(LocalizedTextSourceCategory.Game));
    }

    public bool IsGameLocalizationPreviewEnabled { get; private set; }

    public void PushAutoEnableGameLocalizationPreview()
    {
        ++_gameLocalizationPreviewAutoEnableCount;
    }

    public void PopAutoEnableGameLocalizationPreview()
    {
        --_gameLocalizationPreviewAutoEnableCount;
    }

    public bool ShouldGameLocalizationPreviewAutoEnable => _gameLocalizationPreviewAutoEnableCount > 0;

    public void ConfigureGameLocalizationPreviewLanguage(string cultureName)
    {
        // TODO: Actually read from the Config file
    }

    // TODO: Actually read from the Config file
    public string ConfiguredGameLocalizationPreviewLanguage => "";

    public bool IsLocalizationLocked { get; private set; }

    public bool ShouldForceLoadGameLocalization => App.IsEditor;

    public event Action? OnTextRevsionChanged;

    public void OnCultureChanged()
    {
        if (!IsInitialized)
            return;

        RefreshResources();
    }

    private void LoadLocalizationResourcesForCulture(
        ReadOnlySpan<ILocalizedTextSource> availableTextSources,
        string cultureName,
        LocalizationLoadFlags loadFlags
    )
    {
        if (string.IsNullOrEmpty(cultureName))
            return;

        var culture = CultureManager.Instance.GetCulture(cultureName);
        if (culture is null)
            return;

        var prioritizedCultureNames = CultureManager.Instance.GetPrioritizedCultureNames(cultureName);
        LoadLocalizationResourcesForPrioritizedCultures(
            availableTextSources,
            CollectionsMarshal.AsSpan(prioritizedCultureNames),
            loadFlags
        );
    }

    private void LoadLocalizationResourcesForCultureAsync(
        ReadOnlySpan<ILocalizedTextSource> availableTextSources,
        string cultureName,
        LocalizationLoadFlags loadFlags
    )
    {
        if (string.IsNullOrEmpty(cultureName))
            return;

        var culture = CultureManager.Instance.GetCulture(cultureName);
        if (culture is null)
            return;

        var prioritizedCultureNames = CultureManager.Instance.GetPrioritizedCultureNames(cultureName);
        LoadLocalizationResourcesForPrioritizedCultures(
            availableTextSources,
            CollectionsMarshal.AsSpan(prioritizedCultureNames),
            loadFlags
        );
    }

    [CreateSyncVersion]
    private Task LoadLocalizationResourcesForCultureAsync(
        string cultureName,
        LocalizationLoadFlags loadFlags,
        CancellationToken cancellationToken = default
    )
    {
        var availableTextSources = _localizedTextSources.ToArray();
        return Task.Run(
            () => LoadLocalizationResourcesForCulture(availableTextSources, cultureName, loadFlags),
            cancellationToken
        );
    }

    private void LoadLocalizationResourcesForPrioritizedCultures(
        ReadOnlySpan<ILocalizedTextSource> availableTextSources,
        ReadOnlySpan<string> prioritizedCultureNames,
        LocalizationLoadFlags loadFlags
    )
    {
        if (prioritizedCultureNames.Length == 0)
            return;

        var finalLocLoadFlags =
            loadFlags
            | (ShouldForceLoadGameLocalization ? LocalizationLoadFlags.ForceLocalizedGame : LocalizationLoadFlags.None);

        var textLocalizationResource = new TextLocalizationResource();
        foreach (var localizedTextSource in availableTextSources)
        {
            localizedTextSource.LoadLocalizedResources(
                finalLocLoadFlags,
                prioritizedCultureNames,
                textLocalizationResource,
                textLocalizationResource
            );
        }

        UpdateLiveTable(
            textLocalizationResource,
            replaceExisting: !finalLocLoadFlags.HasFlag(LocalizationLoadFlags.SkipExisting)
        );
    }

    private Task LoadLocalizationResourcesForPrioritizedCulturesAsync(
        ReadOnlyMemory<string> prioritizedCultureNames,
        LocalizationLoadFlags loadFlags
    )
    {
        var availableTextSources = _localizedTextSources.ToArray();
        return QueueAsyncTask(() =>
            LoadLocalizationResourcesForPrioritizedCultures(
                availableTextSources,
                prioritizedCultureNames.Span,
                loadFlags
            )
        );
    }

    private void LoadLocalizationResourcesForPrioritizedCultures(
        ReadOnlySpan<ILocalizedTextSource> availableTextSources,
        ReadOnlySpan<string> localizationTargetPaths,
        ReadOnlySpan<string> prioritizedCultureNames,
        LocalizationLoadFlags loadFlags
    )
    {
        if (prioritizedCultureNames.Length == 0 || localizationTargetPaths.Length == 0)
            return;

        var textLocalizationResource = new TextLocalizationResource();
        _locResTextSource.LoadLocalizedResourcesFromPaths(
            localizationTargetPaths,
            localizationTargetPaths,
            [],
            loadFlags,
            prioritizedCultureNames,
            textLocalizationResource,
            textLocalizationResource
        );
        var needsFullRefresh = false;

        foreach (var localizedTextSource in availableTextSources)
        {
            if (localizedTextSource.Priority <= ((ILocalizedTextSource)_locResTextSource).Priority)
            {
                continue;
            }

            foreach (var textId in textLocalizationResource.Entries.Keys)
            {
                if (
                    localizedTextSource.QueryLocalizedResource(
                        loadFlags,
                        prioritizedCultureNames,
                        textId,
                        textLocalizationResource,
                        textLocalizationResource
                    ) == QueryLocalizedResourceResult.NotImplemented
                )
                {
                    needsFullRefresh = true;
                    break;
                }
            }

            if (needsFullRefresh)
                break;
        }

        if (needsFullRefresh)
        {
            LoadLocalizationResourcesForPrioritizedCultures(availableTextSources, prioritizedCultureNames, loadFlags);
        }
        else
        {
            UpdateLiveTable(
                textLocalizationResource,
                replaceExisting: !loadFlags.HasFlag(LocalizationLoadFlags.SkipExisting)
            );
        }
    }

    private Task LoadLocalizationResourcesForPrioritizedCulturesAsync(
        ReadOnlyMemory<string> prioritizedCultureName,
        ReadOnlyMemory<string> localizationTargetPaths,
        LocalizationLoadFlags loadFlags
    )
    {
        var availableTextSources = _localizedTextSources.ToArray();
        return QueueAsyncTask(() =>
            LoadLocalizationResourcesForPrioritizedCultures(
                availableTextSources,
                prioritizedCultureName.Span,
                localizationTargetPaths.Span,
                loadFlags
            )
        );
    }

    private void LoadLocalizationTargetsForPrioritizedCultures(
        ReadOnlySpan<ILocalizedTextSource> availableTextSources,
        ReadOnlySpan<string> localizationTargetPaths,
        ReadOnlySpan<string> prioritizedCultureNames,
        LocalizationLoadFlags loadFlags
    )
    {
        if (prioritizedCultureNames.Length == 0 || localizationTargetPaths.Length == 0)
            return;

        var textLocalizationResource = new TextLocalizationResource();
        _locResTextSource.LoadLocalizedResourcesFromPaths(
            localizationTargetPaths,
            localizationTargetPaths,
            [],
            loadFlags,
            prioritizedCultureNames,
            textLocalizationResource,
            textLocalizationResource
        );

        var needsFullRefresh = false;
        foreach (var localizedTextSource in availableTextSources)
        {
            if (localizedTextSource.Priority <= ((ILocalizedTextSource)_locResTextSource).Priority)
            {
                continue;
            }

            foreach (var textId in textLocalizationResource.Entries.Keys)
            {
                if (
                    localizedTextSource.QueryLocalizedResource(
                        loadFlags,
                        prioritizedCultureNames,
                        textId,
                        textLocalizationResource,
                        textLocalizationResource
                    ) != QueryLocalizedResourceResult.NotImplemented
                )
                    continue;
                needsFullRefresh = true;
                break;
            }

            if (needsFullRefresh)
                break;
        }

        if (needsFullRefresh)
        {
            LoadLocalizationResourcesForPrioritizedCultures(availableTextSources, prioritizedCultureNames, loadFlags);
        }
        else
        {
            UpdateLiveTable(
                textLocalizationResource,
                replaceExisting: !loadFlags.HasFlag(LocalizationLoadFlags.SkipExisting)
            );
        }
    }

    private Task LoadLocalizationTargetsForPrioritizedCulturesAsync(
        ReadOnlyMemory<string> prioritizedCultureName,
        ReadOnlyMemory<string> localizationTargetPaths,
        LocalizationLoadFlags loadFlags
    )
    {
        var availableTextSources = _localizedTextSources.ToArray();
        return QueueAsyncTask(() =>
            LoadLocalizationTargetsForPrioritizedCultures(
                availableTextSources,
                localizationTargetPaths.Span,
                prioritizedCultureName.Span,
                loadFlags
            )
        );
    }

    public Task QueueAsyncTask(Action task)
    {
        return _asyncLocalizationTaskQueue.Enqueue(task);
    }

    public Task QueueAsyncTask(Func<Task> task)
    {
        return _asyncLocalizationTaskQueue.Enqueue(task);
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
                if (!_displayTable.TryGetValue(textId, out var liveEntry))
                {
                    var newLiveEntry = new DisplayStringEntry
                    {
                        SourceStringHash = newEntry.SourceStringHash,
                        LocalizationTargetPathId = newEntry.LocalizationTargetPathId,
                        DisplayString = newEntry.LocalizedString,
                        LocResId = newEntry.LocResId,
                    };

                    _displayTable[textId] = newLiveEntry;
                    _displayStringsByLocalizationTargetId.TrackTextId(
                        -1,
                        newLiveEntry.LocalizationTargetPathId,
                        textId
                    );
                }
                else if (replaceExisting)
                {
                    liveEntry.SourceStringHash = newEntry.SourceStringHash;
                    liveEntry.DisplayString = newEntry.LocalizedString;
                    liveEntry.LocResId = newEntry.LocResId;

                    _displayStringsByLocalizationTargetId.TrackTextId(
                        liveEntry.LocalizationTargetPathId,
                        newEntry.LocalizationTargetPathId,
                        textId
                    );
                    liveEntry.LocalizationTargetPathId = newEntry.LocalizationTargetPathId;
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
        if (!IsInitialized)
        {
            return null;
        }

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

        OnTextRevsionChanged?.Invoke();
    }
}
