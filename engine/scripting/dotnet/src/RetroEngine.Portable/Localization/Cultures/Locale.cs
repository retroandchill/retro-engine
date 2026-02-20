// // @file Locale.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace RetroEngine.Portable.Localization.Cultures;

public sealed partial class Locale : IDisposable
{
    public CultureId Id { get; }
    private readonly IntPtr _nativeLocale;
    private bool _disposed;

    public bool IsBogus => NativeIsBogus(_nativeLocale);

    private Locale(CultureId id, IntPtr nativeLocale)
    {
        Id = id;
        _nativeLocale = nativeLocale;
    }

    ~Locale()
    {
        ReleaseUnmanagedResources();
    }

    public static Locale? Create(CultureId id)
    {
        var nativeLocale = NativeOpen(id.Name, id.Name.Length, out _);
        return nativeLocale != IntPtr.Zero ? new Locale(id, nativeLocale) : null;
    }

    [LibraryImport(
        Culture.UnicodeLibName,
        EntryPoint = "ulocale_openForLocaleID",
        StringMarshalling = StringMarshalling.Utf8
    )]
    private static partial IntPtr NativeOpen(string localeId, int localeIdLength, out IcuErrorCode errorCode);

    [LibraryImport(Culture.UnicodeLibName, EntryPoint = "ulocale_close")]
    private static partial void NativeClose(IntPtr nativeLocale);

    [LibraryImport(Culture.UnicodeLibName, EntryPoint = "ulocale_isBogus")]
    [return: MarshalAs(UnmanagedType.U1)]
    private static partial bool NativeIsBogus(IntPtr nativeLocale);

    private void ReleaseUnmanagedResources()
    {
        NativeClose(_nativeLocale);
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
