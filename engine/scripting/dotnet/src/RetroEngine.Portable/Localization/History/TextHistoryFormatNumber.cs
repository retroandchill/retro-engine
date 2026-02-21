// // @file TextHistoryFormatNumber.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Portable.Localization.Formatting;

namespace RetroEngine.Portable.Localization.History;

internal abstract class TextHistoryFormatNumber<T> : TextHistoryGenerated
    where T : unmanaged, INumber<T>
{
    protected T SourceValue { get; }

    protected NumberFormattingOptions? FormattingOptions { get; }

    protected Culture? TargetCulture { get; }

    public TextHistoryFormatNumber() { }

    public TextHistoryFormatNumber(
        string displayString,
        T sourceValue,
        NumberFormattingOptions? formattingOptions,
        Culture? targetCulture
    )
        : base(displayString)
    {
        SourceValue = sourceValue;
        FormattingOptions = formattingOptions;
        TargetCulture = targetCulture;
    }

    public override bool IdenticalTo(TextHistory other, TextIdenticalModeFlags flags)
    {
        return other is TextHistoryFormatNumber<T> otherNumber
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
