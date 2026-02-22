// // @file LocalizationManager.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Globalization;
using RetroEngine.Portable.Concurrency;
using RetroEngine.Portable.FileResources;
using RetroEngine.Portable.Localization.Cultures;
using Serilog;

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

public sealed class LocalizationManager
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

    bool IsInitialized => _initializedFlags != LocalizationManagerInitializedFlags.None;

    private readonly ReaderWriterLockSlim _displayTableLock = new();
    private readonly Dictionary<TextId, DisplayStringEntry> _displayTable = new();
    private readonly DisplayStringsByLocalizationTargetId _displayStringsByLocalizationTargetId = new();

    private readonly ReaderWriterLockSlim _textRevisionLock = new();
    private readonly Dictionary<TextId, ushort> _textRevisions = new();
    private ushort _textRevisionCounter = 1;

    private readonly List<ILocalizedTextSource> _localizedTextSources = [];
    private readonly LocalizationResourceTextSource _locResTextSource = new();
    private readonly PolyglotTextSource _polyglotTextSource = new();

    private LocalizationManager()
    {
        const bool refreshResources = false;
        RegisterTextSource(_locResTextSource, refreshResources);
        RegisterTextSource(_polyglotTextSource, refreshResources);
    }

    public static LocalizationManager Instance { get; } = new();

    public void AddOrUpdateDisplayStringInLiveTable(
        string ns,
        string key,
        string displayString,
        string? sourceString = null
    )
    {
        var textId = new TextId(ns, key);

        using var scope = _displayTableLock.EnterWriteScope();

        if (_displayTable.TryGetValue(textId, out var liveEntry))
        {
            liveEntry.DisplayString = displayString;
            DirtyLocalRevisionForTextId(textId);

            Log.Information(
                "Updated string for Namespace='{Namespace}', Key='{Key}' to DisplayString='{DisplayString}'",
                ns,
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
                ns,
                key,
                displayString
            );
        }
        else
        {
            Log.Information("No source string found for Namespace='{Namespace}', Key='{Key}'", ns, key);
        }
    }

    public string RequestLanguageName =>
        GetRequestedCulture("LANGUAGE=", "Language", Culture.DefaultCulture.Name, out _);

    public string RequiredLocaleName => GetRequestedCulture("LOCALE=", "Locale", Culture.DefaultCulture.Name, out _);

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

    public IEnumerable<string> GetLocalizedCultureNames(LocalizedTextSourceCategory category)
    {
        throw new NotImplementedException();
    }

    public int GetLocalizationTargetPathId(string localizationTargetPath)
    {
        throw new NotImplementedException();
    }

    public void RegisterTextSource(ILocalizedTextSource textSource, bool refreshResources = true)
    {
        throw new NotImplementedException();
    }

    public void RegisterPolyglotTextData(PolyglotTextData polyglotTextData, bool addDisplayString = true)
    {
        throw new NotImplementedException();
    }

    public void RegisterPolyglotTextData(ReadOnlySpan<PolyglotTextData> polyglotTextData, bool addDisplayString = true)
    {
        throw new NotImplementedException();
    }

    public string? FindDisplayString(TextKey ns, TextKey key, string? sourceString = null)
    {
        throw new NotImplementedException();
    }

    public string? GetDisplayString(TextKey ns, TextKey key, string? sourceString)
    {
        throw new NotImplementedException();
    }

    public string? GetLocResId(TextKey ns, TextKey key)
    {
        throw new NotImplementedException();
    }

    public void UpdateFromLocalizedResource(string localizationResourceFilePath)
    {
        throw new NotImplementedException();
    }

    public void UpdateFormLocalizationResource(TextLocalizationResource localizationResource)
    {
        throw new NotImplementedException();
    }

    public Task WaitForTasksAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task RefreshResourcesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task HandleLocalizationTargetsMounted(
        ReadOnlyMemory<string> localizationTargetPaths,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public Task HandleLocalizationTargetsUnmounted(
        ReadOnlyMemory<string> localizationTargetPaths,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public ushort TextRevision => throw new NotImplementedException();

    public ushort GetLocalRevisionForTextId(TextId textId)
    {
        throw new NotImplementedException();
    }

    public TextRevisions GetTextRevisions(TextId textId)
    {
        throw new NotImplementedException();
    }

    public void EnableGameLocalizationPreview()
    {
        throw new NotImplementedException();
    }

    public void EnableGameLocalizationPreview(string cultureName)
    {
        throw new NotImplementedException();
    }

    public void DisableGameLocalizationPreview()
    {
        throw new NotImplementedException();
    }

    public bool IsGameLocalizationPreviewEnabled => throw new NotImplementedException();

    public void PushAutoEnableGameLocalizationPreview()
    {
        throw new NotImplementedException();
    }

    public void PopAutoEnableGameLocalizationPreview()
    {
        throw new NotImplementedException();
    }

    public bool ShouldGameLocalizationPreviewAutoEnable => throw new NotImplementedException();

    public void ConfigureGameLocalizationPreviewLanguage(string cultureName)
    {
        throw new NotImplementedException();
    }

    public string ConfiguredGameLocalizationPreviewLanguage => throw new NotImplementedException();

    public bool IsLocalizationLocked => throw new NotImplementedException();

    public bool ShouldForceLoadGameLocalization => throw new NotImplementedException();

    public event Action? OnTextRevsionChanged;

    private void OnPakFileMounted(IPakFile pakFile) { }

    public void OnCultureChanged()
    {
        throw new NotImplementedException();
    }

    private void LoadLocalizationResourcesForCulture(
        ReadOnlySpan<ILocalizedTextSource> availableTextSources,
        string cultureName,
        LocalizationLoadFlags loadFlags
    )
    {
        throw new NotImplementedException();
    }

    private Task LoadLocalizationResourcesForCultureAsync(
        string cultureName,
        LocalizationLoadFlags loadFlags,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    private void LoadLocalizationResourcesForPrioritizedCultures(
        ReadOnlySpan<ILocalizedTextSource> availableTextSources,
        ReadOnlySpan<string> prioritizedCultureName,
        LocalizationLoadFlags loadFlags
    )
    {
        throw new NotImplementedException();
    }

    private Task LoadLocalizationResourcesForPrioritizedCulturesAsync(
        ReadOnlyMemory<string> prioritizedCultureName,
        LocalizationLoadFlags loadFlags,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    private void LoadLocalizationResourcesForPrioritizedCultures(
        ReadOnlySpan<ILocalizedTextSource> availableTextSources,
        ReadOnlySpan<string> localizationTargetPaths,
        ReadOnlySpan<string> prioritizedCultureName,
        LocalizationLoadFlags loadFlags
    )
    {
        throw new NotImplementedException();
    }

    private Task LoadLocalizationResourcesForPrioritizedCulturesAsync(
        ReadOnlyMemory<string> prioritizedCultureName,
        ReadOnlyMemory<string> localizationTargetPaths,
        LocalizationLoadFlags loadFlags,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public void LoadChunkedLocalizationResources(
        ReadOnlySpan<ILocalizedTextSource> availableTextSources,
        int chunkId,
        string pakFilename
    )
    {
        throw new NotImplementedException();
    }

    public Task LoadChunkedLocalizationResourcesAsync(
        int chunkId,
        string pakFilename,
        CancellationToken cancellationToken = default
    )
    {
        throw new NotImplementedException();
    }

    public void QueueAsyncTask(Action task)
    {
        throw new NotImplementedException();
    }

    public void QueueAsyncTask(Func<Task> task)
    {
        throw new NotImplementedException();
    }

    private void UpdateLiveTable(
        TextLocalizationResource textLocalizationResource,
        bool dirtyTextRevision = true,
        bool replaceExisting = true
    )
    {
        throw new NotImplementedException();
    }

    private void DirtyLocalRevisionForTextId(TextId textId)
    {
        throw new NotImplementedException();
    }

    private string? FindDisplayStringInternal(TextId textId, string? sourceString)
    {
        throw new NotImplementedException();
    }

    private void DirtyTextRevision()
    {
        throw new NotImplementedException();
    }
}
