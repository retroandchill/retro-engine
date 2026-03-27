// // @file TextHistorySimple.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using RetroEngine.Portable.Localization.Stringification;
using RetroEngine.Portable.Parsers;
using RetroEngine.Portable.Utils;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace RetroEngine.Portable.Localization.History;

internal sealed class TextHistorySimple : TextHistoryBase, ITextHistory
{
    public TextHistorySimple() { }

    public TextHistorySimple(TextId id, string source, string? localized = null)
        : base(id, source, localized) { }

    private static readonly TextParser<(string Namespace, string Key, string Source)> NsLoctextParser = TextParsers
        .Marker(Markers.NsLocText)
        .IgnoreThen(TextParsers.WhitespaceAndOpenParen)
        .IgnoreThen(
            Parse.Sequence(
                TextParsers.Whitespace.IgnoreThen(TextParsers.QuotedString),
                TextParsers.WhitespaceAndComma.IgnoreThen(TextParsers.QuotedString),
                TextParsers.WhitespaceAndComma.IgnoreThen(TextParsers.QuotedString)
            )
        )
        .FollowedBy(TextParsers.WhitespaceAndCloseParen)
        .AtEnd()
        .Select(r =>
        {
            var (ns, key, source) = r;

            if (string.IsNullOrEmpty(key))
            {
                key = Guid.NewGuid().ToString();
            }

            return (ns, key, source);
        });

    private static readonly TextParser<(string Key, string Source)> LoctextParser = TextParsers
        .Marker(Markers.LocText)
        .IgnoreThen(TextParsers.WhitespaceAndOpenParen)
        .IgnoreThen(
            Parse.Sequence(
                TextParsers.Whitespace.IgnoreThen(TextParsers.QuotedString),
                TextParsers.WhitespaceAndComma.IgnoreThen(TextParsers.QuotedString)
            )
        )
        .FollowedBy(TextParsers.WhitespaceAndCloseParen)
        .AtEnd()
        .Select(r =>
        {
            var (key, source) = r;

            if (string.IsNullOrEmpty(key))
            {
                key = Guid.NewGuid().ToString();
            }

            return (key, source);
        });

    public static Result<ITextData> ReadFromBuffer(string str, string? textNamespace, string? textKey)
    {
        var nsLocTextResult = NsLoctextParser.TryParse(str);
        if (nsLocTextResult.HasValue)
        {
            var (ns, key, source) = nsLocTextResult.Value;
            return Result.Value<ITextData>(
                new TextHistorySimple(new TextId(ns, key), source),
                nsLocTextResult.Location,
                nsLocTextResult.Remainder
            );
        }

        var loctextResult = LoctextParser.TryParse(str);
        // ReSharper disable once InvertIf
        if (loctextResult.HasValue)
        {
            var (key, source) = loctextResult.Value;
            return Result.Value<ITextData>(
                new TextHistorySimple(new TextId(textNamespace ?? string.Empty, key), source),
                loctextResult.Location,
                loctextResult.Remainder
            );
        }

        return Result.Empty<ITextData>(new TextSpan(str));
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
