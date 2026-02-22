// // @file PolyglotTextSource.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Localization.Cultures;
using ZLinq;

namespace RetroEngine.Portable.Localization;

public sealed class PolyglotTextSource : ILocalizedTextSource
{
    private readonly record struct CultureInfo(
        Dictionary<string, int> NativeCultures,
        Dictionary<string, int> LocalizedCultures
    );

    private readonly Dictionary<LocalizedTextSourceCategory, CultureInfo> _availableCultureInfo = new();
    private readonly Dictionary<TextId, PolyglotTextData> _polyglotTextData = new();
    private readonly Lock _polyglotDataLock = new();

    public int Priority => LocalizedTextSourcePriority.Highest;

    public string? GetNativeCultureName(LocalizedTextSourceCategory category)
    {
        using var scope = _polyglotDataLock.EnterScope();
        return _availableCultureInfo.TryGetValue(category, out var cultureInfo)
            ? cultureInfo.NativeCultures.FirstOrDefault().Key
            : null;
    }

    public void GetLocalizedCultureNames(LocalizationLoadFlags flags, ISet<string> localizedCultureNames)
    {
        using var scope = _polyglotDataLock.EnterScope();
        if (flags.HasFlag(LocalizationLoadFlags.Editor))
        {
            AppendCulturesFormCategory(LocalizedTextSourceCategory.Editor);
        }

        if (flags.HasFlag(LocalizationLoadFlags.Game))
        {
            AppendCulturesFormCategory(LocalizedTextSourceCategory.Game);
        }

        if (flags.HasFlag(LocalizationLoadFlags.Engine))
        {
            AppendCulturesFormCategory(LocalizedTextSourceCategory.Engine);
        }

        return;

        void AppendCulturesFormCategory(LocalizedTextSourceCategory category)
        {
            if (!_availableCultureInfo.TryGetValue(category, out var cultureInfo))
                return;
            localizedCultureNames.UnionWith(cultureInfo.LocalizedCultures.Keys);
            localizedCultureNames.UnionWith(cultureInfo.NativeCultures.Keys);
        }
    }

    public void LoadLocalizedResources(
        LocalizationLoadFlags loadFlags,
        ReadOnlySpan<string> prioritizedCultures,
        TextLocalizationResource nativeResource,
        TextLocalizationResource localizedResource
    )
    {
        using var scope = _polyglotDataLock.EnterScope();
        foreach (var polyglotTextData in _polyglotTextData.Values)
        {
            AddPolyglotDataToResource(
                polyglotTextData,
                loadFlags,
                prioritizedCultures,
                nativeResource,
                localizedResource
            );
        }
    }

    public QueryLocalizedResourceResult QueryLocalizedResource(
        LocalizationLoadFlags loadFlags,
        ReadOnlySpan<string> prioritizedCultures,
        TextId textId,
        TextLocalizationResource nativeResource,
        TextLocalizationResource localizedResource
    )
    {
        using var scope = _polyglotDataLock.EnterScope();

        if (!_polyglotTextData.TryGetValue(textId, out var polyglotTextData))
            return QueryLocalizedResourceResult.NotFound;

        AddPolyglotDataToResource(polyglotTextData, loadFlags, prioritizedCultures, nativeResource, localizedResource);
        return QueryLocalizedResourceResult.Found;
    }

    public string? GetLocalizedString(TextId id, Culture cultureInfo, string fallback = "")
    {
        throw new NotImplementedException();
    }

    public void RegisterPolyglotTextData(PolyglotTextData polyglotTextData)
    {
        using var scope = _polyglotDataLock.EnterScope();

        var identity = new TextId(polyglotTextData.Namespace, polyglotTextData.Key);
        if (_polyglotTextData.TryGetValue(identity, out var existingPolyglotTextData))
        {
            UnregisterCultureNames(existingPolyglotTextData);
            _polyglotTextData[identity] = polyglotTextData;
            RegisterCultureNames(polyglotTextData);
        }
        else
        {
            _polyglotTextData[identity] = polyglotTextData;
            RegisterCultureNames(polyglotTextData);
        }
    }

    private void RegisterCultureNames(PolyglotTextData polyglotTextData)
    {
        if (!_availableCultureInfo.TryGetValue(polyglotTextData.Category, out var cultureInfo))
        {
            cultureInfo = new CultureInfo([], []);
            _availableCultureInfo[polyglotTextData.Category] = cultureInfo;
        }

        IncrementCultureCount(cultureInfo.NativeCultures, polyglotTextData.NativeCulture);

        foreach (var localizedCulture in polyglotTextData.LocalizedCultures)
        {
            IncrementCultureCount(cultureInfo.LocalizedCultures, localizedCulture);
        }

        return;

        void IncrementCultureCount(Dictionary<string, int> culturesMap, string cultureName)
        {
            if (string.IsNullOrEmpty(cultureName))
                return;

            var count = culturesMap.GetValueOrDefault(cultureName, 0);
            culturesMap[cultureName] = count + 1;
        }
    }

    private void UnregisterCultureNames(PolyglotTextData polyglotTextData)
    {
        if (!_availableCultureInfo.TryGetValue(polyglotTextData.Category, out var cultureInfo))
        {
            cultureInfo = new CultureInfo([], []);
            _availableCultureInfo[polyglotTextData.Category] = cultureInfo;
        }

        DecrementCultureCount(cultureInfo.NativeCultures, polyglotTextData.NativeCulture);

        foreach (var localizedCulture in polyglotTextData.LocalizedCultures)
        {
            DecrementCultureCount(cultureInfo.LocalizedCultures, localizedCulture);
        }

        return;

        void DecrementCultureCount(Dictionary<string, int> culturesMap, string cultureName)
        {
            if (string.IsNullOrEmpty(cultureName) || !culturesMap.TryGetValue(cultureName, out var count))
                return;

            if (count == 1)
            {
                culturesMap.Remove(cultureName);
            }
            else
            {
                culturesMap[cultureName] = count - 1;
            }
        }
    }

    private void AddPolyglotDataToResource(
        PolyglotTextData polyglotTextData,
        LocalizationLoadFlags loadFlags,
        ReadOnlySpan<string> prioritizedCultures,
        TextLocalizationResource nativeResource,
        TextLocalizationResource localizedResource
    )
    {
        var baseResourcePriority = Priority * -1;
        var nativeResourcePriority = baseResourcePriority + prioritizedCultures.Length;
        var normalNativeResourcePriority = LocalizedTextSourcePriority.Normal + prioritizedCultures.Length;
        var nativeCulture = polyglotTextData.ResolveNativeCulture();

        if (loadFlags.ShouldLoadNative && !prioritizedCultures.Contains(nativeCulture))
        {
            var result = GetLocalizedStringForPolyglotData([nativeCulture]);
            if (result is not null)
            {
                var (localizedString, _) = result.Value;
                var resolvedNativeResourcePriority = polyglotTextData.IsMinimalPatch
                    ? normalNativeResourcePriority
                    : nativeResourcePriority;
                nativeResource.AddEntry(
                    polyglotTextData.Namespace,
                    polyglotTextData.Key,
                    polyglotTextData.NativeString,
                    localizedString,
                    resolvedNativeResourcePriority
                );
            }
        }

        if (!ShouldLoadLocalizedText())
            return;

        if (polyglotTextData.Category == LocalizedTextSourceCategory.Game && loadFlags.ShouldLoadNativeGameData)
        {
            var result = GetLocalizedStringForPolyglotData([nativeCulture]);
            if (result is null)
                return;

            var (localizedString, _) = result.Value;
            localizedResource.AddEntry(
                polyglotTextData.Namespace,
                polyglotTextData.Key,
                polyglotTextData.NativeString,
                localizedString,
                nativeResourcePriority
            );
        }
        else
        {
            var result = GetLocalizedStringForPolyglotData([nativeCulture]);
            if (result is null)
                return;

            var (localizedString, localizedPriority) = result.Value;
            localizedResource.AddEntry(
                polyglotTextData.Namespace,
                polyglotTextData.Key,
                polyglotTextData.NativeString,
                localizedString,
                baseResourcePriority + localizedPriority
            );
        }

        return;

        bool ShouldLoadLocalizedText()
        {
            return polyglotTextData.Category switch
            {
                LocalizedTextSourceCategory.Game => loadFlags.ShouldLoadGame,
                LocalizedTextSourceCategory.Engine => loadFlags.ShouldLoadEngine,
                LocalizedTextSourceCategory.Editor => loadFlags.ShouldLoadEditor,
                _ => throw new InvalidOperationException("Invalid localized text source category."),
            };
        }

        (string LocalizedString, int Priority)? GetLocalizedStringForPolyglotData(ReadOnlySpan<string> culturesToCheck)
        {
            foreach (var (i, cultureName) in culturesToCheck.AsValueEnumerable().Index())
            {
                var localizedString = polyglotTextData.GetLocalizedString(cultureName);
                if (localizedString is not null)
                {
                    return (localizedString, i);
                }
            }

            if (polyglotTextData.IsMinimalPatch)
            {
                return null;
            }

            return (polyglotTextData.NativeString, 0);
        }
    }
}
