// // @file CultureHandle.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Globalization;
using System.Numerics;
using ICU4N.Globalization;
using ICU4N.Text;
using RetroEngine.Portable.Localization.Formatting;

namespace RetroEngine.Portable.Localization;

public sealed class CultureHandle
{
    private static readonly UCultureInfo EnglishCultureInfo = new("en");

    private readonly UCultureInfo _icuCultureInfo;
    public string Name { get; }

    public string NativeName { get; }

    public string DisplayName { get; }

    public string EnglishName { get; }

    public string ThreeLetterIsoLanguageName { get; }

    public string TwoLetterIsoLanguageName { get; }

    public int KeyboardLayoutId { get; }

    public int LCID { get; }

    public string NativeLanguage { get; private set; }

    public string Region { get; }

    public string NativeRegion { get; }

    public string Script { get; }

    public string Variant { get; }

    public bool IsRightToLeft { get; }

    internal CultureHandle(UCultureInfo cultureInfo)
    {
        _icuCultureInfo = cultureInfo;
        Name = _icuCultureInfo.Name;
        DisplayName = _icuCultureInfo.DisplayName;
        NativeName = _icuCultureInfo.NativeName;
        EnglishName = _icuCultureInfo.GetDisplayName(EnglishCultureInfo);
        ThreeLetterIsoLanguageName = _icuCultureInfo.TwoLetterISOLanguageName;
        TwoLetterIsoLanguageName = _icuCultureInfo.TwoLetterISOLanguageName;
        NativeLanguage = _icuCultureInfo.Language;
        Region = _icuCultureInfo.Country;
        Script = _icuCultureInfo.Script;
        Variant = _icuCultureInfo.Variant;
        IsRightToLeft = _icuCultureInfo.IsRightToLeft;
    }

    public IEnumerable<string> GetPrioritizedParentCultureNames()
    {
        return GetPrioritizedParentCultureNames(TwoLetterIsoLanguageName, Script, Region);
    }

    public static IEnumerable<string> GetPrioritizedParentCultureNames(
        string languageCode,
        string scriptCode,
        string regionCode
    )
    {
        if (!string.IsNullOrEmpty(scriptCode) && !string.IsNullOrEmpty(regionCode))
        {
            yield return CreateCultureName(languageCode, scriptCode, regionCode);
        }

        if (!string.IsNullOrEmpty(regionCode))
        {
            yield return CreateCultureName(languageCode, "", regionCode);
        }

        if (!string.IsNullOrEmpty(scriptCode))
        {
            yield return CreateCultureName(languageCode, scriptCode, "");
        }

        yield return languageCode;
    }

    public static string CreateCultureName(string languageCode, string scriptCode, string regionCode)
    {
        if (!string.IsNullOrEmpty(scriptCode) && !string.IsNullOrEmpty(regionCode))
        {
            return $"{languageCode}-{scriptCode}-{regionCode}";
        }

        if (!string.IsNullOrEmpty(regionCode))
        {
            return $"{languageCode}-{regionCode}";
        }

        return !string.IsNullOrEmpty(scriptCode) ? $"{languageCode}-{scriptCode}" : languageCode;
    }

    private static readonly Lock _decimalNumberFormattingRulesLock = new();

    public DecimalNumberFormattingRules DecimalNumberFormattingRules
    {
        get
        {
            if (field is not null)
            {
                return field;
            }

            var numberFormat = _icuCultureInfo.NumberFormat;

            using var scope = _decimalNumberFormattingRulesLock.EnterScope();
            if (field is null)
            {
                field = new DecimalNumberFormattingRules
                {
                    NanString = numberFormat.NaNSymbol,
                    NegativePrefixString = numberFormat.NegativeSign,
                };
            }

            return field;
        }
    }

    private static DecimalNumberFormattingRules CreateDecimalNumberFormattingRules(UNumberFormatInfo numberFormat)
    {
        var useGrouping = numberFormat.NumberGroupSizes.Length > 0;
        var roundingMode = numberFormat.NumberingSystem;
    }

    private static readonly Lock _percentNumberFormattingRulesLock = new();
    public DecimalNumberFormattingRules PercentFormattingRules { get; }

    public DecimalNumberFormattingRules GetCurrencyFormattingRules(string currencyCode) { }

    public TextPluralForm GetPluralForm<T>(T value, TextPluralType pluralType)
        where T : unmanaged, INumber<T>
    {
        /*
        if (T.IsNaN(value) || T.IsInfinity(value))
            return TextPluralForm.Other;

        value = T.Abs(value);

        var number = double.CreateTruncating(value);
        if (double.IsNaN(number) || double.IsInfinity(number))
            return TextPluralForm.Other;

        var rules = PluralRulesCache.GetOrAdd(
            (Culture.Name, pluralType),
            static key =>
            {
                var (cultureName, type) = key;
                var locale = new UCultureInfo(cultureName);

                var pluralTypeIcu = type switch
                {
                    TextPluralType.Cardinal => PluralType.Cardinal,
                    TextPluralType.Ordinal => PluralType.Ordinal,
                    _ => PluralType.Cardinal,
                };

                return PluralRules.GetInstance(locale, pluralTypeIcu);
            }
        );

        var keyword = rules.Select(number);

        // ICU keywords: "zero", "one", "two", "few", "many", "other"
        return keyword switch
        {
            "zero" => TextPluralForm.Zero,
            "one" => TextPluralForm.One,
            "two" => TextPluralForm.Two,
            "few" => TextPluralForm.Few,
            "many" => TextPluralForm.Many,
            _ => TextPluralForm.Other,
        };
        */
        throw new NotImplementedException();
    }

    public IEnumerable<TextPluralForm> GetValidPluralForms(TextPluralType pluralType) { }
}
