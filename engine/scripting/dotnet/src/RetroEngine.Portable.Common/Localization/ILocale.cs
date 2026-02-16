// // @file ILocale.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Localization.Formatting;

namespace RetroEngine.Portable.Localization;

public enum TextPluralType : byte
{
    Cardinal,
    Ordinal,
}

public enum TextPluralForm : byte
{
    Zero,
    One,
    Two,
    Few,
    Many,
    Other,
}

public interface ILocale
{
    string DisplayName { get; }
    string EnglishName { get; }
    int KeyboardLayoutId { get; }
    int LCID { get; }
    string Name { get; }
    string NativeName { get; }
    string ThreeLetterISOLanguageName { get; }
    string TwoLetterISOLanguageName { get; }
    string NativeLanguage { get; }
    string NativeRegion { get; }
    string Script { get; }
    string Variant { get; }
    bool IsRightToLeft { get; }
    DecimalNumberFormattingRules DecimalNumberFormattingRules { get; }
    DecimalNumberFormattingRules PercentNumberFormattingRules { get; }

    DecimalNumberFormattingRules GetCurrencyFormattingRules(string currencyCode);
    TextPluralForm GetPluralForm(int value, TextPluralType pluralType);
    TextPluralForm GetPluralForm(double value, TextPluralType pluralType);
    IEnumerable<TextPluralForm> GetValidPluralForm(TextPluralType pluralType);
}
