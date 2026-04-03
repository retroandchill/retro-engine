// // @file Combinators.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ZParse;

public delegate bool TrySelection<in T, TResult>(T value, out TResult result)
    where T : allows ref struct
    where TResult : allows ref struct;

public static class Combinators
{
    extension<T>(TextParser<T> parser)
        where T : allows ref struct
    {
        public TextParser<TOther> Select<TOther>(Func<T, TOther> selector)
            where TOther : allows ref struct
        {
            return input =>
            {
                var result = parser(input);
                return result.HasValue
                    ? ParseResult.Success(selector(result.Value), result.Input, result.Remainder)
                    : ParseResult.CastEmpty<T, TOther>(result);
            };
        }

        public TextParser<TOther> TrySelect<TOther>(
            TrySelection<T, TOther> selector,
            string message = "condition failed"
        )
            where TOther : allows ref struct
        {
            return input =>
            {
                var result = parser(input);
                if (!result.HasValue)
                    return ParseResult.CastEmpty<T, TOther>(result);

                return selector(result.Value, out var output)
                    ? ParseResult.Success(output, result.Input, result.Remainder)
                    : ParseResult.Empty<TOther>(input, message);
            };
        }

        public TextParser<TOther> SelectMany<TOther>(Func<T, TextParser<TOther>> selector)
            where TOther : allows ref struct
        {
            return input =>
            {
                var result = parser(input);
                if (!result.HasValue)
                    return ParseResult.CastEmpty<T, TOther>(result);

                var nextResult = selector(result.Value)(result.Remainder);
                return nextResult.HasValue
                    ? ParseResult.Success(nextResult.Value, result.Input, nextResult.Remainder)
                    : nextResult;
            };
        }

        public TextParser<TProjection> SelectMany<TOther, TProjection>(
            Func<T, TextParser<TOther>> selector,
            Func<T, TOther, TProjection> projection
        )
            where TOther : allows ref struct
            where TProjection : allows ref struct
        {
            return input =>
            {
                var result = parser(input);
                if (!result.HasValue)
                    return ParseResult.CastEmpty<T, TProjection>(result);

                var nextResult = selector(result.Value)(result.Remainder);
                return nextResult.HasValue
                    ? ParseResult.Success(
                        projection(result.Value, nextResult.Value),
                        result.Input,
                        nextResult.Remainder
                    )
                    : ParseResult.CastEmpty<TOther, TProjection>(nextResult);
            };
        }

        public TextParser<TOther> SelectMany<TOther>(Func<T, ParseResult<TOther>> selector)
        {
            return input =>
            {
                var result = parser(input);
                return result.HasValue ? selector(result.Value) : ParseResult.CastEmpty<T, TOther>(result);
            };
        }

        public TextParser<TProjection> SelectMany<TOther, TProjection>(
            Func<T, ParseResult<TOther>> selector,
            Func<ParseResult<T>, ParseResult<TOther>, ParseResult<TProjection>> projection
        )
        {
            return input =>
            {
                var result = parser(input);
                if (!result.HasValue)
                    return ParseResult.CastEmpty<T, TProjection>(result);

                var subResult = selector(result.Value);
                return projection(result, subResult);
            };
        }

        public TextParser<T> Where(Func<T, bool> predicate, string message = "unsatisfied condition")
        {
            return input =>
            {
                var result = parser(input);
                if (!result.HasValue)
                    return result;

                return predicate(result.Value) ? result : ParseResult.Empty<T>(result.Input, result.Remainder, message);
            };
        }

        public TextParser<TValue> Value<TValue>(TValue value)
        {
            return parser.Select(_ => value);
        }

        public TextParser<T?> OrElseDefault()
        {
            return input =>
            {
                var result = parser(input);
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
                return result.HasValue ? result : ParseResult.Success(default(T), input, input);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
            };
        }

        public TextParser<T> OrElse(Func<T> alternative)
        {
            return input =>
            {
                var result = parser(input);
                return result.HasValue ? result : ParseResult.Success(alternative(), input, input);
            };
        }
    }

    extension<T>(TextParser<T> parser)
    {
        public TextParser<T> OrElse(T alternative)
        {
            return input =>
            {
                var result = parser(input);
                return result.HasValue ? result : ParseResult.Success(alternative, input, input);
            };
        }
    }

    extension<T>(TextParser<T?> parser)
        where T : class
    {
        public TextParser<T> NotNull()
        {
            return input =>
            {
                var result = parser(input);
                if (!result.HasValue)
                    return result!;

                return result.Value is not null
                    ? result!
                    : ParseResult.Empty<T>(result.Input, result.Remainder, "null value");
            };
        }
    }

    extension<T>(TextParser<T?> parser)
        where T : struct
    {
        public TextParser<T> NotNull()
        {
            return input =>
            {
                var result = parser(input);
                if (!result.HasValue)
                    return ParseResult.CastEmpty<T?, T>(result);

                return result.Value is not null
                    ? ParseResult.Success(result.Value.Value, input, input)
                    : ParseResult.Empty<T>(result.Input, result.Remainder, "null value");
            };
        }
    }

    extension<T>(TextParser<T> parser)
        where T : allows ref struct
    {
        public TextParser<T> Or(TextParser<T> alternative)
        {
            return input =>
            {
                var result1 = parser(input);
                if (result1.HasValue)
                    return result1;

                var result2 = alternative(input);
                return result2.HasValue ? result2 : ParseResult.CombineEmpty(result1, result2);
            };
        }

        public TextParser<T> Or(TextParser<T> alternative1, TextParser<T> alternative2)
        {
            return input =>
            {
                var result1 = parser(input);
                if (result1.HasValue)
                    return result1;

                var result2 = alternative1(input);
                if (result2.HasValue)
                    return result2;

                var result3 = alternative2(input);
                return result3.HasValue ? result3 : ParseResult.CombineEmpty(result1, result2, result3);
            };
        }

        public TextParser<T> Or(TextParser<T> alternative1, TextParser<T> alternative2, TextParser<T> alternative3)
        {
            return input =>
            {
                var result1 = parser(input);
                if (result1.HasValue)
                    return result1;

                var result2 = alternative1(input);
                if (result2.HasValue)
                    return result2;

                var result3 = alternative2(input);
                if (result3.HasValue)
                    return result3;

                var result4 = alternative3(input);
                return result4.HasValue ? result4 : ParseResult.CombineEmpty(result1, result2, result3, result4);
            };
        }

        public TextParser<T> Or(
            TextParser<T> alternative1,
            TextParser<T> alternative2,
            TextParser<T> alternative3,
            TextParser<T> alternative4
        )
        {
            return input =>
            {
                var result1 = parser(input);
                if (result1.HasValue)
                    return result1;

                var result2 = alternative1(input);
                if (result2.HasValue)
                    return result2;

                var result3 = alternative2(input);
                if (result3.HasValue)
                    return result3;

                var result4 = alternative3(input);
                if (result4.HasValue)
                    return result4;

                var result5 = alternative4(input);
                return result5.HasValue
                    ? result5
                    : ParseResult.CombineEmpty(result1, result2, result3, result4, result5);
            };
        }

        public TextParser<T> Or(
            TextParser<T> alternative1,
            TextParser<T> alternative2,
            TextParser<T> alternative3,
            TextParser<T> alternative4,
            TextParser<T> alternative5
        )
        {
            return input =>
            {
                var result1 = parser(input);
                if (result1.HasValue)
                    return result1;

                var result2 = alternative1(input);
                if (result2.HasValue)
                    return result2;

                var result3 = alternative2(input);
                if (result3.HasValue)
                    return result3;

                var result4 = alternative3(input);
                if (result4.HasValue)
                    return result4;

                var result5 = alternative4(input);
                if (result5.HasValue)
                    return result5;

                var result6 = alternative5(input);
                return result6.HasValue
                    ? result6
                    : ParseResult.CombineEmpty(result1, result2, result3, result4, result5, result6);
            };
        }

        public TextParser<T> Or(
            TextParser<T> alternative1,
            TextParser<T> alternative2,
            TextParser<T> alternative3,
            TextParser<T> alternative4,
            TextParser<T> alternative5,
            TextParser<T> alternative6
        )
        {
            return input =>
            {
                var result1 = parser(input);
                if (result1.HasValue)
                    return result1;

                var result2 = alternative1(input);
                if (result2.HasValue)
                    return result2;

                var result3 = alternative2(input);
                if (result3.HasValue)
                    return result3;

                var result4 = alternative3(input);
                if (result4.HasValue)
                    return result4;

                var result5 = alternative4(input);
                if (result5.HasValue)
                    return result5;

                var result6 = alternative5(input);
                if (result6.HasValue)
                    return result6;

                var result7 = alternative6(input);
                return result7.HasValue
                    ? result7
                    : ParseResult.CombineEmpty(result1, result2, result3, result4, result5, result6, result7);
            };
        }

        public TextParser<TProjection> Then<TSecond, TProjection>(
            TextParser<TSecond> next,
            Func<T, TSecond, TProjection> projection
        )
            where TSecond : allows ref struct
            where TProjection : allows ref struct
        {
            return input =>
            {
                var result = parser(input);
                if (!result.HasValue)
                    return ParseResult.CastEmpty<T, TProjection>(result);

                var nextResult = next(result.Remainder);
                return nextResult.HasValue
                    ? ParseResult.Success(
                        projection(result.Value, nextResult.Value),
                        result.Input,
                        nextResult.Remainder
                    )
                    : ParseResult.CastEmpty<TSecond, TProjection>(nextResult);
            };
        }

        public TextParser<TOther> IgnoreThen<TOther>(TextParser<TOther> next)
            where TOther : allows ref struct
        {
            return input =>
            {
                var result = parser(input);
                if (!result.HasValue)
                    return ParseResult.CastEmpty<T, TOther>(result);

                var nextResult = next(result.Remainder);
                return nextResult.HasValue
                    ? ParseResult.Success(nextResult.Value, result.Input, nextResult.Remainder)
                    : ParseResult.Empty<TOther>(nextResult.Remainder);
            };
        }

        public TextParser<T> FollowedBy<TIgnored>(TextParser<TIgnored> ignored)
            where TIgnored : allows ref struct
        {
            return parser.Then(ignored, (t, _) => t);
        }

        private TextParser<TResult> RepeatCore<TDelimiter, TBuilder, TResult>(
            int minimum,
            int? maximum,
            TextParser<TDelimiter>? delimiter,
            Func<TBuilder> createBuilder,
            Func<TBuilder, T, TBuilder> addItem,
            Func<TBuilder, TResult> finish,
            Action<TBuilder>? finalizeBuilder
        )
            where TBuilder : allows ref struct
            where TDelimiter : allows ref struct
            where TResult : allows ref struct
        {
            return input =>
            {
                TBuilder builder = default!;
                var createdBuilder = false;
                try
                {
                    var encountered = 0;
                    var nextResult = parser(input);
                    if (!nextResult.HasValue)
                    {
                        if (minimum > 0)
                            return ParseResult.CastEmpty<T, TResult>(nextResult);

                        builder = createBuilder();
                        return ParseResult.Success(finish(builder), input, input);
                    }

                    TextSegment remaining;
                    do
                    {
                        if (!createdBuilder)
                        {
                            builder = createBuilder();
                            createdBuilder = true;
                        }

                        builder = addItem(builder, nextResult.Value);
                        encountered++;
                        remaining = nextResult.Remainder;
                        if (encountered >= maximum)
                            break;

                        if (delimiter is not null)
                        {
                            var delimiterResult = delimiter(remaining);
                            if (!delimiterResult.HasValue)
                                break;

                            remaining = delimiterResult.Remainder;
                        }

                        nextResult = parser(remaining);

                        if (delimiter is not null && !nextResult.HasValue)
                            break;

                        remaining = nextResult.Remainder;
                    } while (nextResult.HasValue);

                    return encountered >= minimum
                        ? ParseResult.Success(finish(builder), input, remaining)
                        : ParseResult.CastEmpty<T, TResult>(nextResult);
                }
                finally
                {
                    if (createdBuilder && finalizeBuilder is not null)
                    {
                        finalizeBuilder(builder);
                    }
                }
            };
        }

        public TextParser<TResult> Many<TBuilder, TResult>(
            Func<TBuilder> identity,
            Func<TBuilder, T, TBuilder> accumulator,
            Func<TBuilder, TResult> finish,
            Action<TBuilder>? finalizeBuilder = null
        )
            where TBuilder : allows ref struct
            where TResult : allows ref struct
        {
            return parser.RepeatCore(0, null, (TextParser<Unit>?)null, identity, accumulator, finish, finalizeBuilder);
        }

        public TextParser<TResult> Many<TResult>(Func<TResult> identity, Func<TResult, T, TResult> accumulator)
            where TResult : allows ref struct
        {
            return parser.RepeatCore(0, null, (TextParser<Unit>?)null, identity, accumulator, x => x, null);
        }

        public TextParser<TResult> ManyDelimitedBy<TBuilder, TResult, TDelimiter>(
            TextParser<TDelimiter> delimiter,
            Func<TBuilder> identity,
            Func<TBuilder, T, TBuilder> accumulator,
            Func<TBuilder, TResult> finish,
            Action<TBuilder>? finalizeBuilder = null
        )
            where TDelimiter : allows ref struct
            where TBuilder : allows ref struct
            where TResult : allows ref struct
        {
            return parser.RepeatCore(0, null, delimiter, identity, accumulator, finish, finalizeBuilder);
        }

        public TextParser<TResult> ManyDelimitedBy<TResult, TDelimiter>(
            TextParser<TDelimiter> delimiter,
            Func<TResult> identity,
            Func<TResult, T, TResult> accumulator
        )
            where TDelimiter : allows ref struct
            where TResult : allows ref struct
        {
            return parser.ManyDelimitedBy(delimiter, identity, accumulator, x => x);
        }

        public TextParser<Unit> IgnoreMany()
        {
            return parser.Many(() => Unit.Value, (_, _) => Unit.Value);
        }

        public TextParser<TResult> AtLeast<TBuilder, TResult>(
            int count,
            Func<TBuilder> identity,
            Func<TBuilder, T, TBuilder> accumulator,
            Func<TBuilder, TResult> finish,
            Action<TBuilder>? finalizeBuilder = null
        )
            where TBuilder : allows ref struct
            where TResult : allows ref struct
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

            return parser.RepeatCore(
                count,
                null,
                (TextParser<Unit>?)null,
                identity,
                accumulator,
                finish,
                finalizeBuilder
            );
        }

        public TextParser<TResult> AtLeast<TResult>(
            int count,
            Func<TResult> identity,
            Func<TResult, T, TResult> accumulator
        )
            where TResult : allows ref struct
        {
            return parser.AtLeast(count, identity, accumulator, x => x);
        }

        public TextParser<TResult> AtLeastDelimitedBy<TBuilder, TResult, TDelimiter>(
            int count,
            TextParser<TDelimiter> delimiter,
            Func<TBuilder> identity,
            Func<TBuilder, T, TBuilder> accumulator,
            Func<TBuilder, TResult> finish,
            Action<TBuilder>? finalizeBuilder = null
        )
            where TResult : allows ref struct
            where TBuilder : allows ref struct
            where TDelimiter : allows ref struct
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
            return parser.RepeatCore(count, null, delimiter, identity, accumulator, finish, finalizeBuilder);
        }

        public TextParser<TResult> AtLeastDelimitedBy<TDelimiter, TResult>(
            int count,
            TextParser<TDelimiter> delimiter,
            Func<TResult> identity,
            Func<TResult, T, TResult> accumulator
        )
            where TResult : allows ref struct
            where TDelimiter : allows ref struct
        {
            return parser.AtLeastDelimitedBy(count, delimiter, identity, accumulator, x => x);
        }

        public TextParser<TResult> AtLeastOnce<TBuilder, TResult>(
            Func<TBuilder> identity,
            Func<TBuilder, T, TBuilder> accumulator,
            Func<TBuilder, TResult> finish,
            Action<TBuilder>? finalizeBuilder = null
        )
        {
            return parser.AtLeast(1, identity, accumulator, finish);
        }

        public TextParser<TResult> AtLeastOnce<TResult>(Func<TResult> identity, Func<TResult, T, TResult> accumulator)
        {
            return parser.AtLeast(1, identity, accumulator);
        }

        public TextParser<Unit> IgnoreAtLeastOnce()
        {
            return parser.AtLeastOnce(() => Unit.Value, (_, _) => Unit.Value);
        }

        public TextParser<TResult> AtLeastOnceDelimitedBy<TDelimiter, TBuilder, TResult>(
            TextParser<TDelimiter> delimiter,
            Func<TBuilder> identity,
            Func<TBuilder, T, TBuilder> accumulator,
            Func<TBuilder, TResult> finish,
            Action<TBuilder>? finalizeBuilder = null
        )
            where TDelimiter : allows ref struct
            where TBuilder : allows ref struct
            where TResult : allows ref struct
        {
            return parser.AtLeastDelimitedBy(1, delimiter, identity, accumulator, finish, finalizeBuilder);
        }

        public TextParser<TResult> AtLeastOnceDelimitedBy<TDelimiter, TResult>(
            TextParser<TDelimiter> delimiter,
            Func<TResult> identity,
            Func<TResult, T, TResult> accumulator
        )
            where TDelimiter : allows ref struct
            where TResult : allows ref struct
        {
            return parser.AtLeastDelimitedBy(1, delimiter, identity, accumulator);
        }

        public TextParser<TResult> AtMost<TBuilder, TResult>(
            int count,
            Func<TBuilder> identity,
            Func<TBuilder, T, TBuilder> accumulator,
            Func<TBuilder, TResult> finish,
            Action<TBuilder>? finalizeBuilder = null
        )
            where TBuilder : allows ref struct
            where TResult : allows ref struct
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
            return parser.RepeatCore(0, count, (TextParser<Unit>?)null, identity, accumulator, finish, finalizeBuilder);
        }

        public TextParser<TResult> AtMost<TResult>(
            int count,
            Func<TResult> identity,
            Func<TResult, T, TResult> accumulator
        )
            where TResult : allows ref struct
        {
            return parser.AtMost(count, identity, accumulator, x => x);
        }

        public TextParser<TResult> AtMostDelimitedBy<TDelimiter, TBuilder, TResult>(
            int count,
            TextParser<TDelimiter> delimiter,
            Func<TBuilder> identity,
            Func<TBuilder, T, TBuilder> accumulator,
            Func<TBuilder, TResult> finish,
            Action<TBuilder>? finalizeBuilder = null
        )
            where TResult : allows ref struct
            where TBuilder : allows ref struct
            where TDelimiter : allows ref struct
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
            return parser.RepeatCore(0, count, delimiter, identity, accumulator, finish, finalizeBuilder);
        }

        public TextParser<TResult> AtMostDelimitedBy<TDelimiter, TResult>(
            int count,
            TextParser<TDelimiter> delimiter,
            Func<TResult> identity,
            Func<TResult, T, TResult> accumulator
        )
            where TResult : allows ref struct
            where TDelimiter : allows ref struct
        {
            return parser.AtMostDelimitedBy(count, delimiter, identity, accumulator, x => x);
        }

        public TextParser<TResult> Repeat<TBuilder, TResult>(
            int count,
            Func<TBuilder> identity,
            Func<TBuilder, T, TBuilder> accumulator,
            Func<TBuilder, TResult> finish,
            Action<TBuilder>? finalizeBuilder = null
        )
            where TBuilder : allows ref struct
            where TResult : allows ref struct
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
            return parser.RepeatCore(
                count,
                count,
                (TextParser<Unit>?)null,
                identity,
                accumulator,
                finish,
                finalizeBuilder
            );
        }

        public TextParser<TResult> Repeat<TResult>(
            int count,
            Func<TResult> identity,
            Func<TResult, T, TResult> accumulator
        )
            where TResult : allows ref struct
        {
            return parser.Repeat(count, identity, accumulator, x => x);
        }

        public TextParser<TResult> RepeatDelimitedBy<TDelimiter, TBuilder, TResult>(
            int count,
            TextParser<TDelimiter> delimiter,
            Func<TBuilder> identity,
            Func<TBuilder, T, TBuilder> accumulator,
            Func<TBuilder, TResult> finish,
            Action<TBuilder>? finalizeBuilder = null
        )
            where TDelimiter : allows ref struct
            where TBuilder : allows ref struct
            where TResult : allows ref struct
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
            return parser.RepeatCore(count, count, delimiter, identity, accumulator, finish, finalizeBuilder);
        }

        public TextParser<TResult> RepeatDelimitedBy<TDelimiter, TResult>(
            int count,
            TextParser<TDelimiter> delimiter,
            Func<TResult> identity,
            Func<TResult, T, TResult> accumulator
        )
            where TDelimiter : allows ref struct
            where TResult : allows ref struct
        {
            return parser.RepeatDelimitedBy(count, delimiter, identity, accumulator, x => x);
        }

        public TextParser<TResult> RepeatedRange<TBuilder, TResult>(
            int minimum,
            int maximum,
            Func<TBuilder> identity,
            Func<TBuilder, T, TBuilder> accumulator,
            Func<TBuilder, TResult> finish,
            Action<TBuilder>? finalizeBuilder = null
        )
            where TBuilder : allows ref struct
            where TResult : allows ref struct
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(minimum);
            return maximum >= minimum
                ? parser.RepeatCore(
                    minimum,
                    maximum,
                    (TextParser<Unit>?)null,
                    identity,
                    accumulator,
                    finish,
                    finalizeBuilder
                )
                : throw new ArgumentOutOfRangeException(nameof(maximum), "Maximum value must be less than the minimum");
        }

        public TextParser<TResult> RepeatedRange<TResult>(
            int minimum,
            int maximum,
            Func<TResult> identity,
            Func<TResult, T, TResult> accumulator
        )
            where TResult : allows ref struct
        {
            return parser.RepeatedRange(minimum, maximum, identity, accumulator, x => x);
        }

        public TextParser<TResult> RepeatedRangeDelimitedBy<TDelimiter, TBuilder, TResult>(
            int minimum,
            int maximum,
            TextParser<TDelimiter> delimiter,
            Func<TBuilder> identity,
            Func<TBuilder, T, TBuilder> accumulator,
            Func<TBuilder, TResult> finish,
            Action<TBuilder>? finalizeBuilder = null
        )
            where TDelimiter : allows ref struct
            where TBuilder : allows ref struct
            where TResult : allows ref struct
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(minimum);
            return maximum >= minimum
                ? parser.RepeatCore(minimum, maximum, delimiter, identity, accumulator, finish, finalizeBuilder)
                : throw new ArgumentOutOfRangeException(nameof(maximum), "Maximum value must be less than the minimum");
        }

        public TextParser<TResult> RepeatedRangeDelimitedBy<TDelimiter, TResult>(
            int minimum,
            int maximum,
            TextParser<TDelimiter> delimiter,
            Func<TResult> identity,
            Func<TResult, T, TResult> accumulator
        )
            where TDelimiter : allows ref struct
            where TResult : allows ref struct
        {
            return parser.RepeatedRangeDelimitedBy(minimum, maximum, delimiter, identity, accumulator, x => x);
        }
    }
}
