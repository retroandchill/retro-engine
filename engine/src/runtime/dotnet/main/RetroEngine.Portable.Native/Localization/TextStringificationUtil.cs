// // @file TextStringificationUtil.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

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
