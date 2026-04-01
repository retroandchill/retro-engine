// // @file Symbols.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using ZParse.Util;

namespace ZParse.Parsers;

public static class Symbols
{
    extension(TextSegment input)
    {
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

        public ParseResult<TextSegment> ParseSymbolIgnoreCase(ReadOnlySpan<char> symbol)
        {
            if (symbol.IsEmpty)
                throw new ArgumentException("Symbol cannot be empty.", nameof(symbol));

            var remainder = input;
            foreach (var c in symbol)
            {
                var next = remainder.ConsumeChar();
                if (!next.HasValue || char.ToUpper(next.Value) != char.ToUpper(c))
                    return ParseResult.CastEmpty<char, TextSegment>(next);

                remainder = next.Remainder;
            }

            return ParseResult.Success(TextSegment.Between(input, remainder), input, remainder);
        }

        public ParseResult<TextSegment> ParseNChars(int characterCount)
        {
            var remainder = input;
            for (var i = 0; i < characterCount; i++)
            {
                var next = remainder.ConsumeChar();
                if (!next.HasValue)
                    return ParseResult.CastEmpty<char, TextSegment>(next);

                remainder = next.Remainder;
            }

            return ParseResult.Success(TextSegment.Between(input, remainder), input, remainder);
        }

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

        public ParseResult<bool> ParseBool()
        {
            return input.ParseSymbol("true").Select(_ => true).OrElse(i => i.ParseSymbol("false").Select(_ => false));
        }

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
}
