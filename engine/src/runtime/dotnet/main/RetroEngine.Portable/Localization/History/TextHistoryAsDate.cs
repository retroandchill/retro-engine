// // @file TextHistoryAsDate.cs
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
internal sealed partial class TextHistoryAsDate : TextHistoryGenerated, ITextHistory
{
    [ArchiveInclude]
    private readonly DateTimeOffset _sourceDateTime;

    [ArchiveInclude]
    private readonly DateTimeFormatStyle _formatStyle;

    [ArchiveInclude]
    private readonly Culture? _targetCulture;

    [ArchiveInclude]
    private readonly string? _timeZoneId;

    public TextHistoryAsDate(
        DateTimeOffset sourceDateTime,
        DateTimeFormatStyle formatStyle,
        string? timeZoneId,
        Culture? targetCulture
    )
    {
        _timeZoneId = timeZoneId;
        _sourceDateTime = sourceDateTime;
        _formatStyle = formatStyle;
        _targetCulture = targetCulture;
        UpdateDisplayString();
    }

    public override string BuildInvariantDisplayString()
    {
        return BuildDateTimeDisplayString(CultureManager.Instance.InvariantCulture);
    }

    private static readonly TextParser<ITextData> Parser = TextStringReader
        .DateTime(Markers.LocGenDate, true, false)
        .Select(ITextData (r) => new TextHistoryAsDate(r.Value, r.DateStyle, r.TimeZone, r.TargetCulture));

    public static ParseResult<ITextData> ImportFromString(TextSegment input, string? textNamespace)
    {
        return Parser(input);
    }

    public override bool ExportToString(StringBuilder buffer)
    {
        buffer.WriteDateTime(
            Markers.LocGenDate,
            _sourceDateTime,
            _formatStyle,
            null,
            null,
            _timeZoneId,
            _targetCulture
        );
        return true;
    }

    public override bool IdenticalTo(TextHistory other, TextIdenticalModeFlags flags)
    {
        return other is TextHistoryAsDate otherDateHistory
            && _sourceDateTime == otherDateHistory._sourceDateTime
            && _formatStyle == otherDateHistory._formatStyle
            && _targetCulture == otherDateHistory._targetCulture;
    }

    protected override string BuildLocalizedDisplayString()
    {
        return BuildDateTimeDisplayString(_targetCulture ?? CultureManager.Instance.CurrentLocale);
    }

    private string BuildDateTimeDisplayString(Culture culture)
    {
        return TextChronoFormatter.AsDate(_sourceDateTime, _formatStyle, _timeZoneId, culture);
    }
}
