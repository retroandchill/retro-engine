// // @file StringTokens.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ZParse;

public static class Parsers
{
    extension(TokenCursor input)
    {
        public TokenResult<Unit> ParseToken(Func<char, ParseState> predicate)
        {
            if (input.IsAtEnd)
                return TokenResult.Empty<Unit>(input);

            var next = input.Advance();
            if (!next.IsSuccess)
                return TokenResult.Empty<Unit>(input);

            var remaining = input;
            do
            {
                var result = predicate(next.Value);
                bool shouldStop;
                switch (result)
                {
                    case ParseState.Continue:
                        remaining = next.Remainder;
                        shouldStop = false;
                        break;
                    case ParseState.StopBefore:
                        shouldStop = true;
                        break;
                    case ParseState.StopAfter:
                        remaining = next.Remainder;
                        shouldStop = true;
                        break;
                    case ParseState.Cancel:
                        return TokenResult.Empty<Unit>(input);
                    default:
                        throw new InvalidOperationException("Invalid parse state.");
                }

                if (shouldStop)
                    break;

                next = input.Advance();
            } while (next.IsSuccess);

            return remaining != input
                ? TokenResult.Success(Unit.Value, input, remaining)
                : TokenResult.Empty<Unit>(input);
        }

        public TokenResult<Unit> ParseSymbol(ReadOnlySpan<char> symbol)
        {
            if (symbol.IsEmpty)
                throw new ArgumentException("Symbol cannot be empty.", nameof(symbol));

            var remainder = input;
            foreach (var c in symbol)
            {
                var next = remainder.Advance();
                if (!next.IsSuccess || next.Value != c)
                    return TokenResult.Empty<Unit>(input);

                remainder = next.Remainder;
            }

            return TokenResult.Success(Unit.Value, input, remainder);
        }

        public TokenResult<Unit> ParseSymbolIgnoreCase(ReadOnlySpan<char> symbol)
        {
            if (symbol.IsEmpty)
                throw new ArgumentException("Symbol cannot be empty.", nameof(symbol));

            var remainder = input;
            foreach (var c in symbol)
            {
                var next = remainder.Advance();
                if (!next.IsSuccess || char.ToUpper(next.Value) != char.ToUpper(c))
                    return TokenResult.Empty<Unit>(input);

                remainder = next.Remainder;
            }

            return TokenResult.Success(Unit.Value, input, remainder);
        }

        public TokenResult<char> ParseChar(char c)
        {
            var next = input.Advance();
            return next.IsSuccess && next.Value == c ? next : TokenResult.Empty<char>(input);
        }

        public TokenResult<Unit> ParseWhitespace()
        {
            var next = input.Advance();
            if (!next.IsSuccess || !char.IsWhiteSpace(next.Value))
                return TokenResult.Empty<Unit>(input, next.Remainder);

            TokenCursor remainder;
            do
            {
                remainder = next.Remainder;
                next = remainder.Advance();
            } while (next.IsSuccess && char.IsWhiteSpace(next.Value));

            return TokenResult.Success(Unit.Value, input, remainder);
        }

        public TokenResult<Unit> ParseOptionalWhitespace()
        {
            var next = input.Advance();
            if (!next.IsSuccess || !char.IsWhiteSpace(next.Value))
                return TokenResult.Success(Unit.Value, input, input);

            TokenCursor remainder;
            do
            {
                remainder = next.Remainder;
                next = remainder.Advance();
            } while (next.IsSuccess && char.IsWhiteSpace(next.Value));

            return TokenResult.Success(Unit.Value, input, remainder);
        }

        public TokenResult<Unit> GenerateToken(int characterCount)
        {
            var remainder = input;
            for (var i = 0; i < characterCount; i++)
            {
                var next = remainder.Advance();
                if (!next.IsSuccess)
                    return TokenResult.Empty<Unit>(input, remainder);

                remainder = next.Remainder;
            }

            return TokenResult.Success(Unit.Value, input, remainder);
        }
    }
}
