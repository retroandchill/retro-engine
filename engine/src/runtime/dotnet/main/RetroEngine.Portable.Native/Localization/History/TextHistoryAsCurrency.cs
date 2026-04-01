// // @file TextHistoryAsCurrency.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Portable.Localization.Formatting;
using RetroEngine.Portable.Localization.Stringification;
using RetroEngine.Portable.Utils;
using ZParse;
using ZParse.Parsers;

namespace RetroEngine.Portable.Localization.History;

internal sealed class TextHistoryAsCurrency : TextHistoryFormatNumber, ITextHistory
{
    private readonly string? _currencyCode;

    public TextHistoryAsCurrency(
        FormatNumericArg sourceValue,
        string? currencyCode,
        NumberFormattingOptions? formattingOptions,
        Culture? targetCulture
    )
        : base(sourceValue, formattingOptions, targetCulture)
    {
        _currencyCode = currencyCode;
        UpdateDisplayString();
    }

    protected override string BuildLocalizedDisplayString()
    {
        var culture = TargetCulture ?? CultureManager.Instance.CurrentLocale;
        var formattingRules = culture.GetCurrencyFormattingRules(_currencyCode);
        return BuildNumericDisplayString(formattingRules);
    }

    public static ParseResult<ITextData> ReadFromBuffer(TextSegment input, string? textNamespace)
    {
        var culture = CultureManager.Instance.CurrentLocale;
        var number = input.ParseSequence(
            i => i.ParseSymbol(Markers.LocGenCurrency),
            i => i.ParseWhitespaceAndChar('('),
            i => i.ParseOptionalWhitespace(),
            i => i.ParseNumber(),
            (_, _, _, i) => i
        );
        if (!number.HasValue)
            return ParseResult.CastEmpty<FormatNumericArg, ITextData>(number);

        var currencyCode = number.Remainder.ParseSequence(
            i => i.ParseWhitespaceAndChar(','),
            i => i.ParseOptionalWhitespace(),
            i => i.ParseQuotedString(),
            (_, _, i) => i
        );

        if (!currencyCode.HasValue)
            return ParseResult.CastEmpty<string, ITextData>(currencyCode);

        var targetCulture = currencyCode.Remainder.ParseSequence(
            i => i.ParseWhitespaceAndChar(','),
            i => i.ParseOptionalWhitespace(),
            i => i.ParseQuotedString(),
            i => i.ParseWhitespaceAndChar(')'),
            (_, _, i, _) => CultureManager.Instance.GetCulture(i)
        );
        if (!targetCulture.HasValue)
            return ParseResult.CastEmpty<Culture?, ITextData>(targetCulture);

        var baseValue = number.Value.Match(i => i, u => u, f => f, d => d);
        var formattingRules = culture.GetCurrencyFormattingRules(currencyCode.Value);
        var formattingOptions = formattingRules.DefaultFormattingOptions;
        var dividedValue = baseValue / FastDecimalFormat.Pow10(formattingOptions.MaximumFractionalDigits);

        return ParseResult.Success<ITextData>(
            new TextHistoryAsCurrency(dividedValue, currencyCode.Value, formattingOptions, targetCulture.Value),
            input,
            targetCulture.Remainder
        );
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
