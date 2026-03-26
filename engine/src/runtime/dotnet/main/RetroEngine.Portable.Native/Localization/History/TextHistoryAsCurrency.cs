// // @file TextHistoryAsCurrency.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Portable.Localization.Formatting;
using RetroEngine.Portable.Utils;
using Superpower.Model;

namespace RetroEngine.Portable.Localization.History;

internal sealed class TextHistoryAsCurrency(
    string displayString,
    FormatNumericArg sourceValue,
    string? currencyCode,
    NumberFormattingOptions? formattingOptions,
    Culture? targetCulture
) : TextHistoryFormatNumber(displayString, sourceValue, formattingOptions, targetCulture), ITextHistory
{
    protected override string BuildLocalizedDisplayString()
    {
        var culture = TargetCulture ?? CultureManager.Instance.CurrentLocale;
        var formattingRules = culture.GetCurrencyFormattingRules(currencyCode);
        return BuildNumericDisplayString(formattingRules);
    }

    public static bool ShouldReadFromBuffer(ReadOnlySpan<char> buffer)
    {
        return buffer.PeekMarker(TextStringificationUtil.LocGenCurrencyMarker);
    }

    public static Result<ITextData> ReadFromBuffer(string str, string? textNamespace, string? textKey)
    {
        var culture = CultureManager.Instance.CurrentLocale;
        remaining = default;
        if (!str.PeekMarker(TextStringificationUtil.LocGenCurrencyMarker))
            return null;

        str = str[TextStringificationUtil.LocGenCurrencyMarker.Length..];

        if (!str.SkipWhitespaceAndCharacter('(', out str))
            return null;

        str = str.SkipWhitespace();
        if (!str.ReadNumber(out var numericValue, out str))
            return null;

        if (!str.SkipWhitespaceAndCharacter(',', out str))
            return null;

        str = str.SkipWhitespace();
        if (!str.ReadQuotedString(out var currencyCode, out str))
            return null;

        if (!str.SkipWhitespaceAndCharacter(',', out str))
            return null;

        str = str.SkipWhitespace();
        if (!str.ReadQuotedString(out var cultureName, out str))
            return null;
        var targetCulture = string.IsNullOrEmpty(cultureName) ? null : CultureManager.Instance.GetCulture(cultureName);

        if (!str.SkipWhitespaceAndCharacter(')', out str))
            return null;

        var baseValue = numericValue.Match(i => i, u => u, f => f, d => d);

        var formattingRules = culture.GetCurrencyFormattingRules(currencyCode);
        var formattingOptions = formattingRules.DefaultFormattingOptions;
        numericValue = baseValue / FastDecimalFormat.Pow10(formattingOptions.MaximumFractionalDigits);

        remaining = str;
        return new TextHistoryAsCurrency("", numericValue, currencyCode, formattingOptions, targetCulture);
    }

    public override bool WriteToBuffer(StringBuilder buffer)
    {
        var culture = TargetCulture ?? CultureManager.Instance.CurrentLocale;

        var dividedValue = SourceValue.Match(i => i, u => u, f => f, d => d);

        var formattingRules = culture.GetCurrencyFormattingRules(currencyCode);
        var formattingOptions = formattingRules.DefaultFormattingOptions;
        var baseValue = (long)(dividedValue * FastDecimalFormat.Pow10(formattingOptions.MaximumFractionalDigits));

        buffer.Append("LOCGEN_CURRENCY(");
        FormatArg.Signed(baseValue).ToExportedString(buffer);
        buffer.Append(", \"");
        buffer.Append(currencyCode?.ReplaceQuotesWithEscapedQuotes());
        buffer.Append("\", \"");
        if (TargetCulture is not null)
        {
            buffer.Append(TargetCulture.Name.ReplaceQuotesWithEscapedQuotes());
        }
        buffer.Append("\")");

        return true;
    }

    public override string BuildInvariantDisplayString()
    {
        var culture = CultureManager.Instance.InvariantCulture;
        var formattingRules = culture.GetCurrencyFormattingRules(currencyCode);
        return BuildNumericDisplayString(formattingRules);
    }
}
