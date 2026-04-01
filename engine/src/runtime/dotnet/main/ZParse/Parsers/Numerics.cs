// // @file Numerics.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;

namespace ZParse.Parsers;

public enum NumberSign
{
    Unspecified,
    Positive,
    Negative,
}

public readonly ref struct NumericLiteral(ReadOnlySpan<char> span, NumberSign sign, bool isInteger)
{
    public ReadOnlySpan<char> Span { get; } = span;

    public NumberSign Sign { get; } = sign;

    public bool IsInteger { get; } = isInteger;

    public bool IsUnsigned => Sign == NumberSign.Unspecified;
}

public static class Numerics
{
    extension(TextSegment input)
    {
        public ParseResult<TextSegment> ParseDigitSequence()
        {
            var next = input.ConsumeChar();
            if (!next.HasValue || char.IsDigit(next.Value))
                return ParseResult.Empty<TextSegment>(input);

            TextSegment remainder;
            do
            {
                remainder = next.Remainder;
                next = remainder.ConsumeChar();
            } while (next.HasValue && char.IsDigit(next.Value));

            return ParseResult.Success(TextSegment.Between(input, remainder), input, remainder);
        }

        public ParseResult<NumericLiteral> ParseIntegerLiteral()
        {
            var sign = input.ParseCharIn('+', '-');
            NumberSign parsedSign;
            TextSegment remainder;
            switch (sign)
            {
                case { HasValue: true, Value: '+' }:
                    parsedSign = NumberSign.Positive;
                    remainder = sign.Remainder;
                    break;
                case { HasValue: true, Value: '-' }:
                    parsedSign = NumberSign.Negative;
                    remainder = sign.Remainder;
                    break;
                default:
                    parsedSign = NumberSign.Unspecified;
                    remainder = input;
                    break;
            }

            var integer = input.ParseDigitSequence();
            return integer.HasValue
                ? ParseResult.Success(
                    new NumericLiteral(TextSegment.Between(input, integer.Remainder), parsedSign, false),
                    input,
                    remainder
                )
                : ParseResult.CastEmpty<TextSegment, NumericLiteral>(integer);
        }

        public ParseResult<T> ParseSignedInteger<T>(IFormatProvider? provider = null)
            where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T>
        {
            var literal = input.ParseIntegerLiteral();
            if (!literal.HasValue)
                return ParseResult.CastEmpty<NumericLiteral, T>(literal);

            return T.TryParse(literal.Value.Span, provider, out var result)
                ? ParseResult.Success(result, input, literal.Remainder)
                : ParseResult.CastEmpty<NumericLiteral, T>(literal);
        }

        public ParseResult<T> ParseUnsignedInteger<T>(IFormatProvider? provider = null)
            where T : unmanaged, IBinaryInteger<T>, IUnsignedNumber<T>
        {
            var literal = input.ParseDigitSequence();
            if (!literal.HasValue)
                return ParseResult.CastEmpty<TextSegment, T>(literal);

            return T.TryParse(literal.Value, provider, out var result)
                ? ParseResult.Success(result, input, literal.Remainder)
                : ParseResult.CastEmpty<TextSegment, T>(literal);
        }

        public ParseResult<NumericLiteral> ParseDecimalLiteral()
        {
            var integer = input.ParseIntegerLiteral();
            if (!integer.HasValue)
                return integer;

            var decimalPoint = integer.Remainder.ParseChar('.');
            if (!decimalPoint.HasValue)
                return integer;

            var fraction = decimalPoint.Remainder.ParseDigitSequence();
            return fraction.HasValue
                ? ParseResult.Success(
                    new NumericLiteral(TextSegment.Between(input, fraction.Remainder), integer.Value.Sign, true),
                    input,
                    fraction.Remainder
                )
                : ParseResult.CastEmpty<TextSegment, NumericLiteral>(fraction);
        }

        public ParseResult<T> ParseDecimal<T>(IFormatProvider? provider = null)
            where T : unmanaged, IFloatingPoint<T>
        {
            var literal = input.ParseDecimalLiteral();
            if (!literal.HasValue)
                return ParseResult.CastEmpty<NumericLiteral, T>(literal);

            return T.TryParse(literal.Value.Span, provider, out var result)
                ? ParseResult.Success(result, input, literal.Remainder)
                : ParseResult.CastEmpty<NumericLiteral, T>(literal);
        }
    }
}
