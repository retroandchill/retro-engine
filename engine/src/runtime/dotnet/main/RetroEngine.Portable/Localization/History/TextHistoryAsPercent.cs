// // @file TextHistoryAsPercent.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using MagicArchive;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Portable.Localization.Formatting;
using RetroEngine.Portable.Localization.Stringification;
using ZParse;

namespace RetroEngine.Portable.Localization.History;

[Archivable]
internal sealed partial class TextHistoryAsPercent : TextHistoryFormatNumber, ITextHistory
{
    public TextHistoryAsPercent(
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
        var formattingRules = culture.PercentNumberFormattingRules;
        return BuildNumericDisplayString(formattingRules, 100);
    }

    public override string BuildInvariantDisplayString()
    {
        var culture = CultureManager.Instance.InvariantCulture;
        var formattingRules = culture.PercentNumberFormattingRules;
        return BuildNumericDisplayString(formattingRules, 100);
    }

    private static readonly TextParser<ITextData> Parser = TextStringReader
        .NumberOrPercent(Markers.LocGenPercent)
        .Select(ITextData (r) => new TextHistoryAsPercent(r.Value, r.Options, r.TargetCulture));

    public static ParseResult<ITextData> ImportFromString(TextSegment input, string? textNamespace)
    {
        return Parser(input);
    }

    public override bool ExportToString(StringBuilder buffer)
    {
        buffer.WriteNumberOrPercent(Markers.LocGenPercent, SourceValue, FormattingOptions, TargetCulture);
        return true;
    }

    public override HistoricTextNumericData? GetHistoricNumericData(Text text)
    {
        return new HistoricTextNumericData(NumberFormatType.Percent, SourceValue);
    }
}
