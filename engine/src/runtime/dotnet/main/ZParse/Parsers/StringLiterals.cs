// // @file Strings.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using LinkDotNet.StringBuilder;
using ZParse.Util;

namespace ZParse.Parsers;

public static class StringLiterals
{
    extension(ParseCursor input)
    {
        public ParseResult<string> ParseQuotedString()
        {
            var builder = new ValueStringBuilder();
            try
            {
                var result = input.ParseQuotedString(ref builder);
                return result.HasValue
                    ? ParseResult.Success(builder.ToString(), input, result.Remainder)
                    : ParseResult.CastEmpty<Unit, string>(result);
            }
            finally
            {
                builder.Dispose();
            }
        }

        public ParseResult<Unit> ParseQuotedString(scoped ref ValueStringBuilder builder)
        {
            var openQuote = input.ParseChar('"');
            if (!openQuote.HasValue)
                return ParseResult.CastEmpty<char, Unit>(openQuote);

            const string stopCharacters = "\"\n\r";
            const string stopAndEscapeCharacters = $"{stopCharacters}\\";

            var remainder = input;
            while (true)
            {
                var unescaped = remainder.ParseUntilCharIn(stopAndEscapeCharacters);
                builder.Append(ParseCursor.Between(unescaped.Cursor, unescaped.Remainder));
                remainder = unescaped.Remainder;

                var next = remainder.Advance();
                if (!next.HasValue)
                {
                    remainder = next.Remainder;
                    break;
                }

                if (next.Value != '\\')
                    break;

                remainder = next.Remainder;
                next = remainder.Advance();
                if (!next.HasValue)
                    return ParseResult.CastEmpty<char, Unit>(next);

                var ch = next.Value;
                switch (ch)
                {
                    case '\\':
                        builder.Append('\\');
                        remainder = next.Remainder;
                        break;
                    case '"':
                        builder.Append('"');
                        remainder = next.Remainder;
                        break;
                    case '\'':
                        builder.Append('\'');
                        remainder = next.Remainder;
                        break;
                    case 'n':
                        builder.Append('\n');
                        remainder = next.Remainder;
                        break;
                    case 'r':
                        builder.Append('\r');
                        remainder = next.Remainder;
                        break;
                    case 't':
                        builder.Append('\t');
                        remainder = next.Remainder;
                        break;
                    default:
                        if (char.IsOctalDigit(ch))
                        {
                            var octalDigits = 0;
                            var result = 0;
                            while (next.HasValue && char.IsOctalDigit(next.Value) && octalDigits < 3)
                            {
                                result = result * 8 + char.ToOctalValue(next.Value);
                                octalDigits++;
                                remainder = next.Remainder;
                                next = remainder.Advance();
                            }

                            builder.Append((char)result);
                        }
                        else if (ch == 'x' && next.Remainder.Peek() is { } nextChar && char.IsAsciiHexDigit(nextChar))
                        {
                            remainder = next.Remainder;
                            next = remainder.Advance();

                            var result = 0;
                            while (next.HasValue && char.IsAsciiHexDigit(next.Value))
                            {
                                result = result * 16 + char.ToHexValue(next.Value);
                                remainder = next.Remainder;
                                next = remainder.Advance();
                            }

                            builder.Append((char)result);
                        }
                        else if (ch == 'u' && next.Remainder.Peek() is { } nextChar2 && char.IsAsciiHexDigit(nextChar2))
                        {
                            remainder = next.Remainder;
                            next = remainder.Advance();

                            var digits = 0;
                            var result = 0;
                            while (next.HasValue && char.IsAsciiHexDigit(next.Value) && digits < 4)
                            {
                                result = result * 16 + char.ToHexValue(next.Value);
                                digits++;
                                remainder = next.Remainder;
                                next = remainder.Advance();
                            }

                            builder.Append((char)result);
                        }
                        else if (ch == 'U' && next.Remainder.Peek() is { } nextChar3 && char.IsAsciiHexDigit(nextChar3))
                        {
                            remainder = next.Remainder;
                            next = remainder.Advance();

                            var digits = 0;
                            var result = 0;
                            while (next.HasValue && char.IsAsciiHexDigit(next.Value) && digits < 8)
                            {
                                result = result * 16 + char.ToHexValue(next.Value);
                                digits++;
                                remainder = next.Remainder;
                                next = remainder.Advance();
                            }

                            if (result <= 0xFFFF)
                            {
                                builder.Append((char)result);
                            }
                            else
                            {
                                var adjusted = result - 0x10000;
                                var high = (char)((adjusted >> 10) + 0xD800);
                                var low = (char)((adjusted & 0x3FF) + 0xDC00);
                                builder.Append(high);
                                builder.Append(low);
                            }
                        }
                        else
                        {
                            builder.Append('\\');
                            builder.Append(ch);
                            remainder = next.Remainder;
                        }

                        break;
                }
            }

            var closeQuote = remainder.Advance();
            return closeQuote.HasValue
                ? ParseResult.Success(Unit.Value, input, remainder)
                : ParseResult.CastEmpty<char, Unit>(closeQuote);
        }
    }
}
