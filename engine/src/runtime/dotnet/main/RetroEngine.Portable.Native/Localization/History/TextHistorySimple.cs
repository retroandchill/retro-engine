// // @file TextHistorySimple.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using RetroEngine.Portable.Localization.Stringification;
using RetroEngine.Portable.Utils;
using ZParse;
using ZParse.Parsers;

namespace RetroEngine.Portable.Localization.History;

internal sealed class TextHistorySimple : TextHistoryBase, ITextHistory
{
    public TextHistorySimple() { }

    public TextHistorySimple(TextId id, string source, string? localized = null)
        : base(id, source, localized) { }

    public static ParseResult<ITextData> ReadFromBuffer(TextSegment input, string? textNamespace)
    {
        var nsloctextResult = input.ParseSymbol(Markers.NsLocText);
        if (nsloctextResult.HasValue)
        {
            var nsString = nsloctextResult.Remainder.ParseSequence(
                i => i.ParseWhitespaceAndChar('('),
                i => i.ParseOptionalWhitespace(),
                i => i.ParseQuotedString(),
                (_, _, i) => i
            );
            if (!nsString.HasValue)
                return ParseResult.CastEmpty<string, ITextData>(nsString);

            var keyString = nsString.Remainder.ParseSequence(
                i => i.ParseWhitespaceAndChar(','),
                i => i.ParseOptionalWhitespace(),
                i => i.ParseQuotedString(),
                (_, _, i) => i
            );
            if (!keyString.HasValue)
                return ParseResult.CastEmpty<string, ITextData>(keyString);

            var key = string.IsNullOrEmpty(keyString.Value) ? Guid.NewGuid().ToString() : keyString.Value;

            var sourceString = keyString.Remainder.ParseSequence(
                i => i.ParseWhitespaceAndChar(','),
                i => i.ParseOptionalWhitespace(),
                i => i.ParseQuotedString(),
                i => i.ParseWhitespaceAndChar(')'),
                (_, _, i, _) => i
            );
            if (!sourceString.HasValue)
                return ParseResult.CastEmpty<string, ITextData>(sourceString);

            return ParseResult.Success<ITextData>(
                new TextHistorySimple(new TextId(nsString.Value, key), sourceString.Value),
                input,
                sourceString.Remainder
            );
        }

        var loctextResult = input.ParseSymbol(Markers.LocText);
        if (loctextResult.HasValue)
        {
            var keyString = loctextResult.Remainder.ParseSequence(
                i => i.ParseWhitespaceAndChar(','),
                i => i.ParseOptionalWhitespace(),
                i => i.ParseQuotedString(),
                (_, _, i) => i
            );
            if (!keyString.HasValue)
                return ParseResult.CastEmpty<string, ITextData>(keyString);

            var key = string.IsNullOrEmpty(keyString.Value) ? Guid.NewGuid().ToString() : keyString.Value;

            var sourceString = keyString.Remainder.ParseSequence(
                i => i.ParseWhitespaceAndChar(','),
                i => i.ParseOptionalWhitespace(),
                i => i.ParseQuotedString(),
                i => i.ParseWhitespaceAndChar(')'),
                (_, _, i, _) => i
            );
            if (!sourceString.HasValue)
                return ParseResult.CastEmpty<string, ITextData>(sourceString);

            return ParseResult.Success<ITextData>(
                new TextHistorySimple(new TextId(textNamespace ?? "", key), sourceString.Value),
                input,
                sourceString.Remainder
            );
        }

        return ParseResult.Empty<ITextData>(input);
    }

    public override bool WriteToBuffer(StringBuilder buffer)
    {
        if (TextId.IsEmpty)
            return false;

        var ns = TextId.Namespace.ToString();
        var key = TextId.Key.ToString();

        buffer.Append("NSLOCTEXT(\"");
        buffer.Append(ns.ReplaceQuotesWithEscapedQuotes());
        buffer.Append("\", \"");
        buffer.Append(key.ReplaceQuotesWithEscapedQuotes());
        buffer.Append(Source.ReplaceQuotesWithEscapedQuotes());
        buffer.Append("\")");

        return true;
    }
}
