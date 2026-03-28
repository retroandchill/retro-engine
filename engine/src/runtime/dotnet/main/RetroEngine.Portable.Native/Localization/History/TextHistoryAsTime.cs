// // @file TextHistoryAsTime.cs
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

internal sealed class TextHistoryAsTime(
    string displayString,
    DateTimeOffset dateTime,
    DateTimeFormatStyle formatStyle,
    string? timeZoneId,
    Culture? targetCulture
) : TextHistoryGenerated(displayString), ITextHistory
{
    private readonly DateTimeOffset _sourceDateTime = dateTime;
    private readonly DateTimeFormatStyle _formatStyle = formatStyle;
    private readonly string? _timeZoneId = timeZoneId;
    private readonly Culture? _targetCulture = targetCulture;

    public override string BuildInvariantDisplayString()
    {
        return BuildDateTimeDisplayString(CultureManager.Instance.InvariantCulture);
    }

    private static readonly TextParser<ITextData> Parser = TextParsers
        .DateTime(Markers.LocGenTime, true, false)
        .Select(ITextData (r) => new TextHistoryAsTime("", r.Value, r.TimeStyle, r.TimeZone, r.TargetCulture));

    public static Result<ITextData> ReadFromBuffer(string str, string? textNamespace, string? textKey)
    {
        return Parser.TryParse(str);
    }

    public override bool WriteToBuffer(StringBuilder buffer)
    {
        buffer.WriteDateTime(
            Markers.LocGenDate,
            _sourceDateTime,
            null,
            _formatStyle,
            null,
            _timeZoneId,
            _targetCulture
        );
        return true;
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
        return BuildDateTimeDisplayString(_targetCulture ?? CultureManager.Instance.CurrentLocale);
    }

    private string BuildDateTimeDisplayString(Culture culture)
    {
        return TextChronoFormatter.AsTime(_sourceDateTime, _formatStyle, _timeZoneId, culture);
    }
}
