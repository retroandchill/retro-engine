// // @file PluralRules.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

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

    public static PluralRules? Create(CultureId locale, PluralType type)
    {
        var rules = NativeOpen(locale, type, out _);
        return rules != IntPtr.Zero ? new PluralRules(rules) : null;
    }

    public string Select(double number)
    {
        Span<char> buffer = stackalloc char[Culture.KeywordAndValuesCapacity];
        var length = NativeSelect(_rules, number, buffer, buffer.Length, out _);
        return length > buffer.Length ? buffer.ToString() : buffer[..length].ToString();
    }

    [LibraryImport(Culture.UnicodeLibName, EntryPoint = "uplrules_openForType")]
    private static partial IntPtr NativeOpen(CultureId locale, PluralType type, out IcuErrorCode errorCode);

    [LibraryImport(Culture.UnicodeLibName, EntryPoint = "uplrules_close")]
    private static partial void NativeClose(IntPtr rules);

    [LibraryImport(Culture.UnicodeLibName, EntryPoint = "uplrules_select")]
    private static partial int NativeSelect(
        IntPtr rules,
        double number,
        Span<char> keyword,
        int capacity,
        out IcuErrorCode errorCode
    );

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
