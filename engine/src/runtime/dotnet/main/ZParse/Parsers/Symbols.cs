// // @file Symbols.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using ZParse.Util;

namespace ZParse.Parsers;

public static class Symbols
{
    private const string UseDeclarativeParsers = "Use declarative parsers instead of this syntax";

    extension(TextSegment input)
    {
        [Obsolete(UseDeclarativeParsers)]
        public ParseResult<TextSegment> ParseSymbol(ReadOnlySpan<char> symbol)
        {
            if (symbol.IsEmpty)
                throw new ArgumentException("Symbol cannot be empty.", nameof(symbol));

            var remainder = input;
            foreach (var c in symbol)
            {
                var next = remainder.ConsumeChar();
                if (!next.HasValue || next.Value != c)
                    return ParseResult.CastEmpty<char, TextSegment>(next);

                remainder = next.Remainder;
            }

            return ParseResult.Success(TextSegment.Between(input, remainder), input, remainder);
        }

        [Obsolete(UseDeclarativeParsers)]
        public ParseResult<TextSegment> ParseUntilChar(char c)
        {
            var next = input.ConsumeChar();
            if (!next.HasValue || next.Value == c)
                return ParseResult.CastEmpty<char, TextSegment>(next);

            TextSegment remainder;
            do
            {
                remainder = next.Remainder;
                next = remainder.ConsumeChar();
            } while (next.HasValue && next.Value != c);

            return ParseResult.Success(TextSegment.Between(input, remainder), input, remainder);
        }

        [Obsolete(UseDeclarativeParsers)]
        public ParseResult<TextSegment> ParseUntilCharIn(params ReadOnlySpan<char> chars)
        {
            var next = input.ConsumeChar();
            if (!next.HasValue || chars.Contains(next.Value))
                return ParseResult.CastEmpty<char, TextSegment>(next);

            TextSegment remainder;
            do
            {
                remainder = next.Remainder;
                next = remainder.ConsumeChar();
            } while (next.HasValue && !chars.Contains(next.Value));

            return ParseResult.Success(TextSegment.Between(input, remainder), input, remainder);
        }

        [Obsolete(UseDeclarativeParsers)]
        public ParseResult<TextSegment> ParseIdentifier()
        {
            var next = input.ConsumeChar();
            if (!next.HasValue || !char.IsIdentifier(next.Value))
                return ParseResult.CastEmpty<char, TextSegment>(next);

            TextSegment remainder;
            do
            {
                remainder = next.Remainder;
                next = remainder.ConsumeChar();
            } while (next.HasValue && char.IsIdentifier(next.Value));

            return ParseResult.Success(TextSegment.Between(input, remainder), input, remainder);
        }

        [Obsolete(UseDeclarativeParsers)]
        public ParseResult<bool> ParseBool()
        {
            return input.ParseSymbol("true").Select(_ => true).OrElse(i => i.ParseSymbol("false").Select(_ => false));
        }

        [Obsolete(UseDeclarativeParsers)]
        public ParseResult<T> ParseEnum<T>(string? scope = null)
            where T : unmanaged, Enum
        {
            TextSegment remainder;
            if (!string.IsNullOrEmpty(scope))
            {
                var scopeIdentifier = input.ParseSymbol(scope);
                if (!scopeIdentifier.HasValue)
                    return ParseResult.CastEmpty<TextSegment, T>(scopeIdentifier);

                remainder = scopeIdentifier.Remainder;
            }
            else
            {
                remainder = input;
            }

            var identifier = remainder.ParseIdentifier();
            if (!identifier.HasValue)
                return ParseResult.CastEmpty<TextSegment, T>(identifier);

            return Enum.TryParse(identifier.Value, out T result)
                ? ParseResult.Success(result, input, identifier.Remainder)
                : ParseResult.CastEmpty<TextSegment, T>(identifier);
        }
    }

    public static TextParser<TextSegment> Identifier { get; } =
        Sequences.MatchedBy(Characters.LetterOrDigitOrUnderscore.IgnoreAtLeastOnce());

    public static TextParser<bool> Boolean { get; } =
        Sequences.EqualTo("true").Value(true).Or(Sequences.EqualTo("false").Value(false));

    public static TextParser<T> EnumLiteral<T>(string? prefix = null)
        where T : unmanaged, Enum
    {
        var parseEnumValue = Identifier.TrySelect((TextSegment s, out T r) => Enum.TryParse(s, out r));
        return prefix is null ? parseEnumValue : Sequences.EqualTo(prefix).IgnoreThen(parseEnumValue);
    }
}
