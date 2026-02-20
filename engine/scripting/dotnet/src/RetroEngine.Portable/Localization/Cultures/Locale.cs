// // @file Locale.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace RetroEngine.Portable.Localization.Cultures;

public sealed unsafe partial class Locale : IDisposable
{
    public CultureId Id { get; }
    private readonly IntPtr _nativeLocale;
    private bool _disposed;

    public bool IsBogus => NativeIsBogus(_nativeLocale);

    public string Name => Utf8StringMarshaller.ConvertToManaged(NativeGetName(_nativeLocale)) ?? "";

    public string DisplayName => new string(NativeGetDisplayName(_nativeLocale));

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
        EntryPoint = "retro_create_locale",
        StringMarshalling = StringMarshalling.Utf8
    )]
    private static partial IntPtr NativeOpen(string localeId);

    [LibraryImport(Culture.UnicodeLibName, EntryPoint = "retro_destroy_locale")]
    private static partial void NativeClose(IntPtr nativeLocale);

    [LibraryImport(Culture.UnicodeLibName, EntryPoint = "retro_locale_is_bogus")]
    [return: MarshalAs(UnmanagedType.U1)]
    private static partial bool NativeIsBogus(IntPtr nativeLocale);

    [LibraryImport(Culture.UnicodeLibName, EntryPoint = "retro_locale_get_name")]
    private static partial byte* NativeGetName(IntPtr nativeLocale);

    [LibraryImport(Culture.UnicodeLibName, EntryPoint = "retro_locale_get_display_name")]
    private static partial char* NativeGetDisplayName(IntPtr nativeLocale);

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
