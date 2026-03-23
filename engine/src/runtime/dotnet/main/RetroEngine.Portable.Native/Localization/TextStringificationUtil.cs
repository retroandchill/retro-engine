// // @file TextStringificationUtil.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using LinkDotNet.StringBuilder;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Portable.Localization.Formatting;
using RetroEngine.Portable.Utils;

namespace RetroEngine.Portable.Localization;

internal static class TextStringificationUtil
{
    public const string TextMarker = "TEXT";
    public const string InvTextMarker = "INVTEXT";
    public const string NsLocTextMarker = "NSLOCTEXT";
    public const string LocTextMarker = "LOCTEXT";
    public const string LocTableMarker = "LOCTABLE";
    public const string LocGenNumberMarker = "LOCGEN_NUMBER";
    public const string LocGenPercentMarker = "LOCGEN_PERCENT";
    public const string LocGenCurrencyMarker = "LOCGEN_CURRENCY";
    public const string LocGenDateMarker = "LOCGEN_DATE";
    public const string LocGenTimeMarker = "LOCGEN_TIME";
    public const string LocGenDateTimeMarker = "LOCGEN_DATETIME";
    public const string LocGenToLowerMarker = "LOCGEN_TOLOWER";
    public const string LocGenToUpperMarker = "LOCGEN_TOUPPER";
    public const string LocGenFormatOrderedMarker = "LOCGEN_FORMAT_ORDERED";
    public const string LocGenFormatNamedMarker = "LOCGEN_FORMAT_NAMED";
    public const string GroupedSuffix = "_GROUPED";
    public const string UngroupedSuffix = "_UNGROUPED";
    public const string CustomSuffix = "_CUSTOM";
    public const string UtcSuffix = "_UTC";
    public const string LocalSuffix = "_LOCAL";

    extension(ReadOnlySpan<char> buffer)
    {
        public bool PeekMarker(ReadOnlySpan<char> marker)
        {
            return buffer.StartsWith(marker);
        }

        public bool PeekInsensitiveMarker(ReadOnlySpan<char> marker)
        {
            return buffer.StartsWith(marker, StringComparison.OrdinalIgnoreCase);
        }

        public bool SkipMarker(ReadOnlySpan<char> marker, out ReadOnlySpan<char> remaining)
        {
            if (buffer.PeekMarker(marker))
            {
                remaining = buffer[marker.Length..];
                return true;
            }

            remaining = default;
            return false;
        }

        public bool SkipInsensitiveMarker(ReadOnlySpan<char> marker, out ReadOnlySpan<char> remaining)
        {
            if (buffer.PeekInsensitiveMarker(marker))
            {
                remaining = buffer[marker.Length..];
                return true;
            }

            remaining = default;
            return false;
        }

        public ReadOnlySpan<char> SkipWhitespace()
        {
            return buffer.TrimStart();
        }

        public bool SkipWhitespaceToCharacter(char character, out ReadOnlySpan<char> remaining)
        {
            var result = buffer.SkipWhitespace();
            if (!buffer.IsEmpty && buffer[0] != character)
            {
                remaining = default;
                return false;
            }

            remaining = result;
            return true;
        }

        public bool SkipWhitespaceAndCharacter(char character, out ReadOnlySpan<char> remaining)
        {
            if (!buffer.SkipWhitespaceToCharacter(character, out buffer))
            {
                remaining = default;
                return false;
            }
            remaining = buffer[1..];
            return true;
        }

        public bool ReadNumber(out FormatNumericArg value, out ReadOnlySpan<char> remaining)
        {
            const string validNumericCharacters = "+-0123456789.ful";
            const string suffixNumericCharacters = "ful";

            var numericString = new List<char>(buffer.Length);
            while (!buffer.IsEmpty && validNumericCharacters.Contains(buffer[0]))
            {
                numericString.Add(buffer[0]);
                buffer = buffer[1..];
            }

            var suffixString = new List<char>(numericString.Count);
            while (numericString.Count > 0 && suffixNumericCharacters.Contains(numericString[^1]))
            {
                suffixString.Add(numericString[^1]);
                numericString.RemoveAt(numericString.Count - 1);
            }

            var numericStringSpan = CollectionsMarshal.AsSpan(numericString);
            if (!numericStringSpan.IsNumeric)
            {
                value = default;
                remaining = default;
                return false;
            }

            if (suffixString.Contains('f'))
            {
                value = float.Parse(numericStringSpan);
            }
            else if (suffixString.Contains('u'))
            {
                value = ulong.Parse(numericStringSpan);
            }
            else if (numericString.Contains('.'))
            {
                value = double.Parse(numericStringSpan);
            }
            else
            {
                value = long.Parse(numericStringSpan);
            }

            remaining = buffer;
            return true;
        }

        public bool ReadLetterOrDigit(out string value, out ReadOnlySpan<char> remaining)
        {
            var valueBuilder = new StringBuilder();
            while (!buffer.IsEmpty && char.IsLetterOrDigit(buffer[0]) || buffer[0] == '_')
            {
                valueBuilder.Append(buffer[0]);
                buffer = buffer[1..];
            }

            if (valueBuilder.Length == 0)
            {
                value = "";
                remaining = default;
                return false;
            }

            value = valueBuilder.ToString();
            remaining = buffer;
            return true;
        }

        public bool ReadQuotedString(out string value, out ReadOnlySpan<char> remaining)
        {
            var isMacroWrapped = buffer.PeekMarker(TextMarker);
            if (isMacroWrapped)
            {
                buffer = buffer[TextMarker.Length..];
                if (!buffer.SkipWhitespaceAndCharacter('(', out buffer))
                {
                    value = "";
                    remaining = default;
                    return false;
                }
            }

            var builder = new StringBuilder();
            if (!Parse.QuotedString(buffer, builder, out var charsRead))
            {
                value = "";
                remaining = default;
                return false;
            }

            buffer = buffer[charsRead..];
            value = builder.ToString();

            if (isMacroWrapped)
            {
                if (!buffer.SkipWhitespaceAndCharacter(')', out buffer))
                {
                    value = "";
                    remaining = default;
                    return false;
                }
            }

            remaining = buffer;
            return true;
        }

        public bool ReadNumberFormattingOptions(
            [NotNullWhen(true)] out NumberFormattingOptions? options,
            out ReadOnlySpan<char> remaining
        )
        {
            var builder = new NumberFormattingOptionsBuilder();

            var didReadOption = true;
            while (didReadOption)
            {
                didReadOption = false;

                if (
                    ReadCustomOption<bool>(
                        ref builder,
                        ref buffer,
                        "SetAlwaysSign",
                        ReadBoolOption,
                        (in b, v) => b with { AlwaysSign = v },
                        ref didReadOption
                    )
                    && ReadCustomOption<bool>(
                        ref builder,
                        ref buffer,
                        "SetUseGrouping",
                        ReadBoolOption,
                        (in b, v) => b with { UseGrouping = v },
                        ref didReadOption
                    )
                    && ReadCustomOption<RoundingMode>(
                        ref builder,
                        ref buffer,
                        "SetRoundingMode",
                        ReadRoundingModeOption,
                        (in b, v) => b with { RoundingMode = v },
                        ref didReadOption
                    )
                    && ReadCustomOption<int>(
                        ref builder,
                        ref buffer,
                        "SetMinimumIntegralDigits",
                        ReadNumericOption,
                        (in b, v) => b with { MinimumIntegralDigits = v },
                        ref didReadOption
                    )
                    && ReadCustomOption<int>(
                        ref builder,
                        ref buffer,
                        "SetMaximumIntegralDigits",
                        ReadNumericOption,
                        (in b, v) => b with { MaximumIntegralDigits = v },
                        ref didReadOption
                    )
                    && ReadCustomOption<int>(
                        ref builder,
                        ref buffer,
                        "SetMinimumFractionalDigits",
                        ReadNumericOption,
                        (in b, v) => b with { MinimumFractionalDigits = v },
                        ref didReadOption
                    )
                    && ReadCustomOption<int>(
                        ref builder,
                        ref buffer,
                        "SetMaximumFractionalDigits",
                        ReadNumericOption,
                        (in b, v) => b with { MaximumFractionalDigits = v },
                        ref didReadOption
                    )
                )
                    continue;
                options = builder.Build();
                remaining = default;
                return false;
            }

            options = builder.Build();
            remaining = buffer;
            return true;

            static bool ReadCustomOption<T>(
                ref NumberFormattingOptionsBuilder builder,
                ref ReadOnlySpan<char> valueBuffer,
                ReadOnlySpan<char> optionMarker,
                NumberFormattingOptionReader<T> readOption,
                NumberFormattingOptionSetter<T> optionSetter,
                ref bool didReadOption
            )
            {
                if (valueBuffer[0] == '.')
                {
                    valueBuffer = valueBuffer[1..];
                }

                var valueStart = valueBuffer;
                valueBuffer.ReadNumberFormattingOption(
                    optionMarker,
                    readOption,
                    builder,
                    optionSetter,
                    out builder,
                    out valueBuffer
                );
                if (valueBuffer.IsEmpty)
                {
                    return false;
                }

                if (valueBuffer.Length < valueStart.Length)
                {
                    didReadOption = true;
                }

                return true;
            }

            static bool ReadBoolOption(ReadOnlySpan<char> valueBuffer, out bool value, out ReadOnlySpan<char> remaining)
            {
                if (valueBuffer.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    value = true;
                    remaining = valueBuffer[4..];
                    return true;
                }

                if (valueBuffer.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    value = false;
                    remaining = valueBuffer[5..];
                    return true;
                }

                value = false;
                remaining = default;
                return false;
            }

            static bool ReadNumericOption(
                ReadOnlySpan<char> valueBuffer,
                out int value,
                out ReadOnlySpan<char> remaining
            )
            {
                if (!valueBuffer.ReadNumber(out var readValue, out valueBuffer))
                {
                    value = 0;
                    remaining = default;
                    return false;
                }

                var matched = readValue.Match(
                    signed => (int?)signed,
                    unsigned => (int)unsigned,
                    floating => (int)floating,
                    doubleFloating => (int)doubleFloating
                );
                if (matched is null)
                {
                    value = 0;
                    remaining = default;
                    return false;
                }

                value = matched.Value;
                remaining = valueBuffer;
                return true;
            }

            static bool ReadRoundingModeOption(
                ReadOnlySpan<char> valueBuffer,
                out RoundingMode value,
                out ReadOnlySpan<char> remaining
            )
            {
                return valueBuffer.ReadScopedEnum("ERoundingMode::", out value, out remaining);
            }
        }

        public bool ReadScopedEnum<T>(ReadOnlySpan<char> scope, out T value, out ReadOnlySpan<char> remaining)
            where T : unmanaged, Enum
        {
            if (!buffer.PeekInsensitiveMarker(scope))
            {
                value = default;
                remaining = default;
                return false;
            }

            buffer = buffer[scope.Length..];
            buffer.ReadLetterOrDigit(out var enumName, out buffer);

            if (Enum.TryParse(enumName, true, out value))
            {
                remaining = buffer;
                return true;
            }

            remaining = default;
            return false;
        }

        private bool ReadNumberFormattingOption<T>(
            ReadOnlySpan<char> optionMarker,
            NumberFormattingOptionReader<T> readOption,
            NumberFormattingOptionsBuilder builder,
            NumberFormattingOptionSetter<T> optionSetter,
            out NumberFormattingOptionsBuilder updatedBuilder,
            out ReadOnlySpan<char> remaining
        )
        {
            if (!buffer.PeekMarker(optionMarker))
            {
                remaining = buffer;
                updatedBuilder = builder;
                return true;
            }

            buffer = buffer[optionMarker.Length..];
            if (!buffer.SkipWhitespaceAndCharacter('(', out buffer))
            {
                remaining = default;
                updatedBuilder = builder;
                return false;
            }
            buffer = buffer.SkipWhitespace();

            if (!readOption(buffer, out var value, out buffer))
            {
                remaining = default;
                updatedBuilder = builder;
                return false;
            }

            updatedBuilder = optionSetter(in builder, value);
            if (!buffer.SkipWhitespaceAndCharacter(')', out buffer))
            {
                remaining = default;
                return false;
            }

            remaining = buffer;
            return true;
        }

        public bool ReadNumberOrPercent(
            ReadOnlySpan<char> tokenMarker,
            out FormatNumericArg value,
            out NumberFormattingOptions? formattingOptions,
            out Culture? targetCulture,
            out ReadOnlySpan<char> remaining
        )
        {
            value = default;
            formattingOptions = null;
            remaining = default;
            targetCulture = null;

            if (!buffer.PeekMarker(tokenMarker))
            {
                return false;
            }

            buffer = buffer[tokenMarker.Length..];

            var isCustom = buffer.PeekMarker(CustomSuffix);
            if (isCustom)
            {
                buffer = buffer[CustomSuffix.Length..];
            }
            else if (buffer.PeekMarker(GroupedSuffix))
            {
                buffer = buffer[GroupedSuffix.Length..];
                formattingOptions = NumberFormattingOptions.DefaultWithGrouping;
            }
            else if (buffer.PeekMarker(UngroupedSuffix))
            {
                buffer = buffer[UngroupedSuffix.Length..];
                formattingOptions = NumberFormattingOptions.DefaultWithoutGrouping;
            }
            else
            {
                formattingOptions = null;
            }

            if (buffer.SkipWhitespaceAndCharacter('(', out buffer))
            {
                return false;
            }

            buffer = buffer.SkipWhitespace();
            if (buffer.ReadNumber(out var numericValue, out buffer))
            {
                return false;
            }
            value = numericValue;

            if (isCustom)
            {
                if (buffer.SkipWhitespaceAndCharacter(',', out buffer))
                {
                    return false;
                }

                buffer = buffer.SkipWhitespace();
                if (!buffer.ReadNumberFormattingOptions(out formattingOptions, out buffer))
                {
                    return false;
                }
            }

            if (!buffer.SkipWhitespaceAndCharacter(',', out buffer))
            {
                return false;
            }

            buffer = buffer.SkipWhitespace();
            if (!buffer.ReadQuotedString(out var cultureName, out buffer))
            {
                return false;
            }

            targetCulture = string.IsNullOrEmpty(cultureName) ? null : CultureManager.Instance.GetCulture(cultureName);

            if (!buffer.SkipWhitespaceAndCharacter(')', out buffer))
            {
                return false;
            }

            remaining = buffer;
            return true;
        }
    }

    private delegate bool NumberFormattingOptionReader<T>(
        ReadOnlySpan<char> valueBuffer,
        out T value,
        out ReadOnlySpan<char> remaining
    );
    private delegate NumberFormattingOptionsBuilder NumberFormattingOptionSetter<in T>(
        in NumberFormattingOptionsBuilder builder,
        T value
    );

    private static readonly NumberFormattingOptions DefaultNumberFormatOptions = new();

    extension(StringBuilder buffer)
    {
        private void WriteNumberFormattingOption<T>(
            ReadOnlySpan<char> optionFunctionName,
            T optionValue,
            T defaultOptionValue,
            Action<StringBuilder, T> writeOption
        )
            where T : unmanaged
        {
            if (optionValue.Equals(defaultOptionValue))
                return;

            if (buffer.Length > 0)
            {
                buffer.Append('.');
            }
            buffer.Append(optionFunctionName);
            buffer.Append('(');
            writeOption(buffer, optionValue);
            buffer.Append(')');
        }

        public void WriteNumberFormattingOptions(NumberFormattingOptions options)
        {
            buffer.WriteNumberFormattingOption(
                "SetAlwaysSign",
                options.AlwaysSign,
                DefaultNumberFormatOptions.AlwaysSign,
                WriteBoolOption
            );
            buffer.WriteNumberFormattingOption(
                "SetUseGrouping",
                options.UseGrouping,
                DefaultNumberFormatOptions.UseGrouping,
                WriteBoolOption
            );
            buffer.WriteNumberFormattingOption(
                "SetRoundingMode",
                options.RoundingMode,
                DefaultNumberFormatOptions.RoundingMode,
                WriteRoundingModeOption
            );
            buffer.WriteNumberFormattingOption(
                "SetMinimumIntegralDigits",
                options.MinimumIntegralDigits,
                DefaultNumberFormatOptions.MinimumIntegralDigits,
                WriteIntOption
            );
            buffer.WriteNumberFormattingOption(
                "SetMaximumIntegralDigits",
                options.MaximumIntegralDigits,
                DefaultNumberFormatOptions.MaximumIntegralDigits,
                WriteIntOption
            );
            buffer.WriteNumberFormattingOption(
                "SetMinimumFractionalDigits",
                options.MinimumFractionalDigits,
                DefaultNumberFormatOptions.MinimumFractionalDigits,
                WriteIntOption
            );
            buffer.WriteNumberFormattingOption(
                "SetMaximumFractionalDigits",
                options.MaximumFractionalDigits,
                DefaultNumberFormatOptions.MaximumFractionalDigits,
                WriteIntOption
            );

            return;

            void WriteBoolOption(StringBuilder builder, bool value)
            {
                builder.Append(value);
            }

            void WriteIntOption(StringBuilder builder, int value)
            {
                builder.Append(value);
            }

            void WriteRoundingModeOption(StringBuilder builder, RoundingMode value)
            {
                builder.WriteScopedEnum("ERoundingMode::", value);
            }
        }

        public void WriteScopedEnum<T>(ReadOnlySpan<char> scope, T value)
            where T : unmanaged, Enum
        {
            buffer.Append(scope);
            buffer.Append(value);
        }

        public void WriteNumberOrPercent(
            ReadOnlySpan<char> tokenMarker,
            in FormatArg sourceValue,
            NumberFormattingOptions? formattingOptions,
            Culture? targetCulture
        )
        {
            string suffix;
            var customOptions = new StringBuilder();
            if (formattingOptions is not null)
            {
                if (formattingOptions.Equals(NumberFormattingOptions.DefaultWithGrouping))
                {
                    suffix = GroupedSuffix;
                }
                else if (formattingOptions.Equals(NumberFormattingOptions.DefaultWithoutGrouping))
                {
                    suffix = UngroupedSuffix;
                }
                else
                {
                    customOptions.WriteNumberFormattingOptions(formattingOptions);
                    suffix = customOptions.Length > 0 ? CustomSuffix : "";
                }
            }
            else
            {
                suffix = "";
            }

            buffer.Append(tokenMarker).Append(suffix).Append('(');
            sourceValue.ToExportedString(buffer);
        }
    }
}
