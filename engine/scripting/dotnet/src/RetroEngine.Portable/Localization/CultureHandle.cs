// // @file CultureHandle.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Globalization;
using RetroEngine.Portable.Localization.Formatting;

namespace RetroEngine.Portable.Localization;

public sealed class CultureHandle
{
    public string Name => Culture.Name;
    internal CultureInfo Culture { get; }

    /// <summary>
    /// Managed fallback number formatting rules derived from <see cref="CultureInfo.NumberFormat"/>.
    /// This is intentionally "rules only" so your own formatter (e.g. FastDecimalFormat) can apply
    /// engine/Unreal-like rounding and digit count behavior consistently.
    /// </summary>
    public DecimalNumberFormattingRules NumberFormattingRules
    {
        get { return field ??= ExtractDecimalRules(Culture.NumberFormat); }
    }

    /// <summary>
    /// Managed fallback percent formatting rules derived from <see cref="CultureInfo.NumberFormat"/>.
    /// </summary>
    public DecimalNumberFormattingRules PercentNumberFormat
    {
        get { return field ??= ExtractPercentRules(Culture.NumberFormat); }
    }

    internal CultureHandle(CultureInfo cultureInfo)
    {
        Culture = cultureInfo;
    }

    // Later: currency support can either:
    //  - return rules derived from NumberFormatInfo + ISO-4217 minor units table (managed fallback), or
    //  - be backed by ICU when available.
    public DecimalNumberFormattingRules GetCurrencyFormattingRules(string currencyCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(currencyCode);

        // Best-effort managed fallback: treat it like a decimal number and let callers decide how to
        // prepend/append currency symbols. This keeps the API working when ICU/native isn't available.
        // You can refine this to use _cultureInfo.NumberFormat.Currency* fields + minor-units table.
        return NumberFormattingRules;
    }

    private static DecimalNumberFormattingRules ExtractDecimalRules(NumberFormatInfo nfi)
    {
        var rules = ExtractCommonRules(
            nfi,
            groupingSeparator: nfi.NumberGroupSeparator,
            decimalSeparator: nfi.NumberDecimalSeparator,
            groupingSizes: nfi.NumberGroupSizes
        );

        return rules with
        {
            DefaultFormattingOptions = new NumberFormattingOptions
            {
                UseGrouping = nfi.NumberGroupSizes.Length > 0 && nfi.NumberGroupSizes[0] > 0,
            },
        };
    }

    private static DecimalNumberFormattingRules ExtractPercentRules(NumberFormatInfo nfi)
    {
        var rules = ExtractCommonRules(
            nfi,
            groupingSeparator: nfi.PercentGroupSeparator,
            decimalSeparator: nfi.PercentDecimalSeparator,
            groupingSizes: nfi.PercentGroupSizes
        );

        var defaultFractionDigits = Math.Max(0, nfi.PercentDecimalDigits);

        // NOTE: UE/ICU percent patterns can include locale-specific spacing and symbol placement.
        // For managed fallback we just treat percent as a number with different separators/grouping;
        // the actual percent symbol placement can be layered elsewhere if needed.
        return rules with
        {
            DefaultFormattingOptions = new NumberFormattingOptions
            {
                UseGrouping = nfi.PercentGroupSizes.Length > 0 && nfi.PercentGroupSizes[0] > 0,
                MinimumFractionalDigits = defaultFractionDigits,
                MaximumFractionalDigits = defaultFractionDigits,
            },
        };
    }

    private static DecimalNumberFormattingRules ExtractCommonRules(
        NumberFormatInfo nfi,
        string groupingSeparator,
        string decimalSeparator,
        int[] groupingSizes
    )
    {
        static char FirstCharOrFallback(string s, char fallback)
        {
            return !string.IsNullOrEmpty(s) ? s[0] : fallback;
        }

        static (byte Primary, byte Secondary) GetGrouping(int[] sizes)
        {
            // .NET grouping: [3] or [3,2] etc. 0 means "no further grouping".
            if (sizes.Length == 0)
                return (0, 0);

            var primary = sizes[0] <= 0 ? (byte)0 : (byte)Math.Min(255, sizes[0]);
            if (primary == 0)
                return (0, 0);

            if (sizes.Length == 1)
                return (primary, primary);

            var secondaryRaw = sizes[1];
            var secondary = secondaryRaw <= 0 ? primary : (byte)Math.Min(255, secondaryRaw);
            return (primary, secondary);
        }

        var (primaryGrouping, secondaryGrouping) = GetGrouping(groupingSizes);

        // .NET doesn't expose full ICU-style prefix/suffix patterns for each style.
        // We keep the sign strings explicit and use simple prefixing as a baseline.
        return new DecimalNumberFormattingRules
        {
            NanString = nfi.NaNSymbol,
            PlusString = nfi.PositiveSign,
            MinusString = nfi.NegativeSign,

            NegativePrefixString = nfi.NegativeSign,
            NegativeSuffixString = "",
            PositivePrefixString = "",
            PositiveSuffixString = "",

            GroupingSeparatorChar = FirstCharOrFallback(groupingSeparator, ','),
            DecimalSeparatorChar = FirstCharOrFallback(decimalSeparator, '.'),
            PrimaryGroupingSize = primaryGrouping,
            SecondaryGroupingSize = secondaryGrouping,

            // CLDR/ICU has nuanced rules here; as a managed fallback we keep it permissive.
            MinimumGroupingDigits = 1,
        };
    }
}
