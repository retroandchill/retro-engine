// // @file Characters.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ZParse.Parsers;

public static class Characters
{
    extension(ParseCursor input)
    {
        public ParseResult<char> ParseChar(char c)
        {
            var next = input.Advance();
            return next.HasValue && next.Value == c ? next : ParseResult.Empty<char>(input);
        }

        public ParseResult<char> ParseCharIn(params ReadOnlySpan<char> chars)
        {
            var next = input.Advance();
            return next.HasValue && chars.Contains(next.Value) ? next : ParseResult.Empty<char>(input);
        }

        public ParseResult<char> ParseAnyCharExcept(char c)
        {
            var next = input.Advance();
            return next.HasValue && next.Value != c ? next : ParseResult.Empty<char>(input);
        }

        public ParseResult<char> ParseAnyCharExceptIn(params ReadOnlySpan<char> chars)
        {
            var next = input.Advance();
            return next.HasValue && !chars.Contains(next.Value) ? next : ParseResult.Empty<char>(input);
        }

        public ParseResult<ReadOnlySpan<char>> ParseWhitespace()
        {
            var next = input.Advance();
            if (!next.HasValue || !char.IsWhiteSpace(next.Value))
                return ParseResult.Empty<ReadOnlySpan<char>>(input);

            ParseCursor remainder;
            do
            {
                remainder = next.Remainder;
                next = remainder.Advance();
            } while (next.HasValue && char.IsWhiteSpace(next.Value));

            return ParseResult.Success(ParseCursor.Between(input, remainder), input, remainder);
        }

        public ParseResult<ReadOnlySpan<char>> ParseOptionalWhitespace()
        {
            var next = input.Advance();
            if (!next.HasValue || !char.IsWhiteSpace(next.Value))
                return ParseResult.Success(ReadOnlySpan<char>.Empty, input, input);

            ParseCursor remainder;
            do
            {
                remainder = next.Remainder;
                next = remainder.Advance();
            } while (next.HasValue && char.IsWhiteSpace(next.Value));

            return ParseResult.Success(ParseCursor.Between(input, remainder), input, remainder);
        }

        public ParseResult<ReadOnlySpan<char>> ParseWhitespaceAndChar(char c)
        {
            var whitespace = input.ParseOptionalWhitespace().Remainder;
            var parsedChar = whitespace.ParseChar(c);
            return parsedChar.HasValue
                ? ParseResult.Success(ParseCursor.Between(input, parsedChar.Remainder), input, parsedChar.Remainder)
                : ParseResult.Empty<ReadOnlySpan<char>>(input);
        }
    }
}
