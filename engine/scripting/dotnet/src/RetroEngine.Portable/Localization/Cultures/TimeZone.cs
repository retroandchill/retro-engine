// // @file TimeZone.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Portable.Interop;

namespace RetroEngine.Portable.Localization.Cultures;

internal sealed partial class TimeZone : IDisposable
{
    public IntPtr NativePtr { get; }
    private readonly bool _ownsNativePtr;
    private bool _disposed;
    public bool IsReadOnly => !_ownsNativePtr;

    public string Id
    {
        get
        {
            Span<char> buffer = stackalloc char[Culture.KeywordAndValuesCapacity];
            var length = NativeGetId(NativePtr, buffer, buffer.Length);
            return length > buffer.Length ? buffer.ToString() : buffer[..length].ToString();
        }
    }

    public static TimeZone Unknown { get; } = new(NativeGetUnknown(), false);

    public static TimeZone Local { get; } = new(TimeZoneInfo.Local.Id);

    public TimeZone()
        : this(NativeCreateDefault()) { }

    public TimeZone(ReadOnlySpan<char> id)
        : this(NativeCreate(id, id.Length)) { }

    private TimeZone(IntPtr nativePtr, bool ownsNativePtr = true)
    {
        NativePtr = nativePtr;
        _ownsNativePtr = ownsNativePtr;
    }

    ~TimeZone()
    {
        ReleaseUnmanagedResources();
    }

    public static string GetCanonicalId(ReadOnlySpan<char> id)
    {
        Span<char> buffer = stackalloc char[Culture.KeywordAndValuesCapacity];
        var length = NativeGetCanonicalId(id, id.Length, buffer, buffer.Length);
        return length > buffer.Length ? buffer.ToString() : buffer[..length].ToString();
    }

    private void ReleaseUnmanagedResources()
    {
        if (!_ownsNativePtr)
            return;
        NativeDestroy(NativePtr);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_get_unknown_time_zone")]
    private static partial IntPtr NativeGetUnknown();

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_create_default_time_zone")]
    private static partial IntPtr NativeCreateDefault();

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_create_time_zone")]
    private static partial IntPtr NativeCreate(ReadOnlySpan<char> id, int length);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_destroy_time_zone")]
    private static partial void NativeDestroy(IntPtr nativePtr);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_time_zone_get_canonical_id")]
    private static partial int NativeGetCanonicalId(
        ReadOnlySpan<char> timeZoneId,
        int timeZoneIdLength,
        Span<char> result,
        int resultLength
    );

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_time_zone_get_id")]
    private static partial int NativeGetId(IntPtr nativePtr, Span<char> result, int resultLength);
}
