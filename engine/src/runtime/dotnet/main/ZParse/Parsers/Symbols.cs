// // @file Symbols.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using ZParse.Util;

namespace ZParse.Parsers;

public static class Symbols
{
    extension(ParseCursor input)
    {
        public ParseResult<ReadOnlySpan<char>> ParseSymbol(ReadOnlySpan<char> symbol)
        {
            if (symbol.IsEmpty)
                throw new ArgumentException("Symbol cannot be empty.", nameof(symbol));

            var remainder = input;
            foreach (var c in symbol)
            {
                var next = remainder.Advance();
                if (!next.HasValue || next.Value != c)
                    return ParseResult.CastEmpty<char, ReadOnlySpan<char>>(next);

                remainder = next.Remainder;
            }

            return ParseResult.Success(ParseCursor.Between(input, remainder), input, remainder);
        }

        public ParseResult<ReadOnlySpan<char>> ParseSymbolIgnoreCase(ReadOnlySpan<char> symbol)
        {
            if (symbol.IsEmpty)
                throw new ArgumentException("Symbol cannot be empty.", nameof(symbol));

            var remainder = input;
            foreach (var c in symbol)
            {
                var next = remainder.Advance();
                if (!next.HasValue || char.ToUpper(next.Value) != char.ToUpper(c))
                    return ParseResult.CastEmpty<char, ReadOnlySpan<char>>(next);

                remainder = next.Remainder;
            }

            return ParseResult.Success(ParseCursor.Between(input, remainder), input, remainder);
        }

        public ParseResult<ReadOnlySpan<char>> ParseNChars(int characterCount)
        {
            var remainder = input;
            for (var i = 0; i < characterCount; i++)
            {
                var next = remainder.Advance();
                if (!next.HasValue)
                    return ParseResult.CastEmpty<char, ReadOnlySpan<char>>(next);

                remainder = next.Remainder;
            }

            return ParseResult.Success(ParseCursor.Between(input, remainder), input, remainder);
        }

        public ParseResult<ReadOnlySpan<char>> ParseUntilChar(char c)
        {
            var next = input.Advance();
            if (!next.HasValue || next.Value == c)
                return ParseResult.CastEmpty<char, ReadOnlySpan<char>>(next);

            ParseCursor remainder;
            do
            {
                remainder = next.Remainder;
                next = remainder.Advance();
            } while (next.HasValue && next.Value != c);

            return ParseResult.Success(ParseCursor.Between(input, remainder), input, remainder);
        }

        public ParseResult<ReadOnlySpan<char>> ParseUntilCharIn(params ReadOnlySpan<char> chars)
        {
            var next = input.Advance();
            if (!next.HasValue || chars.Contains(next.Value))
                return ParseResult.CastEmpty<char, ReadOnlySpan<char>>(next);

            ParseCursor remainder;
            do
            {
                remainder = next.Remainder;
                next = remainder.Advance();
            } while (next.HasValue && !chars.Contains(next.Value));

            return ParseResult.Success(ParseCursor.Between(input, remainder), input, remainder);
        }

        public ParseResult<ReadOnlySpan<char>> ParseIdentifier()
        {
            var next = input.Advance();
            if (!next.HasValue || !char.IsIdentifier(next.Value))
                return ParseResult.CastEmpty<char, ReadOnlySpan<char>>(next);

            ParseCursor remainder;
            do
            {
                remainder = next.Remainder;
                next = remainder.Advance();
            } while (next.HasValue && char.IsIdentifier(next.Value));

            return ParseResult.Success(ParseCursor.Between(input, remainder), input, remainder);
        }

        public ParseResult<bool> ParseBool()
        {
            return input.ParseSymbol("true").Select(_ => true).OrElse(i => i.ParseSymbol("false").Select(_ => false));
        }

        public ParseResult<T> ParseEnum<T>(string? scope = null)
            where T : unmanaged, Enum
        {
            ParseCursor remainder;
            if (!string.IsNullOrEmpty(scope))
            {
                var scopeIdentifier = input.ParseSymbol(scope);
                if (!scopeIdentifier.HasValue)
                    return ParseResult.CastEmpty<ReadOnlySpan<char>, T>(scopeIdentifier);

                remainder = scopeIdentifier.Remainder;
            }
            else
            {
                remainder = input;
            }

            var identifier = remainder.ParseIdentifier();
            if (!identifier.HasValue)
                return ParseResult.CastEmpty<ReadOnlySpan<char>, T>(identifier);

            return Enum.TryParse(identifier.Value, out T result)
                ? ParseResult.Success(result, input, identifier.Remainder)
                : ParseResult.CastEmpty<ReadOnlySpan<char>, T>(identifier);
        }
    }
}
