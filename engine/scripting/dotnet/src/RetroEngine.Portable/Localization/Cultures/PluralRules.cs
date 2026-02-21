// // @file PluralRules.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Portable.Interop;

namespace RetroEngine.Portable.Localization.Cultures;

internal enum PluralType
{
    Cardinal,
    Ordinal,
}

internal sealed partial class PluralRules : IDisposable
{
    private readonly IntPtr _rules;
    private bool _disposed;

    private PluralRules(IntPtr rules) => _rules = rules;

    ~PluralRules()
    {
        ReleaseUnmanagedResources();
    }

    public static PluralRules? Create(Locale locale, PluralType type)
    {
        var rules = NativeOpen(locale.NativeLocale, type);
        return rules != IntPtr.Zero ? new PluralRules(rules) : null;
    }

    public string Select(int number)
    {
        Span<char> buffer = stackalloc char[Culture.KeywordAndValuesCapacity];
        var length = NativeSelect(_rules, number, buffer, buffer.Length);
        return length > buffer.Length ? buffer.ToString() : buffer[..length].ToString();
    }

    public string Select(double number)
    {
        Span<char> buffer = stackalloc char[Culture.KeywordAndValuesCapacity];
        var length = NativeSelect(_rules, number, buffer, buffer.Length);
        return length > buffer.Length ? buffer.ToString() : buffer[..length].ToString();
    }

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_create_plural_rules")]
    private static partial IntPtr NativeOpen(IntPtr locale, PluralType type);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_destroy_plural_rules")]
    private static partial void NativeClose(IntPtr rules);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_plural_rules_select_int32")]
    private static partial int NativeSelect(IntPtr rules, int number, Span<char> keyword, int capacity);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_plural_rules_select_float64")]
    private static partial int NativeSelect(IntPtr rules, double number, Span<char> keyword, int capacity);

    private void ReleaseUnmanagedResources()
    {
        NativeClose(_rules);
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
