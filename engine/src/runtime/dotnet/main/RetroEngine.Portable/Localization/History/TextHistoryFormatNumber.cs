// // @file TextHistoryFormatNumber.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MagicArchive;
using MessagePack;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Portable.Localization.Formatting;

namespace RetroEngine.Portable.Localization.History;

internal abstract class TextHistoryFormatNumber(
    FormatNumericArg sourceValue,
    NumberFormattingOptions? formattingOptions,
    Culture? targetCulture
) : TextHistoryGenerated
{
    [ArchiveInclude]
    protected FormatNumericArg SourceValue { get; } = sourceValue;

    [ArchiveInclude]
    protected NumberFormattingOptions? FormattingOptions { get; } = formattingOptions;

    [ArchiveInclude]
    protected Culture? TargetCulture { get; } = targetCulture;

    public override bool IdenticalTo(TextHistory other, TextIdenticalModeFlags flags)
    {
        return other is TextHistoryFormatNumber otherNumber
            && GetType() == other.GetType()
            && SourceValue == otherNumber.SourceValue
            && FormattingOptions == otherNumber.FormattingOptions
            && Equals(TargetCulture, otherNumber.TargetCulture);
    }

    protected string BuildNumericDisplayString(DecimalNumberFormattingRules formattingRules, int valueMultiplier = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(valueMultiplier);
        var formattingOptions = FormattingOptions ?? formattingRules.DefaultFormattingOptions;
        return FastDecimalFormat.NumberToString(SourceValue, formattingRules, formattingOptions);
    }
}
