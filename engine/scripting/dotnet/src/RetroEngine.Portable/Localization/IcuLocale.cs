// // @file IcuLocale.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Portable.Localization.Formatting;

namespace RetroEngine.Portable.Localization;

public sealed partial class IcuLocale : ILocale
{
    private readonly string _icuLocaleName;
    public string DisplayName { get; }
    public string EnglishName { get; }
    public int KeyboardLayoutId { get; }
    public int LCID { get; }
    public string Name { get; }
    public string NativeName { get; }
    public string ThreeLetterISOLanguageName { get; }
    public string TwoLetterISOLanguageName { get; }
    public string NativeLanguage { get; }
    public string NativeRegion { get; }
    public string Script { get; }
    public string Variant { get; }
    public bool IsRightToLeft { get; }
    public DecimalNumberFormattingRules DecimalNumberFormattingRules { get; }
    public DecimalNumberFormattingRules PercentNumberFormattingRules { get; }

    public IcuLocale(string icuLocaleName)
    {
        _icuLocaleName = icuLocaleName;
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
}
