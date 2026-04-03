// // @file TextStringHelper.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;
using RetroEngine.Portable.Localization.History;
using RetroEngine.Portable.Localization.Stringification;
using RetroEngine.Portable.Utils;
using ZParse;
using ZParse.Parsers;

namespace RetroEngine.Portable.Localization;

internal static class TextStringHelper
{
    public static Text ReadFromBuffer(
        ReadOnlySpan<char> buffer,
        string? textNamespace = null,
        bool requiresQuotes = false
    )
    {
        var result = ReadFromBuffer(new TextSegment(buffer), textNamespace, requiresQuotes);
        return result.HasValue
            ? result.Value
            : throw new ParseException(result.ErrorPosition, result.FormatErrorMessageFragment());
    }

    internal static ParseResult<Text> ReadFromBuffer(
        TextSegment input,
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

        return requiresQuotes ? QuotedTextString(input) : RegularTextString(input);
    }

    private static readonly TextParser<Text> CultureInvariantText = TextStringReader
        .Marked(Markers.InvText, TextStringReader.TextLiteral)
        .Select(r => new Text(r));

    private static readonly TextParser<Text> QuotedTextString = TextStringReader.TextLiteral.Select(r => new Text(r));

    private static readonly TextParser<Text> RegularTextString = Sequences
        .MatchedBy(Characters.AnyChar.IgnoreAtLeastOnce())
        .Select(r => new Text(r.ToString()));

    private static ParseResult<Text> ReadFromBufferComplex(TextSegment buffer, string? textNamespace = null)
    {
        var invariantText = CultureInvariantText(buffer);
        if (invariantText.HasValue)
        {
            return invariantText;
        }

        foreach (var parser in ReadTextHistories)
        {
            var result = parser(buffer, textNamespace);
            if (result.HasValue)
                return ParseResult.Success(new Text(result.Value), result.Input, result.Remainder);
        }

        return ParseResult.Empty<Text>(buffer);
    }

    private static ParseResult<ITextData> ReadText<T>(TextSegment buffer, string? textNamespace)
        where T : ITextHistory
    {
        return T.ReadFromBuffer(buffer, textNamespace);
    }

    private delegate ParseResult<ITextData> TextHistoryParser(TextSegment buffer, string? textNamespace);

    private static readonly ImmutableArray<TextHistoryParser> ReadTextHistories =
    [
        ReadText<TextHistorySimple>,
        ReadText<TextHistoryNamedFormat>,
        ReadText<TextHistoryOrderedFormat>,
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
        if (value.IsCultureInvariant)
        {
            buffer.Append($"{Markers.InvText}(\"");
            buffer.Append(value.ToString().ReplaceCharWithEscapedChar());
            buffer.Append("\")");
        }
        else if (value.TextData.History.WriteToBuffer(buffer))
        {
            // Nothing to do here
        }
        else if (requiresQuotes)
        {
            buffer.Append('"');
            buffer.Append(value.ToString().ReplaceCharWithEscapedChar());
            buffer.Append('"');
        }
        else
        {
            buffer.Append(value.ToString());
        }
    }
}
