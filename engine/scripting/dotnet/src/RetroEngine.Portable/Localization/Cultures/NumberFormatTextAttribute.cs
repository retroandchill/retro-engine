// // @file NumberFormatTextAttribute.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization.Cultures;

public enum NumberFormatTextAttribute
{
    /** Positive prefix */
    PositivePrefix,

    /** Positive suffix */
    PositiveSuffix,

    /** Negative prefix */
    NegativePrefix,

    /** Negative suffix */
    NegativeSuffix,

    /** The character used to pad to the format width. */
    PaddingCharacter,

    /** The ISO currency code */
    CurrencyCode,

    /**
     * The default rule set, such as "%spellout-numbering-year:", "%spellout-cardinal:",
     * "%spellout-ordinal-masculine-plural:", "%spellout-ordinal-feminine:", or
     * "%spellout-ordinal-neuter:". The available public rulesets can be listed using
     * unum_getTextAttribute with PUBLIC_RULESETS. This is only available with
     * rule-based formatters.
     * @stable ICU 3.0
     */
    DefaultRuleset,

    /**
     * The public rule sets.  This is only available with rule-based formatters.
     * This is a read-only attribute.  The public rulesets are returned as a
     * single string, with each ruleset name delimited by ';' (semicolon). See the
     * CLDR LDML spec for more information about RBNF rulesets:
     * http://www.unicode.org/reports/tr35/tr35-numbers.html#Rule-Based_Number_Formatting
     * @stable ICU 3.0
     */
    PublicRulesets,
}
