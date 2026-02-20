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

    public string DisplayName => new(NativeGetDisplayName(_nativeLocale));

    public string EnglishName => new(NativeGetEnglishName(_nativeLocale));

    public string ThreeLetterISOLanguageName =>
        Utf8StringMarshaller.ConvertToManaged(NativeGetISO3Language(_nativeLocale)) ?? "";
    public string TwoLetterISOLanguageName =>
        Utf8StringMarshaller.ConvertToManaged(NativeGetLanguage(_nativeLocale)) ?? "";

    public string Script => Utf8StringMarshaller.ConvertToManaged(NativeGetScript(_nativeLocale)) ?? "";
    public string Variant => Utf8StringMarshaller.ConvertToManaged(NativeGetVariant(_nativeLocale)) ?? "";
    public string DisplayVariant => new(NativeGetDisplayVariant(_nativeLocale));
    public string DisplayScript => new(NativeGetDisplayScript(_nativeLocale));
    public string DisplayCountry => new(NativeGetDisplayCountry(_nativeLocale));
    public string DisplayLanguage => new(NativeGetDisplayLanguage(_nativeLocale));
    public string Region => Utf8StringMarshaller.ConvertToManaged(NativeGetCountry(_nativeLocale)) ?? "";
    public bool IsRightToLeft => NativeIsRightToLeft(_nativeLocale);
    public int LCID => NativeGetLCID(_nativeLocale);

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
        var nativeLocale = NativeOpen(id);
        return nativeLocale != IntPtr.Zero ? new Locale(id, nativeLocale) : null;
    }

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

    [LibraryImport("retro_core", EntryPoint = "retro_create_locale")]
    private static partial IntPtr NativeOpen(CultureId localeId);

    [LibraryImport("retro_core", EntryPoint = "retro_destroy_locale")]
    private static partial void NativeClose(IntPtr nativeLocale);

    [LibraryImport("retro_core", EntryPoint = "retro_locale_is_bogus")]
    [return: MarshalAs(UnmanagedType.U1)]
    private static partial bool NativeIsBogus(IntPtr nativeLocale);

    [LibraryImport("retro_core", EntryPoint = "retro_locale_get_name")]
    private static partial byte* NativeGetName(IntPtr nativeLocale);

    [LibraryImport("retro_core", EntryPoint = "retro_locale_get_display_name")]
    private static partial char* NativeGetDisplayName(IntPtr nativeLocale);

    [LibraryImport("retro_core", EntryPoint = "retro_locale_get_english_name")]
    private static partial char* NativeGetEnglishName(IntPtr nativeLocale);

    [LibraryImport("retro_core", EntryPoint = "retro_locale_get_three_letter_language_name")]
    private static partial byte* NativeGetISO3Language(IntPtr nativeLocale);

    [LibraryImport("retro_core", EntryPoint = "retro_locale_get_two_letter_language_name")]
    private static partial byte* NativeGetLanguage(IntPtr nativeLocale);

    [LibraryImport("retro_core", EntryPoint = "retro_locale_get_script")]
    private static partial byte* NativeGetScript(IntPtr nativeLocale);

    [LibraryImport("retro_core", EntryPoint = "retro_locale_get_display_script")]
    private static partial char* NativeGetDisplayScript(IntPtr nativeLocale);

    [LibraryImport("retro_core", EntryPoint = "retro_locale_get_region")]
    private static partial byte* NativeGetCountry(IntPtr nativeLocale);

    [LibraryImport("retro_core", EntryPoint = "retro_locale_get_display_region")]
    private static partial char* NativeGetDisplayCountry(IntPtr nativeLocale);

    [LibraryImport("retro_core", EntryPoint = "retro_locale_get_variant")]
    private static partial byte* NativeGetVariant(IntPtr nativeLocale);

    [LibraryImport("retro_core", EntryPoint = "retro_locale_get_display_variant")]
    private static partial char* NativeGetDisplayVariant(IntPtr nativeLocale);

    [LibraryImport("retro_core", EntryPoint = "retro_locale_get_display_language")]
    private static partial char* NativeGetDisplayLanguage(IntPtr nativeLocale);

    [LibraryImport("retro_core", EntryPoint = "retro_locale_is_right_to_left")]
    [return: MarshalAs(UnmanagedType.U1)]
    private static partial bool NativeIsRightToLeft(IntPtr nativeLocale);

    [LibraryImport("retro_core", EntryPoint = "retro_locale_get_lcid")]
    private static partial int NativeGetLCID(IntPtr nativeLocale);
}
