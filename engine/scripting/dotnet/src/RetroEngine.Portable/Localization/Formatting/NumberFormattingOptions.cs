// // @file NumberFormattingOptions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;

namespace RetroEngine.Portable.Localization.Formatting;

public enum RoundingMode : int
{
    HalfToEven,
    HalfFromZero,
    HalfToZero,
    FromZero,
    ToZero,
    ToNegativeInfinity,
    ToPositiveInfinity,
}

public record NumberFormattingOptions
{
    /// <summary>
    /// (Max exponent of double) + (the number of decimal digits in a double) + 1.
    /// </summary>
    private const int DefaultMaximumIntegralDigits = 308 + 15 + 1;

    public bool AlwaysSign { get; init; } = false;
    public bool UseGrouping { get; init; } = true;
    public bool IndicateNearlyInteger { get; init; } = false;
    public RoundingMode RoundingMode { get; init; } = RoundingMode.HalfToEven;
    public int MinimumIntegralDigits { get; init; } = 1;
    public int MaximumIntegralDigits { get; init; } = DefaultMaximumIntegralDigits;
    public int MinimumFractionalDigits { get; init; } = 0;
    public int MaximumFractionalDigits { get; init; } = 3;

    public static readonly NumberFormattingOptions DefaultWithGrouping = new() { UseGrouping = true };

    public static readonly NumberFormattingOptions DefaultWithoutGrouping = new() { UseGrouping = false };
}
