// // @file TextChronoFormatter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Localization.Cultures;

namespace RetroEngine.Portable.Localization.Formatting;

public static class TextChronoFormatter
{
    public static string AsDate(
        DateTimeOffset dateTime,
        DateTimeFormatStyle style,
        string? timezone,
        Culture targetCulture
    )
    {
        var icuDate = Culture.DateTimeOffsetToIcuDate(dateTime);
        var format = targetCulture.GetDateFormatter(style, timezone);
        return format.Format(icuDate);
    }

    public static string AsTime(
        DateTimeOffset dateTime,
        DateTimeFormatStyle timeStyle,
        string? timezone,
        Culture culture
    )
    {
        var icuDate = Culture.DateTimeOffsetToIcuDate(dateTime);
        var format = culture.GetTimeFormatter(timeStyle, timezone);
        return format.Format(icuDate);
    }

    public static string AsDateTime(
        DateTimeOffset dateTime,
        DateTimeFormatStyle dateStyle,
        DateTimeFormatStyle timeStyle,
        string? timezone,
        Culture culture
    )
    {
        var icuDate = Culture.DateTimeOffsetToIcuDate(dateTime);
        var format = culture.GetDateTimeFormatter(dateStyle, timeStyle, timezone);
        return format.Format(icuDate);
    }

    public static string AsDateTime(
        DateTimeOffset dateTime,
        ReadOnlySpan<char> customPattern,
        string? timezone,
        Culture culture
    )
    {
        var icuDate = Culture.DateTimeOffsetToIcuDate(dateTime);
        var format = culture.GetDateTimeFormatter(customPattern, timezone);
        return format.Format(icuDate);
    }
}
