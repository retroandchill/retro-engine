// // @file StringLiteral.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using LinkDotNet.StringBuilder;
using RetroEngine.Portable.Utils;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace RetroEngine.Portable.Parsers;

[Union]
public readonly partial struct Piece
{
    [UnionCase]
    public static partial Piece Single(char c);

    [UnionCase]
    public static partial Piece Double(char c1, char c2);
}

public static class StringLiteral
{
    private static readonly string[] ExpectedOctal = ["octal"];

    private static readonly TextParser<char> SimpleEscape = Span.EqualTo("\\\"")
        .Value('"')
        .Or(Span.EqualTo(@"\\").Value('\\'))
        .Or(Span.EqualTo(@"\n").Value('\n'))
        .Or(Span.EqualTo(@"\r").Value('\r'))
        .Or(Span.EqualTo(@"\t").Value('\t'));

    private static readonly TextParser<char> OctalEscape = Character
        .EqualTo('\\')
        .IgnoreThen(input =>
        {
            var next = input.ConsumeChar();
            if (!next.HasValue || !char.IsOctalDigit(next.Value))
                return Result.Empty<char>(input, ExpectedOctal);

            var count = 0;
            var result = 0;
            TextSpan remainder;
            do
            {
                count++;
                result = 8 * result + (next.Value - '0');
                remainder = next.Remainder;
                next = remainder.ConsumeChar();
            } while (next.HasValue && char.IsOctalDigit(next.Value) && count < 3);

            return Result.Value((char)result, input, remainder);
        });

    private static readonly TextParser<char> HexEscape = Span.EqualTo("\\x")
        .IgnoreThen(input =>
        {
            var next = input.ConsumeChar();
            if (!next.HasValue || !char.IsAsciiHexDigit(next.Value))
                return Result.Empty<char>(input, ExpectedOctal);

            var result = 0;
            TextSpan remainder;
            do
            {
                result = 16 * result + GetHexDigitValue(next);
                remainder = next.Remainder;
                next = remainder.ConsumeChar();
            } while (next.HasValue && char.IsAsciiHexDigit(next.Value));

            return Result.Value((char)result, input, remainder);
        });

    private static readonly TextParser<char> Utf16Escape = Span.EqualTo("\\u")
        .IgnoreThen(input =>
        {
            var next = input.ConsumeChar();
            if (!next.HasValue || !char.IsAsciiHexDigit(next.Value))
                return Result.Empty<char>(input, ExpectedOctal);

            var count = 0;
            var result = 0;
            TextSpan remainder;
            do
            {
                count++;
                result = 16 * result + GetHexDigitValue(next);
                remainder = next.Remainder;
                next = remainder.ConsumeChar();
            } while (next.HasValue && char.IsAsciiHexDigit(next.Value) && count < 4);

            return Result.Value((char)result, input, remainder);
        });

    private static readonly TextParser<Piece> SingleCharacterEscape = SimpleEscape
        .Or(OctalEscape)
        .Or(HexEscape)
        .Or(Utf16Escape)
        .Select(Piece.Single);

    private static readonly TextParser<Piece> Utf32Escape = Span.EqualTo("\\U")
        .IgnoreThen(input =>
        {
            var next = input.ConsumeChar();
            if (!next.HasValue || !char.IsAsciiHexDigit(next.Value))
                return Result.Empty<Piece>(input, ExpectedOctal);

            var count = 0;
            var result = 0;
            TextSpan remainder;
            do
            {
                count++;
                result = 16 * result + GetHexDigitValue(next);
                remainder = next.Remainder;
                next = remainder.ConsumeChar();
            } while (next.HasValue && char.IsAsciiHexDigit(next.Value) && count < 8);

            return Result.Value(GetUtf32Chars(result), input, remainder);
        });

    private static readonly TextParser<Piece> OtherEscape = Character
        .EqualTo('\\')
        .IgnoreThen(Character.AnyChar)
        .Select(c => Piece.Double('\\', c));

    private static readonly TextParser<Piece> EscapeCharacter = SingleCharacterEscape.Or(Utf32Escape).Or(OtherEscape);

    private static int GetHexDigitValue(Result<char> next)
    {
        return next.Value switch
        {
            >= '0' and <= '9' => next.Value - '0',
            >= 'a' and <= 'f' => next.Value - 'a' + 10,
            >= 'A' and <= 'F' => next.Value - 'A' + 10,
            _ => throw new InvalidOperationException(),
        };
    }

    private static Piece GetUtf32Chars(int codePoint)
    {
        if (codePoint <= 0xFFFF)
            return Piece.Single((char)codePoint);

        var adjusted = codePoint - 0x10000;
        var high = (char)((adjusted >> 10) + 0xD800);
        var low = (char)((adjusted & 0x3FF) + 0xDC00);
        return Piece.Double(high, low);
    }

    private static readonly TextParser<char> PlainTextChar = Character.ExceptIn('"', '\n', '\r', '\\');

    private static readonly TextParser<string> EscapedString = PlainTextChar
        .Select(Piece.Single)
        .Or(EscapeCharacter)
        .Many()
        .Select(pieces =>
        {
            using var builder = new ValueStringBuilder(pieces.Length * 2);
            foreach (var p in pieces)
            {
                if (p.TryGetSingleData(out var c))
                {
                    builder.Append(c);
                }
                else if (p.TryGetDoubleData(out var first, out var second))
                {
                    builder.Append(first);
                    builder.Append(second);
                }
            }

            return builder.ToString();
        });

    public static readonly TextParser<string> UnrealStyle = EscapedString.Between(
        Character.EqualTo('"'),
        Character.EqualTo('"')
    );
}
