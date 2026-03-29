// // @file TextStringificationUtil.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Portable.Localization.Formatting;
using RetroEngine.Portable.Utils;

namespace RetroEngine.Portable.Localization.Stringification;

internal static class TextStringWriter
{
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
                    suffix = Markers.GroupedSuffix;
                }
                else if (formattingOptions.Equals(NumberFormattingOptions.DefaultWithoutGrouping))
                {
                    suffix = Markers.UngroupedSuffix;
                }
                else
                {
                    customOptions.WriteNumberFormattingOptions(formattingOptions);
                    suffix = customOptions.Length > 0 ? Markers.CustomSuffix : "";
                }
            }
            else
            {
                suffix = "";
            }

            buffer.Append(tokenMarker).Append(suffix).Append('(');
            sourceValue.ToExportedString(buffer);
        }

        public void WriteDateTime(
            string marker,
            DateTimeOffset dateTime,
            DateTimeFormatStyle? dateStyle,
            DateTimeFormatStyle? timeStyle,
            string? customPattern,
            string? timeZoneId,
            Culture? targetCulture
        )
        {
            var isCustom = dateStyle == DateTimeFormatStyle.Custom;
            var isInvariant = timeZoneId == Text.InvariantTimeZone;

            buffer.Append(marker);
            if (isCustom)
            {
                buffer.Append(Markers.CustomSuffix);
            }
            buffer.Append(isInvariant ? Markers.LocalSuffix : Markers.UtcSuffix);
            buffer.Append('(');
            buffer.Append(dateTime.ToUnixTimeMilliseconds());
            if (isCustom)
            {
                buffer.Append(", \"");
                buffer.Append(customPattern?.ReplaceQuotesWithEscapedQuotes());
                buffer.Append('"');
            }
            else
            {
                if (dateStyle is not null)
                {
                    buffer.Append(", ");
                    WriteDateTimeStyle(buffer, dateStyle.Value);
                }

                if (timeStyle is null)
                    return;

                buffer.Append(", ");
                WriteDateTimeStyle(buffer, timeStyle.Value);
            }

            if (!isInvariant)
            {
                buffer.Append(", \"");
                buffer.Append(timeZoneId?.ReplaceQuotesWithEscapedQuotes());
                buffer.Append('"');
            }

            buffer.Append(", \"");
            if (targetCulture is not null)
            {
                buffer.Append(targetCulture.Name.ReplaceQuotesWithEscapedQuotes());
            }
            buffer.Append("\")");
            return;

            void WriteDateTimeStyle(StringBuilder builder, DateTimeFormatStyle style)
            {
                builder.WriteScopedEnum("EDateTimeStyle::", style);
            }
        }
    }
}
