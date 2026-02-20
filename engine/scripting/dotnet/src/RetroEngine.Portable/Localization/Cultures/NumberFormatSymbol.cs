// // @file NumberFormatSymbol.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization.Cultures;

public enum NumberFormatSymbol
{
    /** The decimal separator */
    DecimalSeparatorSymbol = 0,

    /** The grouping separator */
    GroupingSeparatorSymbol = 1,

    /** The pattern separator */
    PatternSeparatorSymbol = 2,

    /** The percent sign */
    PercentSymbol = 3,

    /** Zero*/
    ZeroDigitSymbol = 4,

    /** Character representing a digit in the pattern */
    DigitSymbol = 5,

    /** The minus sign */
    MinusSignSymbol = 6,

    /** The plus sign */
    PlusSignSymbol = 7,

    /** The currency symbol */
    CurrencySymbol = 8,

    /** The international currency symbol */
    IntlCurrencySymbol = 9,

    /** The monetary separator */
    MonetarySeparatorSymbol = 10,

    /** The exponential symbol */
    ExponentialSymbol = 11,

    /** Per mill symbol */
    PermillSymbol = 12,

    /** Escape padding character */
    PadEscapeSymbol = 13,

    /** Infinity symbol */
    InfinitySymbol = 14,

    /** Nan symbol */
    NanSymbol = 15,

    /** Significant digit symbol
     * @stable ICU 3.0 */
    SignificantDigitSymbol = 16,

    /** The monetary grouping separator
     * @stable ICU 3.6
     */
    MonetaryGroupingSeparatorSymbol = 17,

    /** One
     * @stable ICU 4.6
     */
    OneDigitSymbol = 18,

    /** Two
     * @stable ICU 4.6
     */
    TwoDigitSymbol = 19,

    /** Three
     * @stable ICU 4.6
     */
    ThreeDigitSymbol = 20,

    /** Four
     * @stable ICU 4.6
     */
    FourDigitSymbol = 21,

    /** Five
     * @stable ICU 4.6
     */
    FiveDigitSymbol = 22,

    /** Six
     * @stable ICU 4.6
     */
    SixDigitSymbol = 23,

    /** Seven
      * @stable ICU 4.6
     */
    SevenDigitSymbol = 24,

    /** Eight
     * @stable ICU 4.6
     */
    EightDigitSymbol = 25,

    /** Nine
     * @stable ICU 4.6
     */
    NineDigitSymbol = 26,

    /** Multiplication sign
     * @stable ICU 54
     */
    ExponentMultiplicationSymbol = 27,

    /** Approximately sign.
     * @internal
     */
    APPROXIMATELY_SIGN_SYMBOL = 28,

    /**
     * One more than the highest normal UNumberFormatSymbol value.
     * @deprecated ICU 58 The numeric value may change over time, see ICU ticket #12420.
     */
    FORMAT_SYMBOL_COUNT = 29,
}
