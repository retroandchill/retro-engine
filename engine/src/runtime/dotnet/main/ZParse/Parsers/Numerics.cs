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

public readonly ref struct NumericLiteral(TextSegment segment, NumberSign sign, bool isInteger)
{
    public TextSegment Segment { get; } = segment;

    public NumberSign Sign { get; } = sign;

    public bool IsInteger { get; } = isInteger;

    public bool IsUnsigned => Sign == NumberSign.Unspecified;
}

public static class Numerics
{
    private const string UseDeclarativeParsers = "Use declarative parsers instead of this syntax";

    extension(TextSegment input)
    {
        [Obsolete(UseDeclarativeParsers)]
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

        [Obsolete(UseDeclarativeParsers)]
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

        [Obsolete(UseDeclarativeParsers)]
        public ParseResult<T> ParseSignedInteger<T>(IFormatProvider? provider = null)
            where T : unmanaged, IBinaryInteger<T>, ISignedNumber<T>
        {
            var literal = input.ParseIntegerLiteral();
            if (!literal.HasValue)
                return ParseResult.CastEmpty<NumericLiteral, T>(literal);

            return T.TryParse(literal.Value.Segment, provider, out var result)
                ? ParseResult.Success(result, input, literal.Remainder)
                : ParseResult.CastEmpty<NumericLiteral, T>(literal);
        }

        [Obsolete(UseDeclarativeParsers)]
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
    }

    public static TextParser<TextSegment> DigitSequence { get; } =
        input =>
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
        };

    private static readonly TextParser<char> Sign = Characters.In('+', '-');

    private static readonly TextParser<char> DecimalPoint = Characters.EqualTo('.');

    public static TextParser<NumericLiteral> IntegerLiteral { get; } =
        input =>
        {
            var sign = Sign(input);
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

            var integer = DigitSequence(remainder);
            return integer.HasValue
                ? ParseResult.Success(
                    new NumericLiteral(input.Until(integer.Remainder), parsedSign, false),
                    input,
                    integer.Remainder
                )
                : ParseResult.CastEmpty<TextSegment, NumericLiteral>(integer);
        };

    public static TextParser<NumericLiteral> DecimalLiteral { get; } =
        input =>
        {
            var integer = IntegerLiteral(input);
            if (!integer.HasValue)
                return integer;

            var decimalPoint = DecimalPoint(integer.Remainder);
            if (!decimalPoint.HasValue)
                return integer;

            var fraction = DigitSequence(decimalPoint.Remainder);
            return fraction.HasValue
                ? ParseResult.Success(
                    new NumericLiteral(TextSegment.Between(input, fraction.Remainder), integer.Value.Sign, true),
                    input,
                    fraction.Remainder
                )
                : ParseResult.CastEmpty<TextSegment, NumericLiteral>(fraction);
        };

    public static TextParser<T> SignedInteger<T>(IFormatProvider? provider = null)
        where T : unmanaged, ISignedNumber<T>, IBinaryInteger<T>
    {
        return input =>
        {
            var literal = IntegerLiteral(input);
            if (!literal.HasValue)
                return ParseResult.CastEmpty<NumericLiteral, T>(literal);

            return T.TryParse(literal.Value.Segment, provider, out var result)
                ? ParseResult.Success(result, input, literal.Remainder)
                : ParseResult.Empty<T>(input, "invalid integer literal");
        };
    }

    public static TextParser<T> UnsignedInteger<T>(IFormatProvider? provider = null)
        where T : unmanaged, IUnsignedNumber<T>, IBinaryInteger<T>
    {
        return input =>
        {
            var literal = DigitSequence(input);
            if (!literal.HasValue)
                return ParseResult.CastEmpty<TextSegment, T>(literal);

            return T.TryParse(literal.Value, provider, out var result)
                ? ParseResult.Success(result, input, literal.Remainder)
                : ParseResult.Empty<T>(input, "invalid integer literal");
        };
    }

    public static TextParser<T> DecimalNumber<T>(IFormatProvider? provider = null)
        where T : unmanaged, IFloatingPoint<T>
    {
        return input =>
        {
            var literal = DecimalLiteral(input);
            if (!literal.HasValue)
                return ParseResult.CastEmpty<NumericLiteral, T>(literal);

            return T.TryParse(literal.Value.Segment, provider, out var result)
                ? ParseResult.Success(result, input, literal.Remainder)
                : ParseResult.Empty<T>(input, "invalid decimal literal");
        };
    }

    public static TextParser<sbyte> SByte { get; } = SignedInteger<sbyte>();

    public static TextParser<short> Short { get; } = SignedInteger<short>();

    public static TextParser<int> Int { get; } = SignedInteger<int>();

    public static TextParser<long> Long { get; } = SignedInteger<long>();

    public static TextParser<byte> Byte { get; } = UnsignedInteger<byte>();

    public static TextParser<ushort> UShort { get; } = UnsignedInteger<ushort>();

    public static TextParser<uint> UInt { get; } = UnsignedInteger<uint>();

    public static TextParser<ulong> ULong { get; } = UnsignedInteger<ulong>();

    public static TextParser<float> Float { get; } = DecimalNumber<float>();

    public static TextParser<double> Double { get; } = DecimalNumber<double>();

    public static TextParser<decimal> Decimal { get; } = DecimalNumber<decimal>();
}
