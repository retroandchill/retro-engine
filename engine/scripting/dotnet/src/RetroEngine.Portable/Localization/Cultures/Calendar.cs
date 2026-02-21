// // @file Calendar.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Portable.Interop;

namespace RetroEngine.Portable.Localization.Cultures;

internal sealed partial class Calendar : IDisposable
{
    private readonly IntPtr _nativeCalendar;
    private bool _disposed;

    public double Time => NativeGetTime(_nativeCalendar, out _);

    public TimeZone TimeZone
    {
        get;
        set
        {
            field = value;
            NativeSetTimeZone(_nativeCalendar, value.NativePtr);
        }
    } = TimeZone.Unknown;

    private Calendar(IntPtr nativeCalendar)
    {
        _nativeCalendar = nativeCalendar;
    }

    ~Calendar()
    {
        ReleaseUnmanagedResources();
    }

    public static Calendar? Create()
    {
        var nativeCalendar = NativeOpen();
        return nativeCalendar != IntPtr.Zero ? new Calendar(nativeCalendar) : null;
    }

    public void Set(int year, int month, int dayOfMonth, int hourOfDay, int minute, int second)
    {
        NativeSet(_nativeCalendar, year, month, dayOfMonth, hourOfDay, minute, second);
    }

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_create_calendar")]
    private static partial IntPtr NativeOpen();

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_destroy_calendar")]
    private static partial void NativeClose(IntPtr nativeCalendar);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_set_calendar_time_zone")]
    private static partial void NativeSetTimeZone(IntPtr nativeCalendar, IntPtr nativeTimeZone);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_calendar_set")]
    private static partial void NativeSet(
        IntPtr nativeCalendar,
        int year,
        int month,
        int dayOfMonth,
        int hourOfDay,
        int minute,
        int second
    );

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_calendar_get_time")]
    private static partial double NativeGetTime(IntPtr nativeCalendar, out IcuErrorCode errorCode);

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
