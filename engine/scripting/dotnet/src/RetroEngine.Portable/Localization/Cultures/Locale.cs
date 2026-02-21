// // @file Locale.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using RetroEngine.Portable.Interop;

namespace RetroEngine.Portable.Localization.Cultures;

public sealed unsafe partial class Locale : IDisposable
{
    internal IntPtr NativeLocale { get; }
    private bool _disposed;
    private readonly bool _ownsNativePtr;

    public bool IsBogus => NativeIsBogus(NativeLocale);

    public string Name => Utf8StringMarshaller.ConvertToManaged(NativeGetName(NativeLocale)) ?? "";

    public string DisplayName
    {
        get
        {
            Span<char> buffer = stackalloc char[Culture.KeywordAndValuesCapacity];
            return NativeGetString(buffer, NativeGetDisplayName);
        }
    }

    public string EnglishName
    {
        get
        {
            Span<char> buffer = stackalloc char[Culture.KeywordAndValuesCapacity];
            return NativeGetString(buffer, NativeGetEnglishName);
        }
    }

    public string ThreeLetterISOLanguageName =>
        Utf8StringMarshaller.ConvertToManaged(NativeGetISO3Language(NativeLocale)) ?? "";
    public string TwoLetterISOLanguageName =>
        Utf8StringMarshaller.ConvertToManaged(NativeGetLanguage(NativeLocale)) ?? "";

    public string Script => Utf8StringMarshaller.ConvertToManaged(NativeGetScript(NativeLocale)) ?? "";
    public string Variant => Utf8StringMarshaller.ConvertToManaged(NativeGetVariant(NativeLocale)) ?? "";
    public string DisplayVariant
    {
        get
        {
            Span<char> buffer = stackalloc char[Culture.KeywordAndValuesCapacity];
            return NativeGetString(buffer, NativeGetDisplayVariant);
        }
    }

    public string DisplayScript
    {
        get
        {
            Span<char> buffer = stackalloc char[Culture.KeywordAndValuesCapacity];
            return NativeGetString(buffer, NativeGetDisplayScript);
        }
    }

    public string DisplayCountry
    {
        get
        {
            Span<char> buffer = stackalloc char[Culture.KeywordAndValuesCapacity];
            return NativeGetString(buffer, NativeGetDisplayCountry);
        }
    }

    public string DisplayLanguage
    {
        get
        {
            Span<char> buffer = stackalloc char[Culture.KeywordAndValuesCapacity];
            return NativeGetString(buffer, NativeGetDisplayLanguage);
        }
    }

    public string Region => Utf8StringMarshaller.ConvertToManaged(NativeGetCountry(NativeLocale)) ?? "";
    public bool IsRightToLeft => NativeIsRightToLeft(NativeLocale);
    public uint LCID => NativeGetLCID(NativeLocale);

    public static Locale Default { get; } = new(NativeGetDefault(), false);

    private Locale(IntPtr nativeLocale, bool ownsNativePtr = true)
    {
        NativeLocale = nativeLocale;
        _ownsNativePtr = ownsNativePtr;
    }

    public Locale()
    {
        NativeLocale = NativeCreateDefault();
        _ownsNativePtr = true;
    }

    ~Locale()
    {
        ReleaseUnmanagedResources();
    }

    public static Locale? Create(string id)
    {
        var nativeLocale = NativeOpen(id);
        return nativeLocale != IntPtr.Zero ? new Locale(nativeLocale) : null;
    }

    private void ReleaseUnmanagedResources()
    {
        if (!_ownsNativePtr)
            return;
        NativeClose(NativeLocale);
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    private string NativeGetString(Span<char> buffer, Func<IntPtr, Span<char>, int, int> func)
    {
        var realLength = func(NativeLocale, buffer, buffer.Length);
        return realLength > buffer.Length ? buffer.ToString() : buffer[..realLength].ToString();
    }

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_get_default_locale")]
    private static partial IntPtr NativeGetDefault();

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_create_default_locale")]
    private static partial IntPtr NativeCreateDefault();

    [LibraryImport(
        NativeLibraries.RetroCore,
        EntryPoint = "retro_create_locale",
        StringMarshalling = StringMarshalling.Utf8
    )]
    private static partial IntPtr NativeOpen(string localeId);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_destroy_locale")]
    private static partial void NativeClose(IntPtr nativeLocale);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_locale_is_bogus")]
    [return: MarshalAs(UnmanagedType.U1)]
    private static partial bool NativeIsBogus(IntPtr nativeLocale);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_locale_get_name")]
    private static partial byte* NativeGetName(IntPtr nativeLocale);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_locale_get_display_name")]
    private static partial int NativeGetDisplayName(IntPtr nativeLocale, Span<char> buffer, int bufferLength);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_locale_get_english_name")]
    private static partial int NativeGetEnglishName(IntPtr nativeLocale, Span<char> buffer, int bufferLength);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_locale_get_three_letter_language_name")]
    private static partial byte* NativeGetISO3Language(IntPtr nativeLocale);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_locale_get_two_letter_language_name")]
    private static partial byte* NativeGetLanguage(IntPtr nativeLocale);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_locale_get_script")]
    private static partial byte* NativeGetScript(IntPtr nativeLocale);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_locale_get_display_script")]
    private static partial int NativeGetDisplayScript(IntPtr nativeLocale, Span<char> buffer, int bufferLength);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_locale_get_region")]
    private static partial byte* NativeGetCountry(IntPtr nativeLocale);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_locale_get_display_region")]
    private static partial int NativeGetDisplayCountry(IntPtr nativeLocale, Span<char> buffer, int bufferLength);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_locale_get_variant")]
    private static partial byte* NativeGetVariant(IntPtr nativeLocale);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_locale_get_display_variant")]
    private static partial int NativeGetDisplayVariant(IntPtr nativeLocale, Span<char> buffer, int bufferLength);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_locale_get_display_language")]
    private static partial int NativeGetDisplayLanguage(IntPtr nativeLocale, Span<char> buffer, int bufferLength);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_locale_is_right_to_left")]
    [return: MarshalAs(UnmanagedType.U1)]
    private static partial bool NativeIsRightToLeft(IntPtr nativeLocale);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_locale_get_lcid")]
    private static partial uint NativeGetLCID(IntPtr nativeLocale);
}
