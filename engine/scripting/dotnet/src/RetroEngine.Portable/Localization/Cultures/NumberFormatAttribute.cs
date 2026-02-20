namespace RetroEngine.Portable.Localization.Cultures;

internal enum NumberFormatAttribute
{
    /** Parse integers only */
    ParseIntOnly,

    /** Use grouping separator */
    GroupingUsed,

    /** Always show decimal point */
    DecimalAlwaysShown,

    /** Maximum integer digits */
    MaxIntegerDigits,

    /** Minimum integer digits */
    MinIntegerDigits,

    /** Integer digits */
    IntegerDigits,

    /** Maximum fraction digits */
    MaxFractionDigits,

    /** Minimum fraction digits */
    MinFractionDigits,

    /** Fraction digits */
    FractionDigits,

    /** Multiplier */
    Multiplier,

    /** Grouping size */
    GroupingSize,

    /** Rounding Mode */
    RoundingMode,

    /** Rounding increment */
    RoundingIncrement,

    /** The width to which the output of <code>format()</code> is padded. */
    FormatWidth,

    /** The position at which padding will take place. */
    PaddingPosition,

    /** Secondary grouping size */
    SecondaryGroupingSize,

    /** Use significant digits
   * @stable ICU 3.0 */
    SignificantDigitsUsed,

    /** Minimum significant digits
   * @stable ICU 3.0 */
    MinSignificantDigits,

    /** Maximum significant digits
   * @stable ICU 3.0 */
    MaxSignificantDigits,

    /** Lenient parse mode used by rule-based formats.
   * @stable ICU 3.0
   */
    LenientParse,

    /** Consume all input. (may use fastpath). Set to YES (require fastpath), NO (skip fastpath), or MAYBE (heuristic).
   * This is an internal ICU API. Do not use.
   * @internal
   */
    ParseAllInput = 20,

    /**
    * Scale, which adjusts the position of the
    * decimal point when formatting.  Amounts will be multiplied by 10 ^ (scale)
    * before they are formatted.  The default value for the scale is 0 ( no adjustment ).
    *
    * <p>Example: setting the scale to 3, 123 formats as "123,000"
    * <p>Example: setting the scale to -4, 123 formats as "0.0123"
    *
    * This setting is analogous to getMultiplierScale() and setMultiplierScale() in decimfmt.h.
    *
   * @stable ICU 51 */
    Scale = 21,

    /**
   * Minimum grouping digits; most commonly set to 2 to print "1000" instead of "1,000".
   * See DecimalFormat::getMinimumGroupingDigits().
   *
   * For better control over grouping strategies, use UNumberFormatter.
   *
   * @stable ICU 64
   */
    MinimumGroupingDigits = 22,

    /**
   * if this attribute is set to 0, it is set to CURRENCY_STANDARD purpose,
   * otherwise it is CASH_CURRENCY purpose
   * Default: 0 (CURRENCY_STANDARD purpose)
   * @stable ICU 54
   */
    CurrencyUsage = 23,

    /** One below the first bitfield-boolean item.
   * All items after this one are stored in boolean form.
   * @internal */
    MaxNonbooleanAttribute = 0x0FFF,

    /** If 1, specifies that if setting the "max integer digits" attribute would truncate a value, set an error status rather than silently truncating.
   * For example,  formatting the value 1234 with 4 max int digits would succeed, but formatting 12345 would fail. There is no effect on parsing.
   * Default: 0 (not set)
   * @stable ICU 50
   */
    FormatFailIfMoreThanMaxDigits = 0x1000,

    /**
   * if this attribute is set to 1, specifies that, if the pattern doesn't contain an exponent, the exponent will not be parsed. If the pattern does contain an exponent, this attribute has no effect.
   * Has no effect on formatting.
   * Default: 0 (unset)
   * @stable ICU 50
   */
    ParseNoExponent = 0x1001,

    /**
   * if this attribute is set to 1, specifies that, if the pattern contains a
   * decimal mark the input is required to have one. If this attribute is set to 0,
   * specifies that input does not have to contain a decimal mark.
   * Has no effect on formatting.
   * Default: 0 (unset)
   * @stable ICU 54
   */
    ParseDecimalMarkRequired = 0x1002,

    /**
   * Parsing: if set to 1, parsing is sensitive to case (lowercase/uppercase).
   *
   * @stable ICU 64
   */
    ParseCaseSensitive = 0x1003,

    /**
   * Formatting: if set to 1, whether to show the plus sign on non-negative numbers.
   *
   * For better control over sign display, use UNumberFormatter.
   *
   * @stable ICU 64
   */
    SignAlwaysShown = 0x1004,

    /** Limit of boolean attributes. (value should
   * not depend on U_HIDE conditionals)
   * @internal */
    LimitBooleanAttribute = 0x1005,
}
