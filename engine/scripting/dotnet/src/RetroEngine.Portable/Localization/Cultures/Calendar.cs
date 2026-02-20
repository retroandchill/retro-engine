// // @file Calendar.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace RetroEngine.Portable.Localization.Cultures;

internal enum CalendarType
{
    Traditional,
    Default = Traditional,
    Gregorian,
}

internal enum CalendarDateFields
{
    Era,
    Year,
    Month,
    WeekOfYear,
    WeekOfMonth,
    Date,
    DayOfYear,
    DayOfWeek,
    DayOfWeekInMonth,
    AmPm,
    Hour,
    HourOfDay,
    Minute,
    Second,
    Millisecond,
    ZoneOffset,
    DstOffset,
    YearWoy,
    DowLocal,
    ExtendedYear,
    JulianDay,
    MillisecondsInDay,
    IsLeapMonth,
    OrdinalMonth,
    FieldCount = OrdinalMonth + 1,
    DayOfMonth = Date,
}

internal sealed partial class Calendar : IDisposable
{
    private const string UnknownTimeZone = "Etc/Unknown";

    private readonly IntPtr _nativeCalendar;
    private bool _disposed;

    public static string DefaultTimeZone
    {
        get
        {
            Span<char> buffer = stackalloc char[Culture.KeywordAndValuesCapacity];
            var length = NativeGetDefaultTimeZone(buffer, buffer.Length, out _);
            return length > buffer.Length ? buffer.ToString() : buffer[..length].ToString();
        }
    }
    public double Time => NativeGetGregorianChange(_nativeCalendar, out _);

    private Calendar(IntPtr nativeCalendar)
    {
        _nativeCalendar = nativeCalendar;
    }

    ~Calendar()
    {
        ReleaseUnmanagedResources();
    }

    public static Calendar? Create(CalendarType type, ReadOnlySpan<char> timeZone = UnknownTimeZone)
    {
        var nativeCalendar = NativeOpen(timeZone, timeZone.Length, IntPtr.Zero, type, out _);
        return nativeCalendar != IntPtr.Zero ? new Calendar(nativeCalendar) : null;
    }

    public static string GetCanonicalTimeZoneId(ReadOnlySpan<char> id)
    {
        Span<char> buffer = stackalloc char[Culture.KeywordAndValuesCapacity];
        var length = NativeGetCanonicalTimeZoneId(id, id.Length, buffer, buffer.Length, out _);
        return length > buffer.Length ? buffer.ToString() : buffer[..length].ToString();
    }

    public void Set(CalendarDateFields dateField, int value) => NativeSet(_nativeCalendar, dateField, value);

    [LibraryImport(Culture.I18NLibName, EntryPoint = "ucal_open")]
    private static partial IntPtr NativeOpen(
        ReadOnlySpan<char> zoneId,
        int zoneIdLength,
        IntPtr locale,
        CalendarType type,
        out IcuErrorCode errorCode
    );

    [LibraryImport(Culture.I18NLibName, EntryPoint = "ucal_close")]
    private static partial void NativeClose(IntPtr nativeCalendar);

    [LibraryImport(Culture.I18NLibName, EntryPoint = "ucal_set")]
    private static partial void NativeSet(IntPtr nativeCalendar, CalendarDateFields dateField, int value);

    [LibraryImport(Culture.I18NLibName, EntryPoint = "ucal_getGregorianChange")]
    private static partial double NativeGetGregorianChange(IntPtr nativeCalendar, out IcuErrorCode errorCode);

    [LibraryImport(Culture.I18NLibName, EntryPoint = "ucal_getCanonicalTimeZoneID")]
    private static partial int NativeGetCanonicalTimeZoneId(
        ReadOnlySpan<char> id,
        int length,
        Span<char> result,
        int maxResultSize,
        out IcuErrorCode errorCode
    );

    [LibraryImport(Culture.I18NLibName, EntryPoint = "ucal_getDefaultTimeZone")]
    private static partial int NativeGetDefaultTimeZone(
        Span<char> result,
        int maxResultSize,
        out IcuErrorCode errorCode
    );

    [LibraryImport(Culture.I18NLibName, EntryPoint = "ucal_getTimeZoneID")]
    internal static partial int NativeGetTimeZoneId(
        IntPtr nativeCalendar,
        Span<char> result,
        int maxResultSize,
        out IcuErrorCode errorCode
    );

    private void ReleaseUnmanagedResources()
    {
        NativeClose(_nativeCalendar);
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
