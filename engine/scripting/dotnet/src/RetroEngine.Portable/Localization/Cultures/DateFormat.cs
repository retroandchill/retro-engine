// // @file DateFormat.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Portable.Interop;
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

    public TimeZone TimeZone
    {
        get;
        set
        {
            field = value;
            NativeSetTimeZone(_nativeDateFormat, value.NativePtr);
        }
    } = TimeZone.Unknown;

    public DecimalFormat NumberFormat
    {
        set => NativeSetNumberFormat(_nativeDateFormat, value.NativeDecimalFormat);
    }

    private DateFormat(IntPtr nativeDateFormat) => _nativeDateFormat = nativeDateFormat;

    ~DateFormat()
    {
        ReleaseUnmanagedResources();
    }

    public static DateFormat? CreateDate(Locale locale, DateFormatStyle dateStyle)
    {
        var nativeDateFormat = NativeCreateDate(locale.NativeLocale, dateStyle);
        return nativeDateFormat != IntPtr.Zero ? new DateFormat(nativeDateFormat) : null;
    }

    public static DateFormat? CreateTime(Locale locale, DateFormatStyle timeStyle)
    {
        var nativeDateFormat = NativeCreateTime(locale.NativeLocale, timeStyle);
        return nativeDateFormat != IntPtr.Zero ? new DateFormat(nativeDateFormat) : null;
    }

    public static DateFormat? CreateDateTime(Locale locale, DateFormatStyle dateStyle, DateFormatStyle timeStyle)
    {
        var nativeDateFormat = NativeCreateDateTime(locale.NativeLocale, dateStyle, timeStyle);
        return nativeDateFormat != IntPtr.Zero ? new DateFormat(nativeDateFormat) : null;
    }

    public static DateFormat? Create(Locale locale, ReadOnlySpan<char> pattern)
    {
        var nativeDateFormat = NativeCreateCustom(locale.NativeLocale, pattern, pattern.Length);
        return nativeDateFormat != IntPtr.Zero ? new DateFormat(nativeDateFormat) : null;
    }

    public string Format(double date)
    {
        Span<char> buffer = stackalloc char[256];
        var length = NativeFormat(_nativeDateFormat, date, buffer, buffer.Length);
        return length > buffer.Length ? buffer.ToString() : buffer[..length].ToString();
    }

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_create_date_format")]
    private static partial IntPtr NativeCreateDate(IntPtr locale, DateFormatStyle dateStyle);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_create_time_format")]
    private static partial IntPtr NativeCreateTime(IntPtr locale, DateFormatStyle timeStyle);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_create_date_time_format")]
    private static partial IntPtr NativeCreateDateTime(
        IntPtr locale,
        DateFormatStyle dateStyle,
        DateFormatStyle timeStyle
    );

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_create_custom_date_format")]
    private static partial IntPtr NativeCreateCustom(IntPtr locale, ReadOnlySpan<char> pattern, int patternLength);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_destroy_date_format")]
    private static partial void NativeClose(IntPtr nativeDateFormat);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_date_format_set_time_zone")]
    private static partial void NativeSetTimeZone(IntPtr nativeDateFormat, IntPtr nativeTimeZone);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_date_format_set_decimal_format")]
    private static partial void NativeSetNumberFormat(IntPtr nativeDateFormat, IntPtr nativeNumberFormat);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_date_format_format")]
    private static partial int NativeFormat(IntPtr nativeDateFormat, double date, Span<char> result, int maxResultSize);

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
