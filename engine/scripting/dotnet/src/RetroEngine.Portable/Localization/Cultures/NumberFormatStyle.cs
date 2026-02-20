// // @file NumberFormatStyle.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization.Cultures;

public enum NumberFormatStyle
{
    /**
     * Decimal format defined by a pattern string.
     * @stable ICU 3.0
     */
    PatternDecimal = 0,

    /**
     * Decimal format ("normal" style).
     * @stable ICU 2.0
     */
    Decimal = 1,

    /**
     * Currency format (generic).
     * Defaults to CURRENCY_STANDARD style
     * (using currency symbol, e.g., "$1.00", with non-accounting
     * style for negative values e.g. using minus sign).
     * The specific style may be specified using the -cf- locale key.
     * @stable ICU 2.0
     */
    Currency = 2,

    /**
     * Percent format
     * @stable ICU 2.0
     */
    Percent = 3,

    /**
     * Scientific format
     * @stable ICU 2.1
     */
    Scientific = 4,

    /**
     * Spellout rule-based format. The default ruleset can be specified/changed using
     * unum_setTextAttribute with DEFAULT_RULESET; the available public rulesets
     * can be listed using unum_getTextAttribute with PUBLIC_RULESETS.
     * @stable ICU 2.0
     */
    Spellout = 5,

    /**
     * Ordinal rule-based format . The default ruleset can be specified/changed using
     * unum_setTextAttribute with DEFAULT_RULESET; the available public rulesets
     * can be listed using unum_getTextAttribute with PUBLIC_RULESETS.
     * @stable ICU 3.0
     */
    Ordinal = 6,

    /**
     * Duration rule-based format
     * @stable ICU 3.0
     */
    Duration = 7,

    /**
     * Numbering system rule-based format
     * @stable ICU 4.2
     */
    NumberingSystem = 8,

    /**
     * Rule-based format defined by a pattern string.
     * @stable ICU 3.0
     */
    PatternRulebased = 9,

    /**
     * Currency format with an ISO currency code, e.g., "USD1.00".
     * @stable ICU 4.8
     */
    CurrencyISO = 10,

    /**
     * Currency format with a pluralized currency name,
     * e.g., "1.00 US dollar" and "3.00 US dollars".
     * @stable ICU 4.8
     */
    CurrencyPlural = 11,

    /**
     * Currency format for accounting, e.g., "($3.00)" for
     * negative currency amount instead of "-$3.00" ({@link #CURRENCY}).
     * Overrides any style specified using -cf- key in locale.
     * @stable ICU 53
     */
    CurrencyAccounting = 12,

    /**
     * Currency format with a currency symbol given CASH usage, e.g.,
     * "NT$3" instead of "NT$3.23".
     * @stable ICU 54
     */
    CashCurrency = 13,

    /**
     * Decimal format expressed using compact notation
     * (short form, corresponds to UNumberCompactStyle=SHORT)
     * e.g. "23K", "45B"
     * @stable ICU 56
     */
    DecimalCompactShort = 14,

    /**
     * Decimal format expressed using compact notation
     * (long form, corresponds to UNumberCompactStyle=LONG)
     * e.g. "23 thousand", "45 billion"
     * @stable ICU 56
     */
    DecimalCompactLong = 15,

    /**
     * Currency format with a currency symbol, e.g., "$1.00",
     * using non-accounting style for negative values (e.g. minus sign).
     * Overrides any style specified using -cf- key in locale.
     * @stable ICU 56
     */
    CurrencyStandard = 16,

    /**
     * One more than the highest normal UNumberFormatStyle value.
     * @deprecated ICU 58 The numeric value may change over time, see ICU ticket #12420.
     */
    FormatStyleCount = 17,

    /**
     * Default format
     * @stable ICU 2.0
     */
    Default = Decimal,

    /**
     * Alias for PATTERN_DECIMAL
     * @stable ICU 3.0
     */
    Ignore = PatternDecimal,
}
