// // @file TextHistoryAsTime.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Portable.Localization.Formatting;

namespace RetroEngine.Portable.Localization.History;

internal sealed class TextHistoryAsTime : TextHistoryGenerated
{
    private readonly DateTimeOffset _sourceDateTime;
    private readonly DateTimeFormatStyle _formatStyle;
    private readonly string? _timeZoneId;
    private readonly Culture? _targetCulture;

    public TextHistoryAsTime() { }

    public TextHistoryAsTime(
        string displayString,
        DateTimeOffset dateTime,
        DateTimeFormatStyle formatStyle,
        string? timeZoneId,
        Culture? targetCulture
    )
        : base(displayString)
    {
        _sourceDateTime = dateTime;
        _formatStyle = formatStyle;
        _timeZoneId = timeZoneId;
        _targetCulture = targetCulture;
    }

    public override string BuildInvariantDisplayString()
    {
        return BuildDateTimeDisplayString(Culture.InvariantCulture);
    }

    public override bool IdenticalTo(TextHistory other, TextIdenticalModeFlags flags)
    {
        return other is TextHistoryAsTime otherDateHistory
            && _sourceDateTime == otherDateHistory._sourceDateTime
            && _formatStyle == otherDateHistory._formatStyle
            && _timeZoneId == otherDateHistory._timeZoneId
            && _targetCulture == otherDateHistory._targetCulture;
    }

    protected override string BuildLocalizedDisplayString()
    {
        return BuildDateTimeDisplayString(_targetCulture ?? Culture.CurrentCulture);
    }

    private string BuildDateTimeDisplayString(Culture culture)
    {
        return TextChronoFormatter.AsTime(_sourceDateTime, _formatStyle, _timeZoneId, culture);
    }
}
