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

    public bool AlwaysSign { get; init; }

    public bool UseGrouping { get; init; } = true;

    public bool IndicateNearlyInteger { get; init; }

    public RoundingMode RoundingMode { get; init; } = RoundingMode.HalfToEven;

    public int MinimumIntegralDigits { get; init; } = 1;

    public int MaximumIntegralDigits { get; init; } = DoubleMax10Exp + DoubleDigits10Exp + 1;

    public int MinimumFractionalDigits { get; init; }

    public int MaximumFractionalDigits { get; init; } = 3;

    public static NumberFormattingOptions DefaultWithGrouping { get; } = new() { UseGrouping = true };
    public static NumberFormattingOptions DefaultWithoutGrouping { get; } = new() { UseGrouping = false };
}

internal struct NumberFormattingOptionsBuilder
{
    private static readonly NumberFormattingOptions Default = new();

    private bool _edited;
    public bool? AlwaysSign
    {
        get;
        set
        {
            field = value;
            _edited = true;
        }
    }

    public bool? UseGrouping
    {
        get;
        set
        {
            field = value;
            _edited = true;
        }
    }

    public bool? IndicateNearlyInteger
    {
        get;
        set
        {
            field = value;
            _edited = true;
        }
    }

    public RoundingMode? RoundingMode
    {
        get;
        set
        {
            field = value;
            _edited = true;
        }
    }

    public int? MinimumIntegralDigits
    {
        get;
        set
        {
            field = value;
            _edited = true;
        }
    }

    public int? MaximumIntegralDigits
    {
        get;
        set
        {
            field = value;
            _edited = true;
        }
    }

    public int? MinimumFractionalDigits
    {
        get;
        set
        {
            field = value;
            _edited = true;
        }
    }

    public int? MaximumFractionalDigits
    {
        get;
        set
        {
            field = value;
            _edited = true;
        }
    }

    public NumberFormattingOptions Build()
    {
        if (!_edited)
            return Default;

        return new NumberFormattingOptions
        {
            AlwaysSign = AlwaysSign ?? Default.AlwaysSign,
            UseGrouping = UseGrouping ?? Default.UseGrouping,
            IndicateNearlyInteger = IndicateNearlyInteger ?? Default.IndicateNearlyInteger,
            RoundingMode = RoundingMode ?? Default.RoundingMode,
            MinimumIntegralDigits = MinimumIntegralDigits ?? Default.MinimumIntegralDigits,
            MaximumIntegralDigits = MaximumIntegralDigits ?? Default.MaximumIntegralDigits,
            MinimumFractionalDigits = MinimumFractionalDigits ?? Default.MinimumFractionalDigits,
            MaximumFractionalDigits = MaximumFractionalDigits ?? Default.MaximumFractionalDigits,
        };
    }
}
