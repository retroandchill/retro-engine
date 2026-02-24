// // @file DecimalFormat.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Portable.Interop;
using RetroEngine.Portable.Localization.Formatting;

namespace RetroEngine.Portable.Localization.Cultures;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct NativeDecimalDigits
{
    public const int DigitsCapacity = 10;

    public fixed char Digits[DigitsCapacity];
}

internal readonly unsafe ref struct NativeDecimalSymbol(char* buffer, int length)
{
    private ReadOnlySpan<char> AsSpan() => new(buffer, length);

    public override string ToString()
    {
        return AsSpan().ToString();
    }
}

[StructLayout(LayoutKind.Sequential)]
internal ref struct NativeDecimalNumberFormattingRules
{
    public sbyte IsGroupingUsed;
    public NumberFormatRoundingMode RoundingMode;
    public int MinimumIntegerDigits;
    public int MaximumIntegerDigits;
    public int MinimumFractionDigits;
    public int MaximumFractionDigits;
    public NativeDecimalSymbol NanString;
    public NativeDecimalSymbol PlusSign;
    public NativeDecimalSymbol MinusSign;
    public char GroupingSeperator;
    public char DecimalSeparator;
    public int GroupingSize;
    public int SecondaryGroupingSize;
    public int MinimumGroupingDigits;
    public NativeDecimalDigits Digits;
}

internal readonly record struct DecimalFormatPrefixAndSuffix(
    string PositivePrefix,
    string PositiveSuffix,
    string NegativePrefix,
    string NegativeSuffix
);

internal enum NumberFormatRoundingMode
{
    RoundCeiling,
    RoundFloor,
    RoundDown,
    RoundUp,
    RoundHalfEven,
    RoundHalfDown = RoundHalfEven + 1,
    RoundHalfUp,
    RoundUnnecessary,
    RoundHalfOdd,
    RoundHalfCeiling,
    RoundHalfFloor,
}

internal static class NumberRoundingModeExtensions
{
    public static RoundingMode ToRoundingMode(this NumberFormatRoundingMode mode)
    {
        return mode switch
        {
            NumberFormatRoundingMode.RoundHalfEven => RoundingMode.HalfToEven,
            NumberFormatRoundingMode.RoundHalfUp => RoundingMode.HalfFromZero,
            NumberFormatRoundingMode.RoundHalfDown => RoundingMode.HalfToZero,
            NumberFormatRoundingMode.RoundUp => RoundingMode.FromZero,
            NumberFormatRoundingMode.RoundDown => RoundingMode.ToZero,
            NumberFormatRoundingMode.RoundFloor => RoundingMode.ToNegativeInfinity,
            NumberFormatRoundingMode.RoundCeiling => RoundingMode.ToPositiveInfinity,
            _ => RoundingMode.HalfToEven,
        };
    }
}

internal sealed partial class DecimalFormat : IDisposable
{
    public IntPtr NativeDecimalFormat { get; }
    private bool _disposed;

    public bool IsGroupingUsed
    {
        set => NativeSetGroupingUsed(NativeDecimalFormat, value);
    }

    public string? CurrencyCode
    {
        set => NativeSetCurrencyCode(NativeDecimalFormat, value);
    }

    public NativeDecimalDigits Digits
    {
        set => NativeSetDigits(NativeDecimalFormat, in value);
    }

    public NativeDecimalNumberFormattingRules FormattingRules => NativeGetFormattingRules(NativeDecimalFormat);

    public DecimalFormatPrefixAndSuffix PrefixAndSuffix
    {
        get
        {
            Span<char> positivePrefix = stackalloc char[Culture.KeywordAndValuesCapacity];
            Span<char> positiveSuffix = stackalloc char[Culture.KeywordAndValuesCapacity];
            Span<char> negativePrefix = stackalloc char[Culture.KeywordAndValuesCapacity];
            Span<char> negativeSuffix = stackalloc char[Culture.KeywordAndValuesCapacity];
            var (posPreLen, posSufLen, negPreLen, negSufLen) = NativeGetPrefixAndSuffix(
                NativeDecimalFormat,
                positivePrefix,
                positivePrefix.Length,
                positiveSuffix,
                positiveSuffix.Length,
                negativePrefix,
                negativePrefix.Length,
                negativeSuffix,
                negativeSuffix.Length
            );
            return new DecimalFormatPrefixAndSuffix(
                posPreLen >= positivePrefix.Length ? positivePrefix.ToString() : positivePrefix[..posPreLen].ToString(),
                posSufLen >= positiveSuffix.Length ? positiveSuffix.ToString() : positiveSuffix[..posSufLen].ToString(),
                negPreLen >= negativePrefix.Length ? negativePrefix.ToString() : negativePrefix[..negPreLen].ToString(),
                negSufLen >= negativeSuffix.Length ? negativeSuffix.ToString() : negativeSuffix[..negSufLen].ToString()
            );
        }
    }

    private DecimalFormat(IntPtr nativeDecimalFormat)
    {
        NativeDecimalFormat = nativeDecimalFormat;
    }

    ~DecimalFormat()
    {
        ReleaseUnmanagedResources();
    }

    public static DecimalFormat? CreateInstance(Locale locale)
    {
        return Create(locale, NativeCreate);
    }

    public static DecimalFormat? CreatePercentInstance(Locale locale)
    {
        return Create(locale, NativeCreatePercent);
    }

    public static DecimalFormat? CreateCurrencyInstance(Locale locale)
    {
        return Create(locale, NativeCreateCurrency);
    }

    private static DecimalFormat? Create(Locale locale, Func<IntPtr, IntPtr> nativeCreate)
    {
        var format = nativeCreate(locale.NativeLocale);
        return format != IntPtr.Zero ? new DecimalFormat(format) : null;
    }

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_create_decimal_format")]
    private static partial IntPtr NativeCreate(IntPtr locale);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_create_percent_decimal_format")]
    private static partial IntPtr NativeCreatePercent(IntPtr locale);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_create_currency_decimal_format")]
    private static partial IntPtr NativeCreateCurrency(IntPtr locale);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_destroy_decimal_format")]
    private static partial void NativeDestroy(IntPtr nativeDecimalFormat);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_decimal_format_get_formatting_rules")]
    private static partial NativeDecimalNumberFormattingRules NativeGetFormattingRules(IntPtr nativeDecimalFormat);

    [StructLayout(LayoutKind.Sequential)]
    internal readonly record struct DecimalFormatPrefixAndSuffixResult(
        int PositivePrefixLength,
        int PositiveSuffixLength,
        int NegativePrefixLength,
        int NegativeSuffixLength
    );

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_decimal_format_get_prefix_and_suffix_lengths")]
    private static partial DecimalFormatPrefixAndSuffixResult NativeGetPrefixAndSuffix(
        IntPtr nativeDecimalFormat,
        Span<char> positivePrefix,
        int positivePrefixCapacity,
        Span<char> positiveSuffix,
        int positiveSuffixCapacity,
        Span<char> negativePrefix,
        int negativePrefixCapacity,
        Span<char> negativeSuffix,
        int negativeSuffixCapacity
    );

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_decimal_format_set_is_grouping_used")]
    private static partial void NativeSetGroupingUsed(
        IntPtr nativeDecimalFormat,
        [MarshalAs(UnmanagedType.I1)] bool isGroupingUsed
    );

    [LibraryImport(
        NativeLibraries.RetroCore,
        EntryPoint = "retro_decimal_format_set_currency_code",
        StringMarshalling = StringMarshalling.Utf16
    )]
    private static partial void NativeSetCurrencyCode(IntPtr nativeDecimalFormat, string? isGroupingUsed);

    [LibraryImport(NativeLibraries.RetroCore, EntryPoint = "retro_decimal_format_set_digits")]
    private static partial void NativeSetDigits(IntPtr nativeDecimalFormat, in NativeDecimalDigits digits);

    private void ReleaseUnmanagedResources()
    {
        NativeDestroy(NativeDecimalFormat);
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
