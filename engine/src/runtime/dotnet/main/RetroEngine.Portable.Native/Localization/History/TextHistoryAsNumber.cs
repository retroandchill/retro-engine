// // @file TextHistoryAsNumber.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Text;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Portable.Localization.Formatting;
using RetroEngine.Portable.Localization.Stringification;
using ZParse;

namespace RetroEngine.Portable.Localization.History;

internal sealed class TextHistoryAsNumber : TextHistoryFormatNumber, ITextHistory
{
    public TextHistoryAsNumber(
        FormatNumericArg sourceValue,
        NumberFormattingOptions? formattingOptions,
        Culture? targetCulture
    )
        : base(sourceValue, formattingOptions, targetCulture)
    {
        UpdateDisplayString();
    }

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

    public static ParseResult<ITextData> ReadFromBuffer(ParseCursor input, string? textNamespace)
    {
        return input
            .ParseNumberOrPercent(Markers.LocGenNumber)
            .Select(ITextData (r) => new TextHistoryAsNumber(r.Value, r.Options, r.TargetCulture));
    }

    public override bool WriteToBuffer(StringBuilder buffer)
    {
        buffer.WriteNumberOrPercent(Markers.LocGenNumber, SourceValue, FormattingOptions, TargetCulture);
        return true;
    }

    public override HistoricTextNumericData? GetHistoricNumericData(Text text)
    {
        return new HistoricTextNumericData(NumberFormatType.Number, SourceValue);
    }
}
