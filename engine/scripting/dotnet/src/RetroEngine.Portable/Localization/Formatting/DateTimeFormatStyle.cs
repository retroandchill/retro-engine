// // @file DateFormatStyle.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TimeZoneNames;

namespace RetroEngine.Portable.Localization.Formatting;

public enum DateTimeFormatStyle
{
    Default,
    Short,
    Medium,
    Long,
    Full,
}

internal static class DateFormatStyleExtensions
{
    extension(DateTimeOffset dateTime)
    {
        public string ToDateString(
            DateTimeFormatStyle format = DateTimeFormatStyle.Default,
            string? timeZoneId = null,
            CultureHandle? targetCulture = null
        )
        {
            var culture = targetCulture ?? LocalizationManager.Instance.CurrentCulture;
            var timeZone = timeZoneId is not null
                ? TimeZoneInfo.FindSystemTimeZoneById(timeZoneId)
                : TimeZoneInfo.Local;
            var toLocalTime = dateTime.ToOffset(timeZone.GetUtcOffset(dateTime));
            return toLocalTime.ToDateStringInternal(format, culture);
        }

        private string ToDateStringInternal(DateTimeFormatStyle format, CultureHandle targetCulture)
        {
            var pattern = format switch
            {
                DateTimeFormatStyle.Default => null,
                DateTimeFormatStyle.Short => targetCulture.ShortDatePattern,
                DateTimeFormatStyle.Medium => targetCulture.MediumDatePattern,
                DateTimeFormatStyle.Long => targetCulture.LongDatePattern,
                DateTimeFormatStyle.Full => targetCulture.FullDatePattern,
                _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
            };

            return dateTime.ToString(pattern, targetCulture.Culture);
        }

        public string ToTimeString(
            DateTimeFormatStyle format = DateTimeFormatStyle.Default,
            string? timeZoneId = null,
            CultureHandle? targetCulture = null
        )
        {
            var culture = targetCulture ?? LocalizationManager.Instance.CurrentCulture;
            var timeZone = timeZoneId is not null
                ? TimeZoneInfo.FindSystemTimeZoneById(timeZoneId)
                : TimeZoneInfo.Local;
            var toLocalTime = dateTime.ToOffset(timeZone.GetUtcOffset(dateTime));
            return toLocalTime.ToTimeStringInternal(format, timeZone.Id, culture);
        }

        private string ToTimeStringInternal(DateTimeFormatStyle format, string timeZoneId, CultureHandle targetCulture)
        {
            var pattern =
                format == DateTimeFormatStyle.Short ? targetCulture.ShortTimePattern : targetCulture.LongTimePattern;
            var time = dateTime.ToString(pattern, targetCulture.Culture);

            if (format is not (DateTimeFormatStyle.Long or DateTimeFormatStyle.Full))
                return time;

            var timeZoneName = TZNames.GetAbbreviationsForTimeZone(timeZoneId, targetCulture.Name);
            return $"{time} {timeZoneName.Standard}";
        }

        public string ToDateTimeString(
            DateTimeFormatStyle dateFormat = DateTimeFormatStyle.Default,
            DateTimeFormatStyle timeFormat = DateTimeFormatStyle.Default,
            string? timeZoneId = null,
            CultureHandle? targetCulture = null
        )
        {
            var culture = targetCulture ?? LocalizationManager.Instance.CurrentCulture;
            var timeZone = timeZoneId is not null
                ? TimeZoneInfo.FindSystemTimeZoneById(timeZoneId)
                : TimeZoneInfo.Local;
            var toLocalTime = dateTime.ToOffset(timeZone.GetUtcOffset(dateTime));
            var date = toLocalTime.ToDateStringInternal(dateFormat, culture);
            var time = toLocalTime.ToTimeStringInternal(timeFormat, timeZone.Id, culture);
            return $"{date} {time}";
        }
    }
}
