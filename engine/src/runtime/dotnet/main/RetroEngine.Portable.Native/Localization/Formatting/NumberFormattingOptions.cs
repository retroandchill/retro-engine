// // @file NumberFormattingOptions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization.Formatting;

public enum NumberFormatType : byte
{
    Number,
    Percent,
}

public enum RoundingMode
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
    private const int DoubleMax10Exp = 308;
    private const int DoubleDigits10Exp = 15;

    public bool AlwaysSign { get; init; } = false;
    public bool UseGrouping { get; init; } = true;
    public bool IndicateNearlyInteger { get; init; } = false;
    public RoundingMode RoundingMode { get; init; } = RoundingMode.HalfToEven;
    public int MinimumIntegralDigits { get; init; } = 1;
    public int MaximumIntegralDigits { get; init; } = DoubleMax10Exp + DoubleDigits10Exp + 1;
    public int MinimumFractionalDigits { get; init; }
    public int MaximumFractionalDigits { get; init; } = 3;

    public static NumberFormattingOptions DefaultWithGrouping { get; } = new() { UseGrouping = true };
    public static NumberFormattingOptions DefaultWithoutGrouping { get; } = new() { UseGrouping = false };
}
