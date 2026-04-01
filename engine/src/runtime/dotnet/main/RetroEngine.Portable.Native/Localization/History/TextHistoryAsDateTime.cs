// // @file TextHistoryAsDateTime.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Portable.Localization.Formatting;
using RetroEngine.Portable.Localization.Stringification;
using ZParse;

namespace RetroEngine.Portable.Localization.History;

internal sealed class TextHistoryAsDateTime : TextHistoryGenerated, ITextHistory
{
    private readonly DateTimeOffset _sourceDateTime;
    private readonly string? _customPattern;
    private readonly DateTimeFormatStyle _dateFormatStyle;
    private readonly DateTimeFormatStyle _timeFormatStyle;
    private readonly string? _timeZoneId;
    private readonly Culture? _targetCulture;

    public TextHistoryAsDateTime(
        string displayString,
        DateTimeOffset dateTime,
        DateTimeFormatStyle dateFormatStyle,
        DateTimeFormatStyle timeFormatStyle,
        string? timeZoneId,
        Culture? targetCulture
    )
    {
        _sourceDateTime = dateTime;
        _dateFormatStyle = dateFormatStyle;
        _timeFormatStyle = timeFormatStyle;
        _timeZoneId = timeZoneId;
        _targetCulture = targetCulture;
        UpdateDisplayString();
    }

    public TextHistoryAsDateTime(
        string displayString,
        DateTimeOffset dateTime,
        string pattern,
        string? timeZoneId,
        Culture? targetCulture
    )
    {
        _sourceDateTime = dateTime;
        _customPattern = pattern;
        _timeZoneId = timeZoneId;
        _targetCulture = targetCulture;
        UpdateDisplayString();
    }

    private TextHistoryAsDateTime(
        string displayString,
        DateTimeOffset dateTime,
        DateTimeFormatStyle dateFormatStyle,
        DateTimeFormatStyle timeFormatStyle,
        string? pattern,
        string timeZoneId,
        Culture? targetCulture
    )
    {
        _sourceDateTime = dateTime;
        _dateFormatStyle = dateFormatStyle;
        _timeFormatStyle = timeFormatStyle;
        _customPattern = pattern;
        _timeZoneId = timeZoneId;
        _targetCulture = targetCulture;
        UpdateDisplayString();
    }

    public override string BuildInvariantDisplayString()
    {
        return BuildDateTimeDisplayString(CultureManager.Instance.InvariantCulture);
    }

    public static ParseResult<ITextData> ReadFromBuffer(TextSegment input, string? textNamespace)
    {
        return input
            .ParseDateTime(Markers.LocGenTime, true, true)
            .Select(
                ITextData (r) =>
                    new TextHistoryAsDateTime(
                        "",
                        r.Value,
                        r.DateStyle,
                        r.TimeStyle,
                        r.CustomPattern,
                        r.TimeZone,
                        r.TargetCulture
                    )
            );
    }

    public override bool WriteToBuffer(StringBuilder buffer)
    {
        buffer.WriteDateTime(
            Markers.LocGenDate,
            _sourceDateTime,
            _dateFormatStyle,
            _timeFormatStyle,
            _customPattern,
            _timeZoneId,
            _targetCulture
        );
        return true;
    }

    public override bool IdenticalTo(TextHistory other, TextIdenticalModeFlags flags)
    {
        return other is TextHistoryAsDateTime otherDateHistory
            && _sourceDateTime == otherDateHistory._sourceDateTime
            && _dateFormatStyle == otherDateHistory._dateFormatStyle
            && _timeFormatStyle == otherDateHistory._timeFormatStyle
            && _timeZoneId == otherDateHistory._timeZoneId
            && _targetCulture == otherDateHistory._targetCulture;
    }

    protected override string BuildLocalizedDisplayString()
    {
        return BuildDateTimeDisplayString(_targetCulture ?? CultureManager.Instance.CurrentLocale);
    }

    private string BuildDateTimeDisplayString(Culture culture)
    {
        return _customPattern is not null
            ? TextChronoFormatter.AsDateTime(_sourceDateTime, _customPattern, _timeZoneId, culture)
            : TextChronoFormatter.AsDateTime(_sourceDateTime, _dateFormatStyle, _timeFormatStyle, _timeZoneId, culture);
    }
}
