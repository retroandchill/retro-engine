// // @file TextHistoryAsCurrency.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;
using System.Text;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Portable.Localization.Formatting;
using RetroEngine.Portable.Utils;

namespace RetroEngine.Portable.Localization.History;

internal sealed class TextHistoryAsCurrency : TextHistoryFormatNumber
{
    private string? _currencyCode;

    public TextHistoryAsCurrency() { }

    public TextHistoryAsCurrency(
        string displayString,
        FormatNumericArg sourceValue,
        string? currencyCode,
        NumberFormattingOptions? formattingOptions,
        Culture? targetCulture
    )
        : base(displayString, sourceValue, formattingOptions, targetCulture)
    {
        _currencyCode = currencyCode;
    }

    protected override string BuildLocalizedDisplayString()
    {
        var culture = TargetCulture ?? CultureManager.Instance.CurrentLocale;
        var formattingRules = culture.GetCurrencyFormattingRules(_currencyCode);
        return BuildNumericDisplayString(formattingRules);
    }

    public override bool ReadFromBuffer(
        ReadOnlySpan<char> buffer,
        string? textNamespace,
        string? textKey,
        out ReadOnlySpan<char> remaining
    )
    {
        var culture = TargetCulture ?? CultureManager.Instance.CurrentLocale;
        remaining = default;
        if (!buffer.PeekMarker(TextStringificationUtil.LocGenCurrencyMarker))
            return false;

        buffer = buffer[TextStringificationUtil.LocGenCurrencyMarker.Length..];

        if (!buffer.SkipWhitespaceAndCharacter('(', out buffer))
            return false;

        buffer = buffer.SkipWhitespace();
        if (!buffer.ReadNumber(out var numericValue, out buffer))
            return false;
        SourceValue = numericValue;

        if (!buffer.SkipWhitespaceAndCharacter(',', out buffer))
            return false;

        buffer = buffer.SkipWhitespace();
        if (!buffer.ReadQuotedString(out var currencyCode, out buffer))
            return false;
        _currencyCode = currencyCode;

        if (!buffer.SkipWhitespaceAndCharacter(',', out buffer))
            return false;

        buffer = buffer.SkipWhitespace();
        if (!buffer.ReadQuotedString(out var cultureName, out buffer))
            return false;
        TargetCulture = string.IsNullOrEmpty(cultureName) ? null : CultureManager.Instance.GetCulture(cultureName);

        if (!buffer.SkipWhitespaceAndCharacter(')', out buffer))
            return false;

        var baseValue = SourceValue.Match(i => i, u => u, f => f, d => d);

        var formattingRules = culture.GetCurrencyFormattingRules(_currencyCode);
        var formattingOptions = formattingRules.DefaultFormattingOptions;
        SourceValue = baseValue / FastDecimalFormat.Pow10(formattingOptions.MaximumFractionalDigits);

        MarkDisplayStringOutOfDate();
        remaining = buffer;
        return true;
    }

    public override bool WriteToBuffer(StringBuilder buffer)
    {
        var culture = TargetCulture ?? CultureManager.Instance.CurrentLocale;

        var dividedValue = SourceValue.Match(i => i, u => u, f => f, d => d);

        var formattingRules = culture.GetCurrencyFormattingRules(_currencyCode);
        var formattingOptions = formattingRules.DefaultFormattingOptions;
        var baseValue = (long)(dividedValue * FastDecimalFormat.Pow10(formattingOptions.MaximumFractionalDigits));

        buffer.Append("LOCGEN_CURRENCY(");
        FormatArg.Signed(baseValue).ToExportedString(buffer);
        buffer.Append(", \"");
        buffer.Append(_currencyCode?.ReplaceQuotesWithEscapedQuotes());
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
        var formattingRules = culture.GetCurrencyFormattingRules(_currencyCode);
        return BuildNumericDisplayString(formattingRules);
    }
}
