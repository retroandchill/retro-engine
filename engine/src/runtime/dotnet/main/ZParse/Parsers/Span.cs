// // @file Span.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using ZParse.Display;

namespace ZParse.Parsers;

public static class Span
{
    public static StringParser<StringView> Length(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        ArgumentOutOfRangeException.ThrowIfNegative(length);

        var expectations = ImmutableArray.Create($"span of length {length}");
        return input =>
        {
            var remainder = input;
            for (var i = 0; i < length; ++i)
            {
                var ch = remainder.TryGetNext();
                if (!ch.Success)
                {
                    return Result.Empty<StringView>(ch.Input, expectations);
                }

                remainder = ch.Remainder;
            }

            return Result.Value(input.Until(remainder), input, remainder);
        };
    }

    public static StringParser<StringView> EqualTo(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return EqualTo(text.AsMemory());
    }

    public static StringParser<StringView> EqualTo(ReadOnlyMemory<char> text)
    {
        var expectations = ImmutableArray.Create(Presentation.FormatLiteral(text.Span));
        return input =>
        {
            var remainder = input;
            foreach (var t in text.Span)
            {
                var ch = remainder.TryGetNext();
                if (!ch.Success || ch.Value != t)
                {
                    return Result.Empty<StringView>(ch.Input, expectations);
                }
                remainder = ch.Remainder;
            }
            return Result.Value(input.Until(remainder), input, remainder);
        };
    }

    public static StringParser<StringView> EqualToIgnoreCase(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return EqualToIgnoreCase(text.AsMemory());
    }

    public static StringParser<StringView> EqualToIgnoreCase(ReadOnlyMemory<char> text)
    {
        var expectations = ImmutableArray.Create(Presentation.FormatLiteral(text.Span));
        return input =>
        {
            var remainder = input;
            foreach (var t in text.Span)
            {
                var ch = remainder.TryGetNext();
                if (!ch.Success || char.ToUpperInvariant(ch.Value) != char.ToUpperInvariant(t))
                {
                    return Result.Empty<StringView>(ch.Input, expectations);
                }
                remainder = ch.Remainder;
            }
            return Result.Value(input.Until(remainder), input, remainder);
        };
    }

    public static StringParser<StringView> EqualTo(char ch)
    {
        var expectations = ImmutableArray.Create(Presentation.FormatLiteral(ch));
        return input =>
        {
            var result = input.TryGetNext();
            if (result.Success && result.Value == ch)
                return Result.Value(input.Until(result.Remainder), input, result.Remainder);
            return Result.Empty<StringView>(input, expectations);
        };
    }

    public static StringParser<StringView> EqualToIgnoreCase(char ch)
    {
        var chToUpper = char.ToUpperInvariant(ch);
        var expectations = ImmutableArray.Create(Presentation.FormatLiteral(ch));
        return input =>
        {
            var result = input.TryGetNext();
            if (result.Success && char.ToUpperInvariant(result.Value) == chToUpper)
                return Result.Value(input.Until(result.Remainder), input, result.Remainder);
            return Result.Empty<StringView>(input, expectations);
        };
    }

    public static StringParser<StringView> WithoutAny(Func<char, bool> predicate)
    {
        return predicate is not null
            ? WithAll(ch => !predicate(ch))
            : throw new ArgumentNullException(nameof(predicate));
    }

    public static StringParser<StringView> WithAll(Func<char, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return input =>
        {
            var next = input.TryGetNext();
            while (next.Success && predicate(next.Value))
            {
                next = next.Remainder.TryGetNext();
            }

            return next.Input == input
                ? Result.Empty<StringView>(input)
                : Result.Value(input.Until(next.Input), input, next.Input);
        };
    }

    public static StringParser<StringView> WhiteSpace { get; } =
        input =>
        {
            var next = input.TryGetNext();
            while (next.Success && char.IsWhiteSpace(next.Value))
            {
                next = next.Remainder.TryGetNext();
            }

            return next.Input == input
                ? Result.Empty<StringView>(input)
                : Result.Value(input.Until(next.Input), input, next.Input);
        };

    public static StringParser<StringView> NonWhiteSpace { get; } = WithoutAny(char.IsWhiteSpace);

    public static StringParser<StringView> MatchedBy<T>(StringParser<T> parser)
        where T : allows ref struct
    {
        return i =>
        {
            var result = parser(i);

            return result.Success
                ? Result.Value(i.Until(result.Remainder), i, result.Remainder)
                : Result.CastEmpty<T, StringView>(result);
        };
    }
}
