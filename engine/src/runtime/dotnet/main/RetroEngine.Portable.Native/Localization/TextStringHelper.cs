// // @file TextStringHelper.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;
using RetroEngine.Portable.Localization.History;
using RetroEngine.Portable.Localization.Stringification;
using ZParse;
using ZParse.Parsers;

namespace RetroEngine.Portable.Localization;

internal static class TextStringHelper
{
    public static ParseResult<Text> ReadFromBuffer(
        ReadOnlySpan<char> buffer,
        string? textNamespace = null,
        bool requiresQuotes = false
    )
    {
        return ReadFromBuffer(new ParseCursor(buffer), textNamespace, requiresQuotes);
    }

    internal static ParseResult<Text> ReadFromBuffer(
        ParseCursor input,
        string? textNamespace = null,
        bool requiresQuotes = false
    )
    {
        if (input.IsAtEnd)
        {
            return requiresQuotes ? ParseResult.Empty<Text>(input) : ParseResult.Success(Text.Empty, input, input);
        }

        var complexResult = ReadFromBufferComplex(input, textNamespace);
        if (complexResult.HasValue)
        {
            return complexResult;
        }

        if (!requiresQuotes)
            return input.ParseToEnd().Select(x => new Text(x.ToString()));

        return input.ParseQuotedString().Select(x => new Text(x.ToString()));
    }

    private static ParseResult<Text> ParseCultureInvariantText(ParseCursor buffer)
    {
        return buffer.ParseSequence(
            i => i.ParseSymbol(Markers.InvText),
            i => i.ParseWhitespaceAndChar('('),
            i => i.ParseOptionalWhitespace(),
            i => i.ParseQuotedString(),
            i => i.ParseWhitespaceAndChar(')'),
            (_, _, _, i, _) => Text.AsCultureInvariant(i)
        );
    }

    private static ParseResult<Text> ReadFromBufferComplex(ParseCursor buffer, string? textNamespace = null)
    {
        var invariantText = ParseCultureInvariantText(buffer);
        if (invariantText.HasValue)
        {
            return invariantText;
        }

        foreach (var parser in ReadTextHistories)
        {
            var result = parser(buffer, textNamespace);
            if (result.HasValue)
                return result;
        }

        return ParseResult.Empty<Text>(buffer);
    }

    private static ParseResult<Text> ReadText<T>(ParseCursor buffer, string? textNamespace)
        where T : ITextHistory
    {
        return T.ReadFromBuffer(buffer, textNamespace).Select(r => new Text(r));
    }

    private delegate ParseResult<Text> TextHistoryParser(ParseCursor buffer, string? textNamespace);

    private static readonly ImmutableArray<TextHistoryParser> ReadTextHistories =
    [
        ReadText<TextHistorySimple>,
        ReadText<TextHistoryNamedFormat>,
        ReadText<TextHistoryOrderedFormat>,
        //ReadText<TextHistoryArgumentDataFormat>,
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
