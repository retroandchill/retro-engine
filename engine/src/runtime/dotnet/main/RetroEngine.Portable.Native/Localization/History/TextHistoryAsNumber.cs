// // @file TextHistoryAsNumber.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Text;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Portable.Localization.Formatting;

namespace RetroEngine.Portable.Localization.History;

internal sealed class TextHistoryAsNumber : TextHistoryFormatNumber
{
    public TextHistoryAsNumber() { }

    public TextHistoryAsNumber(
        string displayString,
        FormatNumericArg sourceValue,
        NumberFormattingOptions? formattingOptions,
        Culture? targetCulture
    )
        : base(displayString, sourceValue, formattingOptions, targetCulture) { }

    protected override string BuildLocalizedDisplayString()
    {
        var culture = TargetCulture ?? CultureManager.Instance.CurrentLocale;
        var formattingRules = culture.DecimalNumberFormattingRules;
        return BuildNumericDisplayString(formattingRules);
    }

    public override string BuildInvariantDisplayString()
    {
        var culture = CultureManager.Instance.InvariantCulture;
        var formattingRules = culture.DecimalNumberFormattingRules;
        return BuildNumericDisplayString(formattingRules);
    }

    public override bool ReadFromBuffer(
        ReadOnlySpan<char> buffer,
        string? textNamespace,
        string? textKey,
        out ReadOnlySpan<char> remaining
    )
    {
        if (
            !buffer.ReadNumberOrPercent(
                TextStringificationUtil.LocGenNumberMarker,
                out var sourceValue,
                out var formattingOptions,
                out var targetCulture,
                out remaining
            )
        )
        {
            return false;
        }

        SourceValue = sourceValue;
        FormattingOptions = formattingOptions;
        TargetCulture = targetCulture;
        MarkDisplayStringOutOfDate();
        return true;
    }

    public override bool WriteToBuffer(StringBuilder buffer)
    {
        buffer.WriteNumberOrPercent(
            TextStringificationUtil.LocGenNumberMarker,
            SourceValue,
            FormattingOptions,
            TargetCulture
        );
        return true;
    }

    public override HistoricTextNumericData? GetHistoricNumericData(Text text)
    {
        return new HistoricTextNumericData(NumberFormatType.Number, SourceValue);
    }
}
