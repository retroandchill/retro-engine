// // @file TextHistoryAsCurrency.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Portable.Localization.Formatting;
using RetroEngine.Portable.Localization.Stringification;
using RetroEngine.Utilities;
using ZParse;

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

    private static readonly TextParser<ITextData> Parser = TextStringReader.Marked(
        Markers.LocGenCurrency,
        TextStringReader
            .Number.Then(
                TextStringReader.CommaSeparator.IgnoreThen(TextStringReader.TextLiteral),
                (n, c) => (Number: n, CurrencyCode: c)
            )
            .Then(
                TextStringReader.CommaSeparator.IgnoreThen(TextStringReader.CultureByName),
                ITextData (t, targetCulture) =>
                {
                    var culture = CultureManager.Instance.CurrentLocale;

                    var (number, currencyCode) = t;
                    var baseValue = number.Match(i => i, u => u, f => f, d => d);
                    var formattingRules = culture.GetCurrencyFormattingRules(currencyCode);
                    var formattingOptions = formattingRules.DefaultFormattingOptions;
                    var dividedValue = baseValue / FastDecimalFormat.Pow10(formattingOptions.MaximumFractionalDigits);

                    return new TextHistoryAsCurrency(dividedValue, currencyCode, formattingOptions, targetCulture);
                }
            )
    );

    public static ParseResult<ITextData> ImportFromString(TextSegment input, string? textNamespace)
    {
        return Parser(input);
    }

    public override bool ExportToString(StringBuilder buffer)
    {
        var culture = TargetCulture ?? CultureManager.Instance.CurrentLocale;

        var dividedValue = SourceValue.Match(i => i, u => u, f => f, d => d);

        var formattingRules = culture.GetCurrencyFormattingRules(_currencyCode);
        var formattingOptions = formattingRules.DefaultFormattingOptions;
        var baseValue = (long)(dividedValue * FastDecimalFormat.Pow10(formattingOptions.MaximumFractionalDigits));

        buffer.Append("LOCGEN_CURRENCY(");
        FormatArg.Signed(baseValue).ToExportedString(buffer);
        buffer.Append(", \"");
        buffer.Append(_currencyCode?.ReplaceCharWithEscapedChar());
        buffer.Append("\", \"");
        if (TargetCulture is not null)
        {
            buffer.Append(TargetCulture.Name.ReplaceCharWithEscapedChar());
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
