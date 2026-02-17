// // @file NumberFormattingOptions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization.Formatting;

public enum GroupingRule : byte
{
    Enabled,
    Disabled,
}

public enum NumberFormatType : byte
{
    Number,
    Percent,
}

public readonly record struct NumberFormattingOptions
{
    public bool AlwaysSign { get; init; }
    public GroupingRule Grouping { get; init; }
    public int? MinimumIntegralDigits { get; init; }
    public int? MinimumFractionalDigits { get; init; }
    public int? MaximumFractionalDigits { get; init; }

    internal string BuildPattern(NumberFormatType formatType)
    {
        var integral = BuildIntegralPart();
        var fractional = BuildFractionalPart();
        var core = formatType switch
        {
            NumberFormatType.Number => $"{integral}{fractional}",
            NumberFormatType.Percent => $"{integral}{fractional}%",
            _ => throw new ArgumentOutOfRangeException(nameof(formatType), formatType, null),
        };

        if (!AlwaysSign)
            return core;

        var positive = $"+{core}";
        var negative = $"-{core}";

        return $"{positive};{negative};{positive}";
    }

    private string BuildIntegralPart()
    {
        var minInt = MinimumIntegralDigits ?? 1;
        var required = new string('0', minInt);

        return Grouping == GroupingRule.Enabled ? $"#,##{required}" : $"###{required}";
    }

    private string BuildFractionalPart()
    {
        if (MaximumFractionalDigits is null or 0)
            return string.Empty;

        var minFrac = MinimumFractionalDigits ?? 0;
        var maxFrac = MaximumFractionalDigits.Value;

        if (minFrac > maxFrac)
            throw new InvalidOperationException(
                "MinimumFractionalDigits cannot be greater than MaximumFractionalDigits."
            );

        var required = new string('0', minFrac);
        var optional = new string('#', maxFrac - minFrac);

        return "." + required + optional;
    }
}
