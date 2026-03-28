// // @file TextHistoryAsPercent.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Portable.Localization.Formatting;
using RetroEngine.Portable.Localization.Stringification;
using Superpower;
using Superpower.Model;

namespace RetroEngine.Portable.Localization.History;

internal sealed class TextHistoryAsPercent(
    string displayString,
    FormatNumericArg sourceValue,
    NumberFormattingOptions? formattingOptions,
    Culture? targetCulture
) : TextHistoryFormatNumber(displayString, sourceValue, formattingOptions, targetCulture), ITextHistory
{
    protected override string BuildLocalizedDisplayString()
    {
        var culture = TargetCulture ?? CultureManager.Instance.CurrentLocale;
        var formattingRules = culture.PercentNumberFormattingRules;
        return BuildNumericDisplayString(formattingRules, 100);
    }

    public override string BuildInvariantDisplayString()
    {
        var culture = CultureManager.Instance.InvariantCulture;
        var formattingRules = culture.PercentNumberFormattingRules;
        return BuildNumericDisplayString(formattingRules, 100);
    }

    private static readonly TextParser<ITextData> Parser = TextParsers
        .NumberOrPercent(Markers.LocGenPercent)
        .Select(ITextData (r) => new TextHistoryAsNumber("", r.Value, r.Options, r.TargetCulture));

    public static Result<ITextData> ReadFromBuffer(string str, string? textNamespace, string? textKey)
    {
        return Parser.TryParse(str);
    }

    public override bool WriteToBuffer(StringBuilder buffer)
    {
        buffer.WriteNumberOrPercent(Markers.LocGenPercent, SourceValue, FormattingOptions, TargetCulture);
        return true;
    }

    public override HistoricTextNumericData? GetHistoricNumericData(Text text)
    {
        return new HistoricTextNumericData(NumberFormatType.Percent, SourceValue);
    }
}
