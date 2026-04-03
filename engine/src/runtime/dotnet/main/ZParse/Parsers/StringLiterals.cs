// // @file Strings.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;
using LinkDotNet.StringBuilder;
using ZParse.Util;

namespace ZParse.Parsers;

public static class StringLiterals
{
    private static readonly ImmutableArray<char> StopCharacters = ['"', '\n', '\r', '\0'];
    private static readonly ImmutableArray<char> StopAndEscapeCharacters = StopCharacters.Add('\\');

    private static readonly TextParser<TextSegment> UnescapedSequence = Sequences.MatchedBy(
        Characters.ExceptIn(StopAndEscapeCharacters).IgnoreAtLeastOnce()
    );

    private static readonly TextParser<Rune> SimpleEscapeCharacter = Characters
        .In('\\', '"', 'n', 'r', 't')
        .Select(c => new Rune(c));

    private static readonly TextParser<Rune> OctalEscape = Characters.OctalDigit.RepeatedRange(
        1,
        3,
        () => 0,
        (i, c) => i + char.ToOctalValue(c),
        i => new Rune((char)i)
    );

    private static readonly TextParser<Rune> HexEscape = Characters
        .EqualTo('x')
        .IgnoreThen(Characters.HexDigit.AtLeastOnce(() => 0, (i, c) => i + char.ToHexValue(c), i => new Rune((char)i)));

    private static readonly TextParser<Rune> Utf16Escape = Characters
        .EqualTo('u')
        .IgnoreThen(
            Characters.HexDigit.RepeatedRange(1, 4, () => 0, (i, c) => i + char.ToHexValue(c), i => new Rune((char)i))
        );

    private static readonly TextParser<Rune> Utf32Escape = Characters
        .EqualTo('U')
        .IgnoreThen(
            Characters.HexDigit.RepeatedRange(1, 8, () => 0, (i, c) => i + char.ToHexValue(c), i => new Rune(i))
        );

    private static readonly TextParser<Rune> ValidEscapedSequence = SimpleEscapeCharacter.Or(
        OctalEscape,
        HexEscape,
        Utf16Escape,
        Utf32Escape
    );

    private static readonly TextParser<char> QuoteMark = Characters.EqualTo('"');

    public static TextParser<string> QuotedString { get; } =
        input =>
        {
            var openingQuote = QuoteMark(input);
            if (!openingQuote.HasValue)
                return ParseResult.CastEmpty<char, string>(openingQuote);

            using var builder = new ValueStringBuilder();

            var remainder = openingQuote.Remainder;
            while (true)
            {
                var unescaped = UnescapedSequence(remainder);
                if (unescaped.HasValue)
                {
                    remainder = unescaped.Remainder;
                    builder.Append(unescaped.Value);
                }

                var next = remainder.ConsumeChar();
                if (!next.HasValue || next.Value != '\\')
                    break;

                remainder = next.Remainder;
                var validEscape = ValidEscapedSequence(next.Remainder);
                if (validEscape.HasValue)
                {
                    builder.Append(validEscape.Value);
                    remainder = validEscape.Remainder;
                }
                else if (remainder.ConsumeChar() is { HasValue: true } nextChar)
                {
                    builder.Append('\\');
                    builder.Append(nextChar.Value);
                    remainder = nextChar.Remainder;
                }
            }

            var closingQuote = QuoteMark(remainder);
            return closingQuote.HasValue
                ? ParseResult.Success(builder.ToString(), input, closingQuote.Remainder)
                : ParseResult.CastEmpty<char, string>(closingQuote);
        };
}
