// // @file DateFormat.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Portable.Localization.Formatting;

namespace RetroEngine.Portable.Localization.Cultures;

internal enum DateFormatStyle
{
    Full,
    Long,
    Medium,
    Short,
    Default = Medium,

    Relative = (1 << 7),
    FullRelative = Full | Relative,
    LongRelative = Long | Relative,
    MediumRelative = Medium | Relative,
    ShortRelative = Short | Relative,

    None = -1,
    Pattern = -2,
    Ignore = Pattern,
}

internal static class DateFormatStyleExtensions
{
    public static DateFormatStyle ToIcuEnum(this DateTimeFormatStyle style)
    {
        return style switch
        {
            DateTimeFormatStyle.Default => DateFormatStyle.Default,
            DateTimeFormatStyle.Short => DateFormatStyle.Short,
            DateTimeFormatStyle.Medium => DateFormatStyle.Medium,
            DateTimeFormatStyle.Long => DateFormatStyle.Long,
            DateTimeFormatStyle.Full => DateFormatStyle.Full,
            _ => throw new ArgumentOutOfRangeException(nameof(style), style, null),
        };
    }
}

internal sealed partial class DateFormat : IDisposable
{
    private readonly IntPtr _nativeDateFormat;
    private bool _disposed;

    public string TimeZoneId
    {
        get
        {
            var calendar = NativeGetCalendar(_nativeDateFormat);
            if (calendar == IntPtr.Zero)
                return Calendar.DefaultTimeZone;

            Span<char> buffer = stackalloc char[Culture.KeywordAndValuesCapacity];
            var length = Calendar.NativeGetTimeZoneId(calendar, buffer, buffer.Length, out _);
            return length > buffer.Length ? buffer.ToString() : buffer[..length].ToString();
        }
    }

    public DecimalFormat NumberFormat
    {
        set => NativeSetNumberFormat(_nativeDateFormat, value.NativeDecimalFormat);
    }

    private DateFormat(IntPtr nativeDateFormat) => _nativeDateFormat = nativeDateFormat;

    ~DateFormat()
    {
        ReleaseUnmanagedResources();
    }

    public static DateFormat? Create(
        DateFormatStyle timeStyle,
        DateFormatStyle dateStyle,
        CultureId locale,
        ReadOnlySpan<char> timeZoneId
    )
    {
        var nativeDateFormat = NativeOpen(timeStyle, dateStyle, locale, timeZoneId, timeZoneId.Length, null, 0, out _);
        return nativeDateFormat != IntPtr.Zero ? new DateFormat(nativeDateFormat) : null;
    }

    public static DateFormat? Create(ReadOnlySpan<char> pattern, CultureId locale, ReadOnlySpan<char> timeZoneId)
    {
        var nativeDateFormat = NativeOpen(
            DateFormatStyle.Default,
            DateFormatStyle.Default,
            locale,
            timeZoneId,
            timeZoneId.Length,
            pattern,
            pattern.Length,
            out _
        );
        return nativeDateFormat != IntPtr.Zero ? new DateFormat(nativeDateFormat) : null;
    }

    public string Format(double date)
    {
        Span<char> buffer = stackalloc char[256];
        var length = NativeFormat(_nativeDateFormat, date, buffer, buffer.Length, IntPtr.Zero, out _);
        return length > buffer.Length ? buffer.ToString() : buffer[..length].ToString();
    }

    [LibraryImport(Culture.UnicodeLibName, EntryPoint = "udat_open")]
    private static partial IntPtr NativeOpen(
        DateFormatStyle timeStyle,
        DateFormatStyle dateStyle,
        CultureId locale,
        ReadOnlySpan<char> timeZoneId,
        int timeZoneIdLength,
        ReadOnlySpan<char> pattern,
        int patternLength,
        out IcuErrorCode errorCode
    );

    [LibraryImport(Culture.UnicodeLibName, EntryPoint = "udat_close")]
    private static partial void NativeClose(IntPtr nativeDateFormat);

    [LibraryImport(Culture.UnicodeLibName, EntryPoint = "udat_setNumberFormat")]
    private static partial void NativeSetNumberFormat(IntPtr nativeDateFormat, IntPtr nativeNumberFormat);

    [LibraryImport(Culture.UnicodeLibName, EntryPoint = "udat_getCalendar")]
    private static partial IntPtr NativeGetCalendar(IntPtr nativeDateFormat);

    [LibraryImport(Culture.UnicodeLibName, EntryPoint = "udat_format")]
    private static partial int NativeFormat(
        IntPtr nativeDateFormat,
        double date,
        Span<char> result,
        int maxResultSize,
        IntPtr position,
        out IcuErrorCode errorCode
    );

    private void ReleaseUnmanagedResources()
    {
        NativeClose(_nativeDateFormat);
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }
}
