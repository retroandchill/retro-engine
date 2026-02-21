// // @file PolyglotTextData.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization;

public sealed class PolyglotTextData
{
    private const string LocTextNamespace = "PolyglotTextData";

    public LocalizedTextSourceCategory Category
    {
        get;
        set
        {
            ClearCache();
            field = value;
        }
    }
    public string NativeCulture
    {
        get;
        set
        {
            ClearCache();
            field = value;
        }
    } = "";
    public string Namespace { get; private set; } = "";
    public string Key { get; private set; } = "";

    public (string Namespace, string Key) Identity
    {
        get => (Namespace, Key);
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value.Key);
            ClearCache();
            Namespace = value.Namespace;
            Key = value.Key;
        }
    }

    public string NativeString
    {
        get;
        set
        {
            ClearCache();
            field = value;
        }
    } = "";
    private readonly Dictionary<string, string> _localizedStrings = new();
    public bool IsMinimalPatch { get; set; } = false;
    private Text _cachedText = Text.Empty;

    public Text Text
    {
        get
        {
            if (_cachedText.IsEmpty)
            {
                CacheText(out _);
            }

            return _cachedText;
        }
    }

    public PolyglotTextData() { }

    public PolyglotTextData(
        LocalizedTextSourceCategory category,
        string ns,
        string key,
        string nativeString,
        string nativeCulture
    )
    {
        Category = category;
        NativeCulture = nativeCulture;
        Namespace = ns;
        Key = key;
        NativeString = nativeString;
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentException.ThrowIfNullOrEmpty(nativeString);
    }

    public bool IsValid(out Text failureReason)
    {
        if (string.IsNullOrEmpty(Key))
        {
            failureReason = Text.AsLocalizable(
                LocTextNamespace,
                "ValidationError_NoKey",
                "Polyglot data has no key set"
            );
            return false;
        }

        if (string.IsNullOrEmpty(NativeString))
        {
            failureReason = Text.AsLocalizable(
                LocTextNamespace,
                "ValidationError_NoNativeString",
                "Polyglot data has no native string set"
            );
            return false;
        }

        failureReason = Text.Empty;
        return true;
    }

    public string ResolveNativeCulture()
    {
        if (!string.IsNullOrEmpty(NativeCulture))
        {
            return NativeCulture;
        }

        var resolvedNativeCulture = TextLocalizationResourceUtil.GetNativeCultureName(Category);
        return !string.IsNullOrEmpty(resolvedNativeCulture) ? resolvedNativeCulture : "en";
    }

    public IEnumerable<string> LocalizedCultures => _localizedStrings.Keys.OrderBy(x => x);

    public void AddLocalizedString(string culture, string localizedString)
    {
        ArgumentException.ThrowIfNullOrEmpty(culture);
        _localizedStrings.Add(culture, localizedString);
    }

    public void RemoveLocalizedString(string culture)
    {
        ArgumentException.ThrowIfNullOrEmpty(culture);
        _localizedStrings.Remove(culture);
    }

    public string? GetLocalizedString(string culture)
    {
        ArgumentException.ThrowIfNullOrEmpty(culture);
        return _localizedStrings.GetValueOrDefault(culture);
    }

    public void ClearLocalizedStrings()
    {
        _localizedStrings.Clear();
    }

    private void CacheText(out Text failureReason)
    {
        if (IsValid(out failureReason))
        {
            LocalizationManager.Instance.RegisterPolyglotTextData(this);
            if (!Text.FindTextInLiveTable(Namespace, Key, out _cachedText, NativeCulture))
            {
                ClearCache();
            }
        }
        else
        {
            ClearCache();
        }
    }

    private void ClearCache()
    {
        _cachedText = Text.Empty;
    }
}
