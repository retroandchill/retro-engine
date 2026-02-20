// // @file DecimalFormat.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace RetroEngine.Portable.Localization.Cultures;

internal sealed partial class DecimalFormat : IDisposable
{
    public IntPtr NativeDecimalFormat { get; }
    private bool _disposed;

    public bool IsGroupingUsed
    {
        get => NativeGetAttribute(NativeDecimalFormat, NumberFormatAttribute.GroupingUsed) != 0;
        set => NativeSetAttribute(NativeDecimalFormat, NumberFormatAttribute.GroupingUsed, value ? 1 : 0);
    }

    public NumberFormatRoundingMode RoundingMode =>
        (NumberFormatRoundingMode)NativeGetAttribute(NativeDecimalFormat, NumberFormatAttribute.RoundingMode);
    public int MinimumIntegerDigits => NativeGetAttribute(NativeDecimalFormat, NumberFormatAttribute.MinIntegerDigits);
    public int MaximumIntegerDigits => NativeGetAttribute(NativeDecimalFormat, NumberFormatAttribute.MaxIntegerDigits);
    public int MinimumFractionDigits =>
        NativeGetAttribute(NativeDecimalFormat, NumberFormatAttribute.MinFractionDigits);
    public int MaximumFractionDigits =>
        NativeGetAttribute(NativeDecimalFormat, NumberFormatAttribute.MaxFractionDigits);
    public int GroupingSize => NativeGetAttribute(NativeDecimalFormat, NumberFormatAttribute.GroupingSize);
    public int SecondaryGroupingSize =>
        NativeGetAttribute(NativeDecimalFormat, NumberFormatAttribute.SecondaryGroupingSize);

    private DecimalFormat(IntPtr nativeDecimalFormat)
    {
        NativeDecimalFormat = nativeDecimalFormat;
    }

    ~DecimalFormat()
    {
        ReleaseUnmanagedResources();
    }

    public static DecimalFormat? CreateInstance(CultureId locale)
    {
        return Create(NumberFormatStyle.Default, locale);
    }

    public static DecimalFormat? CreatePercentInstance(CultureId locale)
    {
        return Create(NumberFormatStyle.Percent, locale);
    }

    public static DecimalFormat? CreateCurrencyInstance(CultureId locale)
    {
        return Create(NumberFormatStyle.Currency, locale);
    }

    private static DecimalFormat? Create(NumberFormatStyle style, CultureId locale)
    {
        var format = NativeOpen(style, string.Empty, 0, locale, IntPtr.Zero, out var errorCode);
        return format != IntPtr.Zero ? new DecimalFormat(format) : null;
    }

    public string GetTextAttribute(NumberFormatTextAttribute attribute)
    {
        Span<char> buffer = stackalloc char[Culture.KeywordAndValuesCapacity];
        var length = NativeGetTextAttribute(NativeDecimalFormat, attribute, buffer, buffer.Length, out _);
        return length > buffer.Length ? buffer.ToString() : buffer[..length].ToString();
    }

    public void SetTextAttribute(NumberFormatTextAttribute attribute, string? value)
    {
        NativeSetTextAttribute(NativeDecimalFormat, attribute, value, value?.Length ?? 0, out _);
    }

    public string GetSymbol(NumberFormatSymbol symbol)
    {
        Span<char> buffer = stackalloc char[Culture.KeywordAndValuesCapacity];
        var length = NativeGetSymbol(NativeDecimalFormat, symbol, buffer, buffer.Length, out _);
        return length > buffer.Length ? buffer.ToString() : buffer[..length].ToString();
    }

    public void SetSymbol(NumberFormatSymbol symbol, ReadOnlySpan<char> value)
    {
        NativeSetSymbol(NativeDecimalFormat, symbol, value, value.Length, out _);
    }

    [LibraryImport(Culture.UnicodeLibName, EntryPoint = "unum_open")]
    private static partial IntPtr NativeOpen(
        NumberFormatStyle style,
        ReadOnlySpan<char> pattern,
        int patternLength,
        CultureId locale,
        IntPtr parseError,
        out IcuErrorCode errorCode
    );

    [LibraryImport(Culture.UnicodeLibName, EntryPoint = "unum_close")]
    private static partial void NativeClose(IntPtr nativeDecimalFormat);

    [LibraryImport(Culture.UnicodeLibName, EntryPoint = "unum_getAttribute")]
    private static partial int NativeGetAttribute(IntPtr nativeDecimalFormat, NumberFormatAttribute attribute);

    [LibraryImport(Culture.UnicodeLibName, EntryPoint = "unum_setAttribute")]
    private static partial void NativeSetAttribute(
        IntPtr nativeDecimalFormat,
        NumberFormatAttribute attribute,
        int value
    );

    [LibraryImport(Culture.UnicodeLibName, EntryPoint = "unum_getTextAttribute")]
    private static partial int NativeGetTextAttribute(
        IntPtr nativeDecimalFormat,
        NumberFormatTextAttribute attribute,
        Span<char> result,
        int maxResultSize,
        out IcuErrorCode errorCode
    );

    [LibraryImport(Culture.UnicodeLibName, EntryPoint = "unum_setTextAttribute")]
    private static partial void NativeSetTextAttribute(
        IntPtr nativeDecimalFormat,
        NumberFormatTextAttribute attribute,
        ReadOnlySpan<char> value,
        int valueLength,
        out IcuErrorCode errorCode
    );

    [LibraryImport(Culture.UnicodeLibName, EntryPoint = "unum_getSymbol")]
    private static partial int NativeGetSymbol(
        IntPtr nativeDecimalFormat,
        NumberFormatSymbol symbol,
        Span<char> result,
        int maxResultSize,
        out IcuErrorCode errorCode
    );

    [LibraryImport(Culture.UnicodeLibName, EntryPoint = "unum_setSymbol")]
    private static partial void NativeSetSymbol(
        IntPtr nativeDecimalFormat,
        NumberFormatSymbol symbol,
        ReadOnlySpan<char> value,
        int valueLength,
        out IcuErrorCode errorCode
    );

    private void ReleaseUnmanagedResources()
    {
        NativeClose(NativeDecimalFormat);
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
