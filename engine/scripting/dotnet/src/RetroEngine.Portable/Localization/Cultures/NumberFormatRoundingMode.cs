// // @file IcuNumberFormatRoundingMode.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Localization.Formatting;

namespace RetroEngine.Portable.Localization.Cultures;

internal enum NumberFormatRoundingMode
{
    RoundCeiling,
    RoundFloor,
    RoundDown,
    RoundUp,

    /**
     * Half-even rounding
     * @stable, ICU 3.8
     */
    RoundHalfEven,
    RoundHalfDown = RoundHalfEven + 1,
    RoundHalfUp,

    /**
      * ROUND_UNNECESSARY reports an error if formatted result is not exact.
      * @stable ICU 4.8
      */
    RoundUnnecessary,

    /**
     * Rounds ties toward the odd number.
     * @stable ICU 69
     */
    RoundHalfOdd,

    /**
     * Rounds ties toward +∞.
     * @stable ICU 69
     */
    RoundHalfCeiling,

    /**
     * Rounds ties toward -∞.
     * @stable ICU 69
     */
    RoundHalfFloor,
}

internal static class NumberRoundingModeExtensions
{
    public static RoundingMode ToRoundingMode(this NumberFormatRoundingMode mode)
    {
        return mode switch
        {
            NumberFormatRoundingMode.RoundHalfEven => RoundingMode.HalfToEven,
            NumberFormatRoundingMode.RoundHalfUp => RoundingMode.HalfFromZero,
            NumberFormatRoundingMode.RoundHalfDown => RoundingMode.HalfToZero,
            NumberFormatRoundingMode.RoundUp => RoundingMode.FromZero,
            NumberFormatRoundingMode.RoundDown => RoundingMode.ToZero,
            NumberFormatRoundingMode.RoundFloor => RoundingMode.ToNegativeInfinity,
            NumberFormatRoundingMode.RoundCeiling => RoundingMode.ToPositiveInfinity,
            _ => RoundingMode.HalfToEven,
        };
    }
}
