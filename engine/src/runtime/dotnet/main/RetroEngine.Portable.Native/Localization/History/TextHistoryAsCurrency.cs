// // @file TextHistoryAsCurrency.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Portable.Localization.Formatting;
using RetroEngine.Portable.Localization.Stringification;
using RetroEngine.Portable.Utils;
using Superpower;
using Superpower.Model;

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

    private static readonly TextParser<(FormatNumericArg, string, Culture?)> Parser = Parse.Sequence(
        TextParsers
            .Marker(Markers.LocGenCurrency)
            .IgnoreThen(TextParsers.WhitespaceAndOpenParen)
            .IgnoreThen(TextParsers.Whitespace)
            .IgnoreThen(TextParsers.Number),
        TextParsers.WhitespaceAndComma.IgnoreThen(TextParsers.QuotedString),
        TextParsers.WhitespaceAndComma.IgnoreThen(TextParsers.CultureFromName)
    );

    public static Result<ITextData> ReadFromBuffer(string str, string? textNamespace, string? textKey)
    {
        var culture = CultureManager.Instance.CurrentLocale;

        var result = Parser.TryParse(str);
        if (!result.HasValue)
        {
            return Result.CastEmpty<(FormatNumericArg, string, Culture?), ITextData>(result);
        }

        var (sourceValue, currencyCode, targetCulture) = result.Value;

        var baseValue = sourceValue.Match(i => i, u => u, f => f, d => d);

        var formattingRules = culture.GetCurrencyFormattingRules(currencyCode);
        var formattingOptions = formattingRules.DefaultFormattingOptions;
        var dividedValue = baseValue / FastDecimalFormat.Pow10(formattingOptions.MaximumFractionalDigits);

        return Result.Value<ITextData>(
            new TextHistoryAsCurrency(dividedValue, currencyCode, formattingOptions, targetCulture),
            result.Location,
            result.Remainder
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
