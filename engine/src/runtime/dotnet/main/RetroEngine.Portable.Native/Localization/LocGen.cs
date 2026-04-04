// // @file LocGen.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Portable.Localization.Formatting;
using ZLinq;

namespace RetroEngine.Portable.Localization;

public static class LocGen
{
    private static Culture? GetCulture(string culture)
    {
        return string.IsNullOrEmpty(culture) ? null : CultureManager.Instance.GetCulture(culture);
    }

    public static Text Number(FormatNumericArg arg, string culture) => Text.AsNumber(arg, null, GetCulture(culture));

    public static Text NumberGrouped(FormatNumericArg arg, string culture) =>
        Text.AsNumber(arg, NumberFormattingOptions.DefaultWithGrouping, GetCulture(culture));

    public static Text NumberUngrouped(FormatNumericArg arg, string culture) =>
        Text.AsNumber(arg, NumberFormattingOptions.DefaultWithoutGrouping, GetCulture(culture));

    public static Text NumberCustom(FormatNumericArg arg, NumberFormattingOptions options, string culture) =>
        Text.AsNumber(arg, options, GetCulture(culture));

    public static Text Percent(FormatNumericArg arg, string culture) => Text.AsPercent(arg, null, GetCulture(culture));

    public static Text PercentGrouped(FormatNumericArg arg, string culture) =>
        Text.AsPercent(arg, NumberFormattingOptions.DefaultWithGrouping, GetCulture(culture));

    public static Text PercentUngrouped(FormatNumericArg arg, string culture) =>
        Text.AsPercent(arg, NumberFormattingOptions.DefaultWithoutGrouping, GetCulture(culture));

    public static Text PercentCustom(FormatNumericArg arg, NumberFormattingOptions options, string culture) =>
        Text.AsPercent(arg, options, GetCulture(culture));

    public static Text Currency(FormatNumericArg num, string currency, string culture) =>
        Text.AsCurrency(num, currency, null, GetCulture(culture));

    public static Text DateUtc(long unixTime, DateTimeFormatStyle dateStyle, string timeZone, string culture) =>
        Text.AsDate(DateTimeOffset.FromUnixTimeMilliseconds(unixTime), dateStyle, timeZone, GetCulture(culture));

    public static Text DateLocal(long unixTime, DateTimeFormatStyle dateStyle, string culture) =>
        Text.AsDate(
            DateTimeOffset.FromUnixTimeMilliseconds(unixTime),
            dateStyle,
            Text.InvariantTimeZone,
            GetCulture(culture)
        );

    public static Text TimeUtc(long unixTime, DateTimeFormatStyle timeStyle, string timeZone, string culture) =>
        Text.AsTime(DateTimeOffset.FromUnixTimeMilliseconds(unixTime), timeStyle, timeZone, GetCulture(culture));

    public static Text TimeLocal(long unixTime, DateTimeFormatStyle timeStyle, string culture) =>
        Text.AsTime(
            DateTimeOffset.FromUnixTimeMilliseconds(unixTime),
            timeStyle,
            Text.InvariantTimeZone,
            GetCulture(culture)
        );

    public static Text DateTimeUtc(
        long unixTime,
        DateTimeFormatStyle dateStyle,
        DateTimeFormatStyle timeStyle,
        string timeZone,
        string culture
    ) =>
        Text.AsDateTime(
            DateTimeOffset.FromUnixTimeMilliseconds(unixTime),
            dateStyle,
            timeStyle,
            timeZone,
            GetCulture(culture)
        );

    public static Text DateTimeLocal(
        long unixTime,
        DateTimeFormatStyle dateStyle,
        DateTimeFormatStyle timeStyle,
        string culture
    ) =>
        Text.AsDateTime(
            DateTimeOffset.FromUnixTimeMilliseconds(unixTime),
            dateStyle,
            timeStyle,
            Text.InvariantTimeZone,
            GetCulture(culture)
        );

    public static Text DateTimeCustomUtc(long unixTime, string pattern, string timeZone, string culture) =>
        Text.AsDateTime(DateTimeOffset.FromUnixTimeMilliseconds(unixTime), pattern, timeZone, GetCulture(culture));

    public static Text DateTimeCustomLocal(long unixTime, string pattern, string culture) =>
        Text.AsDateTime(
            DateTimeOffset.FromUnixTimeMilliseconds(unixTime),
            pattern,
            Text.InvariantTimeZone,
            GetCulture(culture)
        );
}
