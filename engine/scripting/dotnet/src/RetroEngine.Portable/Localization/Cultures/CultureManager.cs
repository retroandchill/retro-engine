// // @file CultureManager.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using RetroEngine.Portable.Strings;
using RetroEngine.Portable.Utils;

namespace RetroEngine.Portable.Localization.Cultures;

public sealed class CultureManager
{
    internal readonly record struct IcuCultureData(
        string Name,
        string LanguageCode,
        string ScriptCode,
        string CountryCode
    )
    {
        public override string ToString() => Name;

        public bool Equals(IcuCultureData other)
        {
            return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode() => Name.GetHashCode();
    }

    public event Action? OnCultureChanged;

    public Culture CurrentLanguage { get; private set; }
    public Culture CurrentLocale { get; private set; }

    private readonly List<(Name Name, Culture Culture)> _currentAssetGroupCultures = [];

    public Culture DefaultCulture => DefaultLanguage;
    public Culture DefaultLanguage { get; }
    public Culture DefaultLocale { get; }
    public Culture InvariantCulture { get; }
    private readonly List<Culture> _customCultures = [];

    private readonly List<IcuCultureData> _availableCultures = [];
    private readonly Dictionary<string, int> _availableCulturesMap = new();
    private readonly Dictionary<string, List<int>> _availableLanguagesToSubCulturesMap = new();

    private bool _cultureMappingsInitialized;
    private readonly Dictionary<string, string> _cultureMappings = new();

    [MemberNotNullWhen(true, nameof(_allowedCulturesFilter))]
    private bool AllowedCulturesInitialized => _allowedCulturesFilter is not null;
    private CultureFilter? _allowedCulturesFilter;

    private readonly Lock _cachedCulturesLock = new();
    private readonly Dictionary<string, Culture> _cachedCultures = new();

    private readonly Lock _invariantGregorianCalendarLock = new();
    private readonly Calendar? _invariantGregorianCalendar;

    private readonly List<string> _cachedPrioritizedDisplayCultureNames = [];

    public readonly record struct CultureStateSnapshot(
        string Language,
        string Locale,
        ImmutableArray<(Name Name, string Culture)> AssetGroups
    );

    public Culture CurrentCulture => CurrentLanguage;

    private CultureManager()
    {
        InitializeAvailableCultures();

        _cultureMappingsInitialized = false;
        ConditionalInitializeCultureMappings();

        ConditionalIntializeAllowedCultures();

        InvariantCulture = FindCanonizedCulture("en-US-POSIX") ?? FindOrMakeCanonizedCulture("");

        DefaultLanguage = FindOrMakeCulture(CultureInfo.DefaultThreadCurrentCulture?.Name ?? "en");
        var defaultLocaleName = CultureInfo.InstalledUICulture.Name;
        DefaultLocale = !string.IsNullOrEmpty(defaultLocaleName)
            ? FindOrMakeCulture(defaultLocaleName)
            : DefaultLanguage;
        CurrentLanguage = DefaultLanguage;
        CurrentLocale = DefaultLocale;

        HandleLanguageChanged(DefaultLanguage);

        // We just need to pull in the TimeZone class to force the static constructor to run.
        _ = TimeZone.Local;
        _invariantGregorianCalendar = Calendar.Create();
        _invariantGregorianCalendar?.TimeZone = TimeZone.Unknown;
    }

    public static CultureManager Instance { get; } = new();

    public bool SetCurrentCulture(string cultureName)
    {
        var newCulture = GetCulture(cultureName);
        if (
            newCulture is null
            || CurrentLanguage == newCulture && CurrentLocale == newCulture && _currentAssetGroupCultures.Count <= 0
        )
            return CurrentLanguage == newCulture
                && CurrentLocale == newCulture
                && _currentAssetGroupCultures.Count == 0;

        CurrentLanguage = newCulture;
        CurrentLocale = newCulture;
        HandleLanguageChanged(newCulture);
        OnCultureChanged?.Invoke();
        return true;
    }

    public bool SetCurrentLanguage(string languageName)
    {
        var newCulture = GetCulture(languageName);

        if (newCulture is null)
            return false;

        if (CurrentLanguage == newCulture)
            return true;

        CurrentLanguage = newCulture;
        HandleLanguageChanged(newCulture);
        OnCultureChanged?.Invoke();
        return true;
    }

    public bool SetCurrentLocale(string localeName)
    {
        var newCulture = GetCulture(localeName);

        if (newCulture is null)
            return false;

        if (CurrentLocale == newCulture)
            return true;

        CurrentLocale = newCulture;
        OnCultureChanged?.Invoke();
        return true;
    }

    public bool SetCurrentLanguageAndLocale(string cultureName)
    {
        var newCulture = GetCulture(cultureName);

        if (newCulture is null)
            return false;

        if (CurrentLanguage == newCulture && CurrentLocale == newCulture)
            return true;

        CurrentLanguage = newCulture;
        CurrentLocale = newCulture;
        HandleLanguageChanged(newCulture);
        OnCultureChanged?.Invoke();
        return true;
    }

    public bool SetCurrentAssetGroupCulture(Name assetGroupName, string cultureName)
    {
        var newCulture = GetCulture(cultureName);

        if (newCulture is null)
            return false;

        var entryToUpdate = _currentAssetGroupCultures.Index().FirstOrNull(x => x.Item.Name == assetGroupName);

        var changedCulture = false;
        if (entryToUpdate is not null)
        {
            if (entryToUpdate.Value.Item.Culture != newCulture)
            {
                _currentAssetGroupCultures[entryToUpdate.Value.Index] = entryToUpdate.Value.Item with
                {
                    Culture = newCulture,
                };
                changedCulture = true;
            }
        }
        else
        {
            changedCulture = true;
            _currentAssetGroupCultures.Add((assetGroupName, newCulture));
        }

        if (changedCulture)
            OnCultureChanged?.Invoke();
        return changedCulture;
    }

    public Culture GetCurrentAssetGroupCulture(Name assetGroupName)
    {
        foreach (var (name, culture) in _currentAssetGroupCultures)
        {
            if (name == assetGroupName)
                return culture;
        }

        return CurrentLanguage;
    }

    public void ClearCurrentAssetGroupCulture(Name assetGroupName)
    {
        _currentAssetGroupCultures.RemoveAll(x => x.Name == assetGroupName);
    }

    public Culture? GetCulture(string cultureName)
    {
        return FindCulture(cultureName);
    }

    public IEnumerable<Culture> GetCurrentCultures(bool includeLanguage, bool includeLocale, bool includeAssetGroups)
    {
        return GetCurrentCulturesImpl(includeLanguage, includeLocale, includeAssetGroups).Distinct();
    }

    private IEnumerable<Culture> GetCurrentCulturesImpl(
        bool includeLanguage,
        bool includeLocale,
        bool includeAssetGroups
    )
    {
        if (includeLanguage)
            yield return CurrentLanguage;
        if (includeLocale)
            yield return CurrentLocale;
        if (!includeAssetGroups)
            yield break;

        foreach (var (_, culture) in _currentAssetGroupCultures)
        {
            yield return culture;
        }
        ;
    }

    public CultureStateSnapshot BackupCultureState()
    {
        return new CultureStateSnapshot(
            CurrentLanguage.Name,
            CurrentLocale.Name,
            [.. _currentAssetGroupCultures.Select(x => (x.Name, x.Culture.Name))]
        );
    }

    public void RestoreCultureState(CultureStateSnapshot cultureStateSnapshot)
    {
        var changedCulture = false;

        if (!string.IsNullOrEmpty(cultureStateSnapshot.Language))
        {
            var newCulture = GetCulture(cultureStateSnapshot.Language);
            if (newCulture is not null && CurrentLanguage != newCulture)
            {
                CurrentLanguage = newCulture;
                changedCulture = true;
            }
        }

        if (!string.IsNullOrEmpty(cultureStateSnapshot.Locale))
        {
            var newCulture = GetCulture(cultureStateSnapshot.Locale);
            if (newCulture is not null && CurrentLocale != newCulture)
            {
                CurrentLocale = newCulture;
            }
        }

        changedCulture |= _currentAssetGroupCultures.Count > 0;
        _currentAssetGroupCultures.Clear();
        foreach (var (name, culture) in cultureStateSnapshot.AssetGroups)
        {
            var newCulture = GetCulture(culture);
            if (newCulture is not null)
            {
                _currentAssetGroupCultures.Add((name, newCulture));
            }
        }

        if (changedCulture)
            OnCultureChanged?.Invoke();
    }

    public void LoadAllCultureData()
    {
        foreach (var cultureData in _availableCultures)
        {
            FindCulture(cultureData.Name);
        }
    }

    public void AddCustomCulture(ICustomCulture customCulture)
    {
        _customCultures.Add(new Culture(new CustomLocale(customCulture)));
    }

    public Culture? GetCustomCulture(string cultureName)
    {
        return _customCultures.FirstOrDefault(x => x.Name == cultureName);
    }

    public bool IsCultureRemapped(string name, [NotNullWhen(true)] out string? mappedCulture)
    {
        ConditionalInitializeCultureMappings();
        return _cultureMappings.TryGetValue(name, out mappedCulture);
    }

    public bool IsCultureAllowed(string name)
    {
        ConditionalIntializeAllowedCultures();
        return _allowedCulturesFilter.IsCultureAllowed(name);
    }

    public void RefreshCultureDisplayNames(ReadOnlySpan<string> prioritizedDisplayCultureNames)
    {
        using var scope = _cachedCulturesLock.EnterScope();
        foreach (var (_, culture) in _cachedCultures)
        {
            culture.RefreshCultureDisplayNames(prioritizedDisplayCultureNames);
        }
    }

    public void RefreshCachedConfigData()
    {
        _cultureMappingsInitialized = false;
        _cultureMappings.Clear();
        ConditionalInitializeCultureMappings();

        _allowedCulturesFilter = null;
        ConditionalIntializeAllowedCultures();
    }

    public IEnumerable<string> CultureNames
    {
        get { return _availableCultures.Select(x => x.Name).Concat(_customCultures.Select(x => x.Name)); }
    }

    public List<string> GetPrioritizedCultureNames(string name)
    {
        var givenCulture = IsCultureRemapped(name, out var mappedCulture)
            ? mappedCulture
            : Culture.GetCanonicalName(name, this);

        var prioritizedCultureNames = new List<string>();

        var givenCultureData = PopulateCultureData(givenCulture);
        if (givenCultureData is not null)
        {
            var parentCultureData = new List<IcuCultureData>();
            if (
                string.IsNullOrEmpty(givenCultureData.Value.ScriptCode)
                && !string.IsNullOrEmpty(givenCultureData.Value.CountryCode)
            )
            {
                if (
                    _availableLanguagesToSubCulturesMap.TryGetValue(
                        givenCultureData.Value.LanguageCode,
                        out var culturesForLanguage
                    )
                )
                {
                    parentCultureData.AddRange(
                        culturesForLanguage
                            .Select(cultureIndex => _availableCultures[cultureIndex])
                            .Where(cultureData =>
                                !string.IsNullOrEmpty(cultureData.ScriptCode)
                                && cultureData.CountryCode == givenCultureData.Value.CountryCode
                            )
                    );
                }
            }

            if (parentCultureData.Count == 0)
            {
                parentCultureData.Add(givenCultureData.Value);
            }

            var prioritizedCultureData = new List<IcuCultureData>(parentCultureData.Count + 3);
            foreach (var cultureData in parentCultureData)
            {
                var prioritizedParentCultures = Culture.GetPrioritizedParentCultureNames(
                    cultureData.LanguageCode,
                    cultureData.ScriptCode,
                    cultureData.CountryCode
                );
                foreach (var prioritizedParentCulture in prioritizedParentCultures)
                {
                    var prioritizedParentCultureData = PopulateCultureData(prioritizedParentCulture);
                    if (
                        prioritizedParentCultureData is not null
                        && !prioritizedCultureData.Contains(prioritizedParentCultureData.Value)
                    )
                    {
                        prioritizedCultureData.Add(prioritizedParentCultureData.Value);
                    }
                }
            }

            var preferTraditionalChinese = givenCultureData.Value.CountryCode is "HK" or "MO";
            prioritizedCultureData.Sort(
                (a, b) =>
                {
                    var dataOneSortWeight =
                        (string.IsNullOrEmpty(a.CountryCode) ? 0 : 4)
                        + (string.IsNullOrEmpty(a.ScriptCode) ? 0 : 2)
                        + (preferTraditionalChinese && a.ScriptCode == "Hant" ? 1 : 0);
                    var dataTwoSortWeight =
                        (string.IsNullOrEmpty(b.CountryCode) ? 0 : 4)
                        + (string.IsNullOrEmpty(b.ScriptCode) ? 0 : 2)
                        + (preferTraditionalChinese && b.ScriptCode == "Hant" ? 1 : 0);
                    return dataOneSortWeight >= dataTwoSortWeight ? 1 : -1;
                }
            );

            prioritizedCultureNames.EnsureCapacity(prioritizedCultureData.Count);
            prioritizedCultureNames.AddRange(
                prioritizedCultureData
                    .Where(cultureData => IsCultureAllowed(cultureData.Name))
                    .Select(cultureData => cultureData.Name)
            );
        }

        if (prioritizedCultureNames.Count == 0)
        {
            prioritizedCultureNames.Add("en");
        }

        return prioritizedCultureNames;

        IcuCultureData? PopulateCultureData(string cultureName)
        {
            if (_availableCulturesMap.TryGetValue(cultureName, out var cultureDataIndex))
            {
                return _availableCultures[cultureDataIndex];
            }

            var culture = FindCanonizedCulture(cultureName);
            if (culture is not null)
            {
                return new IcuCultureData(
                    culture.Name,
                    culture.TwoLetterISOLanguageName,
                    culture.Script,
                    culture.Region
                );
            }

            return null;
        }
    }

    public IEnumerable<string> GetAvailableCultures(ICollection<string> cultureNames, bool includeDerivedCultures)
    {
        if (includeDerivedCultures)
        {
            return CultureNames
                .Select(GetCulture)
                .OfType<Culture>()
                .SelectMany(c => c.PrioritizedParentCultureNames.Where(cultureNames.Contains))
                .Distinct();
        }

        return cultureNames.Where(c => GetCulture(c) is not null).Distinct();
    }

    public void HandleLanguageChanged(Culture newLanguage)
    {
        Locale.SetDefaultLocale(newLanguage.Name);

        _cachedPrioritizedDisplayCultureNames.Clear();
        _cachedPrioritizedDisplayCultureNames.AddRange(newLanguage.PrioritizedParentCultureNames);

        using var scope = _cachedCulturesLock.EnterScope();
        foreach (var (_, culture) in _cachedCultures)
        {
            culture.RefreshCultureDisplayNames(CollectionsMarshal.AsSpan(_cachedPrioritizedDisplayCultureNames));
        }
    }

    public double DateTimeOffsetToIcuDate(DateTimeOffset dateTimeOffset)
    {
        const int millisPerSecond = 1000;
        if (_invariantGregorianCalendar is null)
        {
            var unixTimestamp = dateTimeOffset.ToUnixTimeMilliseconds();
            return (double)unixTimestamp / millisPerSecond;
        }

        var utcTime = dateTimeOffset.UtcDateTime;
        using var scope = _invariantGregorianCalendarLock.EnterScope();
        _invariantGregorianCalendar.Set(
            utcTime.Year,
            utcTime.Month - 1,
            utcTime.Day,
            utcTime.Hour,
            utcTime.Minute,
            utcTime.Second
        );
        return _invariantGregorianCalendar.Time;
    }

    private void InitializeAvailableCultures()
    {
        var availableLocales = Locale.GetAvailableLocales();

        _availableCultures.EnsureCapacity(availableLocales.Length);
        _availableCulturesMap.EnsureCapacity(availableLocales.Length);
        _availableLanguagesToSubCulturesMap.EnsureCapacity(availableLocales.Length);

        foreach (var locale in availableLocales)
        {
            var language = locale.TwoLetterISOLanguageName;
            var script = locale.Script;
            var country = locale.Region;

            AppendCultureData(script, language, "", "");
            if (!string.IsNullOrEmpty(country))
            {
                AppendCultureData(Culture.CreateCultureName(language, "", country), language, "", country);
            }

            if (!string.IsNullOrEmpty(script))
            {
                AppendCultureData(Culture.CreateCultureName(language, script, ""), language, script, "");
            }

            if (!string.IsNullOrEmpty(country) && !string.IsNullOrEmpty(script))
            {
                AppendCultureData(Culture.CreateCultureName(language, script, country), language, script, country);
            }
        }

        foreach (var language in Locale.GetAvailableLanguageIds().Select(x => x.ToLowerInvariant()))
        {
            AppendCultureData(language, language, "", "");
        }

        AppendCultureData("en-US-POSIX", "en", "", "US-POSIX");
        return;

        void AppendCultureData(string cultureName, string languageCode, string scriptCode, string countryCode)
        {
            if (_availableCulturesMap.ContainsKey(cultureName))
                return;

            var cultureDataIndex = _availableCultures.Count;
            _availableCulturesMap.Add(cultureName, cultureDataIndex);

            if (!_availableLanguagesToSubCulturesMap.TryGetValue(languageCode, out var culturesForLanguage))
            {
                culturesForLanguage = [];
                _availableLanguagesToSubCulturesMap.Add(languageCode, culturesForLanguage);
            }

            culturesForLanguage.Add(cultureDataIndex);

            var cultureData = new IcuCultureData(cultureName, languageCode, scriptCode, countryCode);
            _availableCultures.Add(cultureData);
        }
    }

    private void ConditionalInitializeCultureMappings()
    {
        // TODO: Once configs are in place, we need to go back and set this up
        if (_cultureMappingsInitialized)
        {
            return;
        }

        _cultureMappingsInitialized = true;
    }

    [MemberNotNull(nameof(_allowedCulturesFilter))]
    private void ConditionalIntializeAllowedCultures()
    {
        if (AllowedCulturesInitialized)
        {
            return;
        }

        _allowedCulturesFilter = new CultureFilter(_availableCulturesMap.Keys.ToHashSet());
    }

    private Culture? FindCulture(string name)
    {
        return FindCanonizedCulture(Culture.GetCanonicalName(name, this));
    }

    private Culture FindOrMakeCulture(string name)
    {
        return FindOrMakeCanonizedCulture(Culture.GetCanonicalName(name, this));
    }

    private Culture? FindCanonizedCulture(string name)
    {
        using (_cachedCulturesLock.EnterScope())
        {
            if (_cachedCultures.TryGetValue(name, out var culture))
            {
                return culture;
            }
        }

        Culture? newCulture;

        var customCulture = GetCustomCulture(name);
        if (customCulture is not null)
        {
            newCulture = customCulture;
        }
        else if (_availableCulturesMap.ContainsKey(name))
        {
            newCulture = new Culture(name);
        }
        else
        {
            // TODO: We should load from resources but that is not currently set up
            newCulture = null;
        }

        if (newCulture is null)
            return newCulture;

        newCulture.RefreshCultureDisplayNames(CollectionsMarshal.AsSpan(_cachedPrioritizedDisplayCultureNames), false);

        using (_cachedCulturesLock.EnterScope())
        {
            _cachedCultures[name] = newCulture;
        }

        return newCulture;
    }

    private Culture FindOrMakeCanonizedCulture(string name)
    {
        return FindCanonizedCulture(name) ?? new Culture(name);
    }
}
