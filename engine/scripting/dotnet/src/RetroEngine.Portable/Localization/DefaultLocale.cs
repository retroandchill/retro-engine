// // @file DefaultLocale.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Globalization;
using RetroEngine.Portable.Localization.Formatting;

namespace RetroEngine.Portable.Localization;

public sealed class DefaultLocale : ILocale
{
    private readonly CultureInfo _cultureInfo;
    public string DisplayName => _cultureInfo.DisplayName;
    public string EnglishName => _cultureInfo.EnglishName;
    public int KeyboardLayoutId => _cultureInfo.KeyboardLayoutId;
    public int LCID => _cultureInfo.LCID;
    public string Name => _cultureInfo.Name;
    public string NativeName => _cultureInfo.NativeName;
    public string ThreeLetterISOLanguageName => _cultureInfo.ThreeLetterISOLanguageName;
    public string TwoLetterISOLanguageName => _cultureInfo.TwoLetterISOLanguageName;
    public string NativeLanguage { get; }
    public string NativeRegion { get; }
    public string Script { get; }
    public string Variant { get; }
    public bool IsRightToLeft => _cultureInfo.TextInfo.IsRightToLeft;
    public DecimalNumberFormattingRules DecimalNumberFormattingRules { get; }
    public DecimalNumberFormattingRules PercentNumberFormattingRules { get; }

    public DefaultLocale(CultureInfo cultureInfo)
    {
        _cultureInfo = cultureInfo;
    }

    public DecimalNumberFormattingRules GetCurrencyFormattingRules(string currencyCode)
    {
        throw new NotImplementedException();
    }

    public TextPluralForm GetPluralForm(int value, TextPluralType pluralType)
    {
        throw new NotImplementedException();
    }

    public TextPluralForm GetPluralForm(double value, TextPluralType pluralType)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<TextPluralForm> GetValidPluralForm(TextPluralType pluralType)
    {
        throw new NotImplementedException();
    }

    private static DecimalNumberFormattingRules ExtractNumberFormattingRules(NumberFormatInfo cultureInfo)
    {
        return new DecimalNumberFormattingRules
        {
            DefaultFormattingOptions = new NumberFormattingOptions
            {
                UseGrouping = cultureInfo.NumberGroupSizes.Length > 0 && cultureInfo.NumberGroupSizes[0] > 0,
            },
        };
    }
}
