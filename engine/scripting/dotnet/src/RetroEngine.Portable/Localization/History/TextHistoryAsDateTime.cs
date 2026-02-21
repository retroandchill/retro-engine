// // @file TextHistoryAsDateTime.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Portable.Localization.Formatting;

namespace RetroEngine.Portable.Localization.History;

internal sealed class TextHistoryAsDateTime : TextHistoryGenerated
{
    private readonly DateTimeOffset _sourceDateTime;
    private readonly string? _customPattern;
    private readonly DateTimeFormatStyle _dateFormatStyle;
    private readonly DateTimeFormatStyle _timeFormatStyle;
    private readonly string? _timeZoneId;
    private readonly Culture? _targetCulture;

    public TextHistoryAsDateTime() { }

    public TextHistoryAsDateTime(
        string displayString,
        DateTimeOffset dateTime,
        DateTimeFormatStyle dateFormatStyle,
        DateTimeFormatStyle timeFormatStyle,
        string? timeZoneId,
        Culture? targetCulture
    )
        : base(displayString)
    {
        _sourceDateTime = dateTime;
        _dateFormatStyle = dateFormatStyle;
        _timeFormatStyle = timeFormatStyle;
        _timeZoneId = timeZoneId;
        _targetCulture = targetCulture;
    }

    public TextHistoryAsDateTime(
        string displayString,
        DateTimeOffset dateTime,
        string pattern,
        string? timeZoneId,
        Culture? targetCulture
    )
        : base(displayString)
    {
        _sourceDateTime = dateTime;
        _customPattern = pattern;
        _timeZoneId = timeZoneId;
        _targetCulture = targetCulture;
    }

    public override string BuildInvariantDisplayString()
    {
        return BuildDateTimeDisplayString(Culture.InvariantCulture);
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
        return BuildDateTimeDisplayString(_targetCulture ?? Culture.CurrentCulture);
    }

    private string BuildDateTimeDisplayString(Culture culture)
    {
        return _customPattern is not null
            ? TextChronoFormatter.AsDateTime(_sourceDateTime, _customPattern, _timeZoneId, culture)
            : TextChronoFormatter.AsDateTime(_sourceDateTime, _dateFormatStyle, _timeFormatStyle, _timeZoneId, culture);
    }
}
