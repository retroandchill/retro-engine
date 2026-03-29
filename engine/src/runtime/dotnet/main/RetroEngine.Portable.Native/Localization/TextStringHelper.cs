// // @file TextStringHelper.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;
using RetroEngine.Portable.Localization.History;
using RetroEngine.Portable.Localization.Stringification;
using RetroEngine.Portable.Parsers;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace RetroEngine.Portable.Localization;

internal static class TextStringHelper
{
    public static Result<Text> ReadFromBuffer(string buffer, string? textNamespace = null, bool requiresQuotes = false)
    {
        return ReadFromBuffer(new TextSpan(buffer), textNamespace, requiresQuotes);
    }

    private static readonly TextParser<Text> QuotedTextLiteral = StringLiteral.UnrealStyle.Select(t => new Text(t));

    internal static Result<Text> ReadFromBuffer(
        TextSpan buffer,
        string? textNamespace = null,
        bool requiresQuotes = false
    )
    {
        if (buffer.IsAtEnd)
        {
            return requiresQuotes ? Result.Empty<Text>(buffer) : Result.Value(Text.Empty, buffer, buffer);
        }

        var complexResult = ReadFromBufferComplex(buffer, textNamespace);
        if (complexResult.HasValue)
        {
            return complexResult;
        }

        if (!requiresQuotes)
            return Result.Value(new Text(buffer.ToStringValue()), buffer, buffer.Skip(buffer.Length));

        var literalResult = QuotedTextLiteral(buffer);
        return literalResult.HasValue
            ? Result.Value(new Text(literalResult.Value), literalResult.Location, literalResult.Remainder)
            : Result.CombineEmpty(complexResult, literalResult);
    }

    public static readonly TextParser<Text> CultureInvariantText = Span.EqualTo(Markers.InvText)
        .IgnoreThen(TextParsers.WhitespaceAndOpenParen)
        .IgnoreThen(TextParsers.Whitespace)
        .IgnoreThen(TextParsers.QuotedString)
        .FollowedBy(TextParsers.WhitespaceAndCloseParen)
        .Select(Text.AsCultureInvariant);

    private static Result<Text> ReadFromBufferComplex(TextSpan buffer, string? textNamespace = null)
    {
        var invariantText = CultureInvariantText(buffer);
        if (invariantText.HasValue)
        {
            return invariantText;
        }

        var emptyResult = invariantText;
        foreach (var result in ReadTextHistories.Select(parser => parser(buffer, textNamespace)))
        {
            if (result.HasValue)
                return result;

            emptyResult = Result.CombineEmpty(emptyResult, result);
        }

        return emptyResult;
    }

    private static Result<Text> ReadText<T>(TextSpan buffer, string? textNamespace)
        where T : ITextHistory
    {
        var result = T.ReadFromBuffer(buffer, textNamespace);
        return result.HasValue
            ? Result.Value(new Text(result.Value), result.Location, result.Remainder)
            : Result.Empty<Text>(buffer);
    }

    private delegate Result<Text> TextHistoryParser(TextSpan buffer, string? textNamespace);

    private static readonly ImmutableArray<TextHistoryParser> ReadTextHistories =
    [
        ReadText<TextHistorySimple>,
        ReadText<TextHistoryNamedFormat>,
        ReadText<TextHistoryOrderedFormat>,
        //ReadTextData<TextHistoryArgumentDataFormat)>,
        ReadText<TextHistoryAsNumber>,
        ReadText<TextHistoryAsPercent>,
        ReadText<TextHistoryAsCurrency>,
        ReadText<TextHistoryAsDateTime>,
        ReadText<TextHistoryAsDate>,
        ReadText<TextHistoryAsTime>,
        ReadText<TextHistoryTransformed>,
        //ReadTextData<TextHistoryStringTableEntry>
    ];

    public static void WriteToBuffer(StringBuilder buffer, Text value, bool requiresQuotes = false)
    {
        throw new NotImplementedException();
    }
}
