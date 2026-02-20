// // @file FastDecimalFormat.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using RetroEngine.Portable.Utils;

namespace RetroEngine.Portable.Localization.Formatting;

[InlineArray(10)]
public struct DecimalDigits
{
    public char Digit0;
}

public record DecimalNumberFormattingRules
{
    public string NanString { get; init; } = "";
    public string NegativePrefixString { get; init; } = "";
    public string NegativeSuffixString { get; init; } = "";
    public string PositivePrefixString { get; init; } = "";
    public string PositiveSuffixString { get; init; } = "";
    public string PlusString { get; init; } = "";
    public string MinusString { get; init; } = "";
    public char GroupingSeparatorChar { get; init; }
    public char DecimalSeparatorChar { get; init; }
    public byte PrimaryGroupingSize { get; init; }
    public byte SecondaryGroupingSize { get; init; }
    public byte MinimumGroupingDigits { get; init; } = 1;
    public DecimalDigits Digits { get; init; }

    public NumberFormattingOptions DefaultFormattingOptions { get; init; } = new();

    public DecimalNumberFormattingRules()
    {
        var digits = new DecimalDigits();
        digits[0] = '0';
        digits[1] = '1';
        digits[2] = '2';
        digits[3] = '3';
        digits[4] = '4';
        digits[5] = '5';
        digits[6] = '6';
        digits[7] = '7';
        digits[8] = '8';
        digits[9] = '9';
        Digits = digits;
    }
}

[Flags]
internal enum DecimalNumberSingingStringsFlags : byte
{
    None = 0,
    AlwaysSign = 1 << 0,
    UseAsciiSigns = 1 << 1,
}

internal readonly struct DecimalNumberSigningStrings
{
    public string NegativePrefixString { get; }
    public string NegativeSuffixString { get; }
    public string PositivePrefixString { get; }
    public string PositiveSuffixString { get; }

    public DecimalNumberSigningStrings(DecimalNumberFormattingRules rules, DecimalNumberSingingStringsFlags flags)
    {
        var negativePrefix = rules.NegativePrefixString;
        var negativeSuffix = rules.NegativeSuffixString;
        var positivePrefix = rules.PositivePrefixString;
        var positiveSuffix = rules.PositiveSuffixString;

        if (flags.HasFlag(DecimalNumberSingingStringsFlags.AlwaysSign))
        {
            string SynthesizePositiveString(string negativeString, string fallback)
            {
                return negativeString.Contains(rules.MinusString, StringComparison.Ordinal)
                    ? negativeString.Replace(rules.MinusString, rules.PlusString, StringComparison.Ordinal)
                    : fallback;
            }

            positivePrefix = SynthesizePositiveString(negativePrefix, positivePrefix);
            positiveSuffix = SynthesizePositiveString(negativeSuffix, positiveSuffix);
        }

        if (flags.HasFlag(DecimalNumberSingingStringsFlags.UseAsciiSigns))
        {
            var generatedNegativePrefix = negativePrefix.Replace(
                rules.MinusString,
                rules.PlusString,
                StringComparison.Ordinal
            );
            var generatedNegativeSuffix = negativeSuffix.Replace(
                rules.MinusString,
                rules.PlusString,
                StringComparison.Ordinal
            );
            var generatedPositivePrefix = positivePrefix.Replace(
                rules.PlusString,
                rules.MinusString,
                StringComparison.Ordinal
            );
            var generatedPositiveSuffix = positiveSuffix.Replace(
                rules.PlusString,
                rules.MinusString,
                StringComparison.Ordinal
            );

            if (!string.IsNullOrEmpty(rules.NegativePrefixString))
            {
                negativePrefix = generatedNegativePrefix;
            }

            if (!string.IsNullOrEmpty(rules.NegativeSuffixString))
            {
                negativeSuffix = generatedNegativeSuffix;
            }

            if (!string.IsNullOrEmpty(rules.PositivePrefixString))
            {
                positivePrefix = generatedPositivePrefix;
            }

            if (!string.IsNullOrEmpty(rules.PositiveSuffixString))
            {
                positiveSuffix = generatedPositiveSuffix;
            }
        }

        NegativePrefixString = negativePrefix;
        NegativeSuffixString = negativeSuffix;
        PositivePrefixString = positivePrefix;
        PositiveSuffixString = positiveSuffix;
    }
}

public static class FastDecimalFormat
{
    private const int MaxIntegralPrintLength = 20;
    private const int MaxFractionalPrintLength = 18;
    private const int MinRequiredIntegralBufferSize = MaxIntegralPrintLength * 2 + 1;

    private static readonly ImmutableArray<ulong> Pow10Table =
    [
        1, // 10^0
        10, // 10^1
        100, // 10^2
        1000, // 10^3
        10000, // 10^4
        100000, // 10^5
        1000000, // 10^6
        10000000, // 10^7
        100000000, // 10^8
        1000000000, // 10^9
        10000000000, // 10^10
        100000000000, // 10^11
        1000000000000, // 10^12
        10000000000000, // 10^13
        100000000000000, // 10^14
        1000000000000000, // 10^15
        10000000000000000, // 10^16
        100000000000000000, // 10^17
        1000000000000000000, // 10^18
    ];

    private static NumberFormattingOptions SanitizeNumberFormattingOptions(NumberFormattingOptions options)
    {
        var minimumIntegral = Math.Max(0, options.MinimumIntegralDigits);
        var minimumFractional = Math.Max(0, options.MinimumFractionalDigits);

        var maximumIntegral = Math.Max(minimumIntegral, options.MaximumIntegralDigits);
        var maximumFractional = Math.Max(minimumFractional, options.MaximumFractionalDigits);

        if (
            options.MinimumIntegralDigits != minimumIntegral
            || options.MaximumIntegralDigits != maximumIntegral
            || options.MinimumFractionalDigits != minimumFractional
            || options.MaximumFractionalDigits != maximumFractional
        )
        {
            return options with
            {
                MinimumIntegralDigits = minimumIntegral,
                MaximumIntegralDigits = maximumIntegral,
                MinimumFractionalDigits = minimumFractional,
                MaximumFractionalDigits = maximumFractional,
            };
        }

        return options;
    }

    private static Span<char> IntegralToStringULongToString(
        ulong value,
        bool useGrouping,
        byte primaryGroupingSize,
        byte secondaryGroupingSize,
        byte minimumGroupingDigits,
        char groupingSeparatorChar,
        DecimalDigits digitCharacters,
        int minDigitsToPrint,
        int maxDigitsToPrint,
        Span<char> buffer
    )
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(buffer.Length, MinRequiredIntegralBufferSize);

        Span<char> tempBuffer = stackalloc char[MinRequiredIntegralBufferSize];
        var stringLength = 0;

        var digitsPrinted = 0;
        var numUntilNextGroup = primaryGroupingSize;

        if (value > 0)
        {
            var tempNum = value;
            while (digitsPrinted < maxDigitsToPrint && tempNum != 0)
            {
                if (useGrouping && numUntilNextGroup-- == 0)
                {
                    tempBuffer[stringLength++] = groupingSeparatorChar;
                    numUntilNextGroup = secondaryGroupingSize;
                }

                tempBuffer[stringLength++] = digitCharacters[(int)(tempNum % 10)];
                tempNum /= 10;

                ++digitsPrinted;
            }
        }

        var paddingToApply = Math.Min(minDigitsToPrint - digitsPrinted, MaxIntegralPrintLength - digitsPrinted);
        for (var paddingIndex = 0; paddingIndex < paddingToApply; ++paddingIndex)
        {
            if (useGrouping && numUntilNextGroup-- == 0)
            {
                tempBuffer[stringLength++] = groupingSeparatorChar;
                numUntilNextGroup = secondaryGroupingSize;
            }

            tempBuffer[stringLength++] = digitCharacters[0];
            ++digitsPrinted;
        }

        if (
            useGrouping
            && digitsPrinted > primaryGroupingSize
            && digitsPrinted < (primaryGroupingSize + minimumGroupingDigits)
        )
        {
            tempBuffer = tempBuffer[1..];
            --stringLength;
        }

        for (var finalBufferIndex = 0; finalBufferIndex < stringLength; ++finalBufferIndex)
        {
            buffer[finalBufferIndex] = tempBuffer[stringLength - finalBufferIndex - 1];
        }

        return buffer[..stringLength];
    }

    private static Span<char> IntegralToStringCommon(
        ulong value,
        DecimalNumberFormattingRules rules,
        NumberFormattingOptions formattingOptions,
        Span<char> buffer
    )
    {
        return IntegralToStringULongToString(
            value,
            formattingOptions.UseGrouping && rules.PrimaryGroupingSize > 0,
            rules.PrimaryGroupingSize,
            rules.SecondaryGroupingSize,
            rules.MinimumGroupingDigits,
            rules.GroupingSeparatorChar,
            rules.Digits,
            formattingOptions.MinimumIntegralDigits,
            formattingOptions.MaximumIntegralDigits,
            buffer
        );
    }

    private static (double IntegralPar, double FractionalPart) FractionalToStringSplitRoundNumber(
        bool isNegative,
        double value,
        int numDecimalPlaces,
        RoundingMode roundingMode
    )
    {
        var decimalPlacesToRoundTo = Math.Min(numDecimalPlaces, MaxFractionalPrintLength);

        var isRoundingEntireNumber = decimalPlacesToRoundTo == 0;

        var integralPart = value;
        double fractionalPart;
        if (isRoundingEntireNumber)
        {
            fractionalPart = 0;
        }
        else
        {
            integralPart = Math.Truncate(value);
            fractionalPart = value - integralPart;
        }

        ref var valueToRound = ref isRoundingEntireNumber ? ref integralPart : ref fractionalPart;
        valueToRound = Math.TruncateToHalfIfClose(valueToRound * Pow10Table[decimalPlacesToRoundTo]);

        valueToRound = roundingMode switch
        {
            RoundingMode.HalfToEven => Math.Round(valueToRound, MidpointRounding.ToEven),
            RoundingMode.HalfFromZero => Math.RoundHalfFromZero(valueToRound),
            RoundingMode.HalfToZero => Math.RoundHalfToZero(valueToRound),
            RoundingMode.FromZero => Math.Round(valueToRound, MidpointRounding.AwayFromZero),
            RoundingMode.ToZero => Math.Round(valueToRound, MidpointRounding.ToZero),
            RoundingMode.ToNegativeInfinity => Math.Round(valueToRound, MidpointRounding.ToNegativeInfinity),
            RoundingMode.ToPositiveInfinity => Math.Round(valueToRound, MidpointRounding.ToPositiveInfinity),
            _ => throw new ArgumentOutOfRangeException(nameof(roundingMode), roundingMode, null),
        };

        if (isRoundingEntireNumber)
        {
            return (integralPart, 0);
        }

        var valueOverflowTest = isNegative ? -valueToRound : valueToRound;
        if (!(valueOverflowTest > Pow10Table[decimalPlacesToRoundTo]))
            return (integralPart, valueToRound);

        if (isNegative)
        {
            integralPart--;
            valueToRound += Pow10Table[decimalPlacesToRoundTo];
        }
        else
        {
            integralPart++;
            valueToRound -= Pow10Table[decimalPlacesToRoundTo];
        }

        return (integralPart, valueToRound);
    }

    private static void BuildFinalString(
        bool isNegative,
        bool alwaysSign,
        DecimalNumberFormattingRules rules,
        ReadOnlySpan<char> integralPart,
        ReadOnlySpan<char> fractionalPart,
        StringBuilder buffer
    )
    {
        var signingStrings = new DecimalNumberSigningStrings(
            rules,
            alwaysSign ? DecimalNumberSingingStringsFlags.AlwaysSign : DecimalNumberSingingStringsFlags.None
        );
        var finalPrefix = isNegative ? signingStrings.NegativePrefixString : signingStrings.PositivePrefixString;
        var finalSuffix = isNegative ? signingStrings.NegativeSuffixString : signingStrings.PositiveSuffixString;

        buffer.EnsureCapacity(
            buffer.Length + finalPrefix.Length + integralPart.Length + 1 + fractionalPart.Length + finalSuffix.Length
        );

        buffer.Append(finalPrefix);
        buffer.Append(integralPart);
        if (fractionalPart.Length > 0)
        {
            buffer.Append(rules.DecimalSeparatorChar);
            buffer.Append(fractionalPart);
        }
        buffer.Append(finalSuffix);
    }

    private static void IntegralToString(
        bool isNegative,
        ulong value,
        DecimalNumberFormattingRules rules,
        NumberFormattingOptions formattingOptions,
        StringBuilder buffer
    )
    {
        formattingOptions = SanitizeNumberFormattingOptions(formattingOptions);

        Span<char> integralPartBuffer = stackalloc char[MinRequiredIntegralBufferSize];
        var integralPart = IntegralToStringCommon(value, rules, formattingOptions, integralPartBuffer);

        Span<char> fractionalPartBuffer = stackalloc char[MinRequiredIntegralBufferSize];
        var fractionalPartLength = 0;
        if (formattingOptions.MinimumFractionalDigits > 0)
        {
            var paddingToApply = Math.Min(formattingOptions.MinimumFractionalDigits, MaxFractionalPrintLength);
            for (var paddingIndex = 0; paddingIndex < paddingToApply; ++paddingIndex)
            {
                fractionalPartBuffer[fractionalPartLength++] = rules.Digits[0];
            }
        }

        var fractionalSpan = fractionalPartBuffer[..fractionalPartLength];
        BuildFinalString(isNegative, formattingOptions.AlwaysSign, rules, integralPart, fractionalSpan, buffer);
    }

    private static void FractionalToString(
        double value,
        DecimalNumberFormattingRules rules,
        NumberFormattingOptions formattingOptions,
        StringBuilder buffer
    )
    {
        formattingOptions = SanitizeNumberFormattingOptions(formattingOptions);

        if (double.IsNaN(value))
        {
            buffer.Append(rules.NanString);
            return;
        }

        var isNegative = double.IsNegative(value);

        var (integralPart, fractionalPart) = FractionalToStringSplitRoundNumber(
            isNegative,
            value,
            formattingOptions.MaximumFractionalDigits,
            formattingOptions.RoundingMode
        );

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        var nearlyInteger =
            formattingOptions.IndicateNearlyInteger && Math.IsNearlyEqual(integralPart, value) && integralPart != value;

        if (isNegative)
        {
            integralPart = -integralPart;
            fractionalPart = -fractionalPart;
        }

        var intIntegralPart = (ulong)integralPart;
        Span<char> integralPartBuffer = stackalloc char[MinRequiredIntegralBufferSize];
        var integralSpan = IntegralToStringCommon(intIntegralPart, rules, formattingOptions, integralPartBuffer);

        Span<char> fractionalPartBuffer = stackalloc char[MinRequiredIntegralBufferSize];
        var fractionalPartLength = 0;
        if (fractionalPart != 0)
        {
            var splitSection = IntegralToStringULongToString(
                (ulong)fractionalPart,
                false,
                0,
                0,
                1,
                ' ',
                rules.Digits,
                0,
                formattingOptions.MaximumFractionalDigits,
                fractionalPartBuffer
            );
            fractionalPartLength = splitSection.Length;
            var leadingZerosToAdd = Math.Min(
                formattingOptions.MaximumFractionalDigits - fractionalPartLength,
                MaxFractionalPrintLength - fractionalPartLength
            );
            if (leadingZerosToAdd > 0)
            {
                for (var i = 0; i < fractionalPartLength; ++i)
                {
                    fractionalPartBuffer[i + leadingZerosToAdd] = fractionalPartBuffer[i];
                }

                for (var i = 0; i < leadingZerosToAdd; ++i)
                {
                    fractionalPartBuffer[i] = rules.Digits[0];
                }

                fractionalPartLength += leadingZerosToAdd;
            }
        }

        var paddingToApply = Math.Min(
            formattingOptions.MinimumFractionalDigits - fractionalPartLength,
            MaxFractionalPrintLength - fractionalPartLength
        );
        for (var paddingIndex = 0; paddingIndex < paddingToApply; ++paddingIndex)
        {
            fractionalPartBuffer[fractionalPartLength++] = rules.Digits[0];
        }

        if (nearlyInteger && fractionalPartLength + 3 < fractionalPartBuffer.Length)
        {
            fractionalPartBuffer[fractionalPartLength++] = '.';
            fractionalPartBuffer[fractionalPartLength++] = '.';
            fractionalPartBuffer[fractionalPartLength++] = '.';
        }

        var fractionalSpan = fractionalPartBuffer[..fractionalPartLength];
        BuildFinalString(isNegative, formattingOptions.AlwaysSign, rules, integralSpan, fractionalSpan, buffer);
    }

    private static void NumberToString<T>(
        T value,
        DecimalNumberFormattingRules rules,
        NumberFormattingOptions formattingOptions,
        StringBuilder builder
    )
        where T : unmanaged, INumber<T>
    {
        if (
            typeof(sbyte) == typeof(T)
            || typeof(short) == typeof(T)
            || typeof(int) == typeof(T)
            || typeof(long) == typeof(T)
        )
        {
            var isNegative = T.IsNegative(value);
            var magnitude = T.Abs(value);

            IntegralToString(isNegative, ulong.CreateChecked(magnitude), rules, formattingOptions, builder);
        }
        else if (
            typeof(byte) == typeof(T)
            || typeof(ushort) == typeof(T)
            || typeof(uint) == typeof(T)
            || typeof(ulong) == typeof(T)
        )
        {
            IntegralToString(false, ulong.CreateChecked(value), rules, formattingOptions, builder);
        }
        else if (typeof(float) == typeof(T) || typeof(double) == typeof(T))
        {
            FractionalToString(double.CreateChecked(value), rules, formattingOptions, builder);
        }
        else
        {
            throw new ArgumentException($"Type {typeof(T)} is not supported.", nameof(value));
        }
    }

    public static string NumberToString<T>(
        T value,
        DecimalNumberFormattingRules rules,
        NumberFormattingOptions formattingOptions
    )
        where T : unmanaged, INumber<T>
    {
        var builder = new StringBuilder();
        NumberToString(value, rules, formattingOptions, builder);
        return builder.ToString();
    }
}
