// // @file Sequences.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using ZParse.Display;

namespace ZParse.Parsers;

public static class Sequences
{
    public static TextParser<TextSegment> EqualTo(string expected) => EqualTo(expected.AsMemory());

    public static TextParser<TextSegment> EqualTo(ReadOnlyMemory<char> expected)
    {
        if (expected.Span.IsEmpty)
            throw new ArgumentException("Expected sequence cannot be empty", nameof(expected));

        var staticExpectation = ImmutableArray.Create<ParseExpectation>(Presentation.FormatLiteral(expected.Span));
        var dynamicExpectation = ImmutableArray.Create(
            new ParseExpectation((_, remainder) => Presentation.FormatLiteral(remainder[0]))
        );
        return input =>
        {
            var remainder = input;
            foreach (var c in expected.Span)
            {
                var next = remainder.ConsumeChar();
                if (!next.HasValue || next.Value != c)
                {
                    return remainder == input
                        ? ParseResult.Empty<TextSegment>(input, staticExpectation)
                        : ParseResult.Empty<TextSegment>(input, remainder, dynamicExpectation);
                }

                remainder = next.Remainder;
            }

            return ParseResult.Success(TextSegment.Between(input, remainder), input, remainder);
        };
    }

    public static TextParser<TextSegment> EqualToIgnoreCase(string expected) => EqualToIgnoreCase(expected.AsMemory());

    public static TextParser<TextSegment> EqualToIgnoreCase(ReadOnlyMemory<char> expected)
    {
        if (expected.Span.IsEmpty)
            throw new ArgumentException("Expected sequence cannot be empty", nameof(expected));

        var staticExpectation = ImmutableArray.Create<ParseExpectation>(Presentation.FormatLiteral(expected.Span));
        var dynamicExpectation = ImmutableArray.Create(
            new ParseExpectation((_, remainder) => Presentation.FormatLiteral(remainder[0]))
        );
        return input =>
        {
            var remainder = input;
            foreach (var c in expected.Span)
            {
                var next = remainder.ConsumeChar();
                if (!next.HasValue || char.ToUpperInvariant(next.Value) != char.ToUpperInvariant(c))
                {
                    return remainder == input
                        ? ParseResult.Empty<TextSegment>(input, staticExpectation)
                        : ParseResult.Empty<TextSegment>(input, remainder, dynamicExpectation);
                }

                remainder = next.Remainder;
            }

            return ParseResult.Success(TextSegment.Between(input, remainder), input, remainder);
        };
    }

    public static TextParser<TextSegment> Length(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);

        var staticExpectations = ImmutableArray.Create<ParseExpectation>($"sequence of length {length}");
        var dynamicExpectations = ImmutableArray.Create(
            new ParseExpectation(
                (input, remainder) =>
                {
                    var remaining = length - input.Position.Index + remainder.Position.Index;
                    return $"{remaining} more {Friendly.Pluralize("character", remaining)}";
                }
            )
        );
        return input =>
        {
            var remainder = input;
            for (var i = 0; i < length; i++)
            {
                var next = remainder.ConsumeChar();
                if (!next.HasValue)
                {
                    return i == 0
                        ? ParseResult.Empty<TextSegment>(input, staticExpectations)
                        : ParseResult.Empty<TextSegment>(input, remainder, dynamicExpectations);
                }

                remainder = next.Remainder;
            }

            return ParseResult.Success(input.Until(remainder), input, remainder);
        };
    }

    public static TextParser<TextSegment> MatchedBy<T>(TextParser<T> parser)
        where T : allows ref struct
    {
        return input =>
        {
            var result = parser(input);
            return result.HasValue
                ? ParseResult.Success(input.Until(result.Remainder), input, result.Remainder)
                : ParseResult.CastEmpty<T, TextSegment>(result);
        };
    }

    public static TextParser<TextSegment> While(
        Func<char, bool> predicate,
        string name = "at least one character matching condition"
    )
    {
        var expectations = ImmutableArray.Create<ParseExpectation>(name);
        return input =>
        {
            var next = input.ConsumeChar();
            var consumed = false;
            var remainder = input;
            while (next.HasValue && predicate(next.Value))
            {
                consumed = true;
                remainder = next.Remainder;
                next = remainder.ConsumeChar();
            }

            return consumed
                ? ParseResult.Success(input.Until(remainder), input, remainder)
                : ParseResult.Empty<TextSegment>(input, expectations);
        };
    }

    public static TextParser<TextSegment> Until(
        Func<char, bool> predicate,
        string name = "at least one character until"
    )
    {
        return While(i => !predicate(i), name);
    }

    public static TextParser<TextSegment> UntilChar(char ch)
    {
        return Until(c => c == ch, $"at least one character until {Presentation.FormatLiteral(ch)}");
    }

    public static TextParser<TextSegment> Whitespace { get; } = While(char.IsWhiteSpace, "whitespace");

    public static TextParser<TextSegment> OptionalWhitespace { get; } = Whitespace.OrElseDefault();

    public static TextParser<TextSegment> NonWhitespace { get; } = Until(char.IsWhiteSpace, "non-whitespace");
}
