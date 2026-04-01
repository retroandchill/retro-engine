// // @file Combinators.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ZParse;

public delegate void OnParseSuccess<TContext, in T>(scoped ref TContext context, T value)
    where TContext : allows ref struct
    where T : allows ref struct;

public static class Combinators
{
    extension<T>(ParseResult<T> result)
        where T : allows ref struct
    {
        public ParseResult<TResult> Select<TResult>(Func<T, TResult> selector)
            where TResult : allows ref struct
        {
            return result.HasValue
                ? ParseResult.Success(selector(result.Value), result.Input, result.Remainder)
                : ParseResult.CastEmpty<T, TResult>(result);
        }

        public ParseResult<TResult> Select<TContext, TResult>(TContext context, Func<TContext, T, TResult> selector)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return result.HasValue
                ? ParseResult.Success(selector(context, result.Value), result.Input, result.Remainder)
                : ParseResult.CastEmpty<T, TResult>(result);
        }

        public ParseResult<T> Where(Func<T, bool> predicate)
        {
            if (!result.HasValue || predicate(result.Value))
                return result;

            return ParseResult.Empty<T>(result.Input, result.Remainder);
        }

        public ParseResult<T> Where<TContext>(TContext context, Func<TContext, T, bool> predicate)
        {
            if (!result.HasValue || predicate(context, result.Value))
                return result;

            return ParseResult.Empty<T>(result.Input, result.Remainder);
        }

        public ParseResult<T> OrElse(Func<TextSegment, ParseResult<T>> fallback)
        {
            return result.HasValue ? result : fallback(result.Input);
        }

        public ParseResult<T> OrElse<TContext>(TContext context, Func<TContext, TextSegment, ParseResult<T>> fallback)
            where TContext : allows ref struct
        {
            return result.HasValue ? result : fallback(context, result.Input);
        }

        public ParseResult<T> AndThen(Action<T> next)
        {
            if (result.HasValue)
            {
                next(result.Value);
            }

            return result;
        }

        public ParseResult<T> AndThen<TContext>(TContext context, Action<TContext, T> next)
            where TContext : allows ref struct
        {
            if (result.HasValue)
            {
                next(context, result.Value);
            }

            return result;
        }

        public ParseResult<T> AndThen<TContext>(scoped ref TContext context, OnParseSuccess<TContext, T> next)
            where TContext : allows ref struct
        {
            if (result.HasValue)
            {
                next(ref context, result.Value);
            }

            return result;
        }
    }

    extension(TextSegment input)
    {
        public ParseResult<TResult> ParseSequence<T1, T2, TResult>(
            Func<TextSegment, ParseResult<T1>> first,
            Func<TextSegment, ParseResult<T2>> second,
            Func<T1, T2, TResult> selector
        )
            where T1 : allows ref struct
            where T2 : allows ref struct
            where TResult : allows ref struct
        {
            var firstResult = first(input);
            if (!firstResult.HasValue)
                return ParseResult.CastEmpty<T1, TResult>(firstResult);

            var secondResult = second(firstResult.Remainder);
            if (!secondResult.HasValue)
                return ParseResult.CastEmpty<T2, TResult>(secondResult);

            return ParseResult.Success(
                selector(firstResult.Value, secondResult.Value),
                firstResult.Input,
                secondResult.Remainder
            );
        }

        public ParseResult<TResult> ParseSequence<T1, T2, T3, TResult>(
            Func<TextSegment, ParseResult<T1>> first,
            Func<TextSegment, ParseResult<T2>> second,
            Func<TextSegment, ParseResult<T3>> third,
            Func<T1, T2, T3, TResult> selector
        )
            where T1 : allows ref struct
            where T2 : allows ref struct
            where T3 : allows ref struct
            where TResult : allows ref struct
        {
            var firstResult = first(input);
            if (!firstResult.HasValue)
                return ParseResult.CastEmpty<T1, TResult>(firstResult);

            var secondResult = second(firstResult.Remainder);
            if (!secondResult.HasValue)
                return ParseResult.CastEmpty<T2, TResult>(secondResult);

            var thirdResult = third(secondResult.Remainder);
            if (!thirdResult.HasValue)
                return ParseResult.CastEmpty<T3, TResult>(thirdResult);

            return ParseResult.Success(
                selector(firstResult.Value, secondResult.Value, thirdResult.Value),
                firstResult.Input,
                secondResult.Remainder
            );
        }

        public ParseResult<TResult> ParseSequence<T1, T2, T3, T4, TResult>(
            Func<TextSegment, ParseResult<T1>> first,
            Func<TextSegment, ParseResult<T2>> second,
            Func<TextSegment, ParseResult<T3>> third,
            Func<TextSegment, ParseResult<T4>> fourth,
            Func<T1, T2, T3, T4, TResult> selector
        )
            where T1 : allows ref struct
            where T2 : allows ref struct
            where T3 : allows ref struct
            where T4 : allows ref struct
            where TResult : allows ref struct
        {
            var firstResult = first(input);
            if (!firstResult.HasValue)
                return ParseResult.CastEmpty<T1, TResult>(firstResult);

            var secondResult = second(firstResult.Remainder);
            if (!secondResult.HasValue)
                return ParseResult.CastEmpty<T2, TResult>(secondResult);

            var thirdResult = third(secondResult.Remainder);
            if (!thirdResult.HasValue)
                return ParseResult.CastEmpty<T3, TResult>(thirdResult);

            var fourthResult = fourth(thirdResult.Remainder);
            if (!fourthResult.HasValue)
                return ParseResult.CastEmpty<T4, TResult>(fourthResult);

            return ParseResult.Success(
                selector(firstResult.Value, secondResult.Value, thirdResult.Value, fourthResult.Value),
                firstResult.Input,
                secondResult.Remainder
            );
        }

        public ParseResult<TResult> ParseSequence<T1, T2, T3, T4, T5, TResult>(
            Func<TextSegment, ParseResult<T1>> first,
            Func<TextSegment, ParseResult<T2>> second,
            Func<TextSegment, ParseResult<T3>> third,
            Func<TextSegment, ParseResult<T4>> fourth,
            Func<TextSegment, ParseResult<T5>> fifth,
            Func<T1, T2, T3, T4, T5, TResult> selector
        )
            where T1 : allows ref struct
            where T2 : allows ref struct
            where T3 : allows ref struct
            where T4 : allows ref struct
            where T5 : allows ref struct
            where TResult : allows ref struct
        {
            var firstResult = first(input);
            if (!firstResult.HasValue)
                return ParseResult.CastEmpty<T1, TResult>(firstResult);

            var secondResult = second(firstResult.Remainder);
            if (!secondResult.HasValue)
                return ParseResult.CastEmpty<T2, TResult>(secondResult);

            var thirdResult = third(secondResult.Remainder);
            if (!thirdResult.HasValue)
                return ParseResult.CastEmpty<T3, TResult>(thirdResult);

            var fourthResult = fourth(thirdResult.Remainder);
            if (!fourthResult.HasValue)
                return ParseResult.CastEmpty<T4, TResult>(fourthResult);

            var fifthResult = fifth(fourthResult.Remainder);
            if (!fifthResult.HasValue)
                return ParseResult.CastEmpty<T5, TResult>(fifthResult);

            return ParseResult.Success(
                selector(
                    firstResult.Value,
                    secondResult.Value,
                    thirdResult.Value,
                    fourthResult.Value,
                    fifthResult.Value
                ),
                firstResult.Input,
                secondResult.Remainder
            );
        }

        public ParseResult<TResult> ParseSequence<T1, T2, T3, T4, T5, T6, TResult>(
            Func<TextSegment, ParseResult<T1>> first,
            Func<TextSegment, ParseResult<T2>> second,
            Func<TextSegment, ParseResult<T3>> third,
            Func<TextSegment, ParseResult<T4>> fourth,
            Func<TextSegment, ParseResult<T5>> fifth,
            Func<TextSegment, ParseResult<T6>> sixth,
            Func<T1, T2, T3, T4, T5, T6, TResult> selector
        )
            where T1 : allows ref struct
            where T2 : allows ref struct
            where T3 : allows ref struct
            where T4 : allows ref struct
            where T5 : allows ref struct
            where T6 : allows ref struct
            where TResult : allows ref struct
        {
            var firstResult = first(input);
            if (!firstResult.HasValue)
                return ParseResult.CastEmpty<T1, TResult>(firstResult);

            var secondResult = second(firstResult.Remainder);
            if (!secondResult.HasValue)
                return ParseResult.CastEmpty<T2, TResult>(secondResult);

            var thirdResult = third(secondResult.Remainder);
            if (!thirdResult.HasValue)
                return ParseResult.CastEmpty<T3, TResult>(thirdResult);

            var fourthResult = fourth(thirdResult.Remainder);
            if (!fourthResult.HasValue)
                return ParseResult.CastEmpty<T4, TResult>(fourthResult);

            var fifthResult = fifth(fourthResult.Remainder);
            if (!fifthResult.HasValue)
                return ParseResult.CastEmpty<T5, TResult>(fifthResult);

            var sixthResult = sixth(fifthResult.Remainder);
            if (!sixthResult.HasValue)
                return ParseResult.CastEmpty<T6, TResult>(sixthResult);

            return ParseResult.Success(
                selector(
                    firstResult.Value,
                    secondResult.Value,
                    thirdResult.Value,
                    fourthResult.Value,
                    fifthResult.Value,
                    sixthResult.Value
                ),
                firstResult.Input,
                secondResult.Remainder
            );
        }

        public ParseResult<TResult> ParseSequence<TContext, T1, T2, TResult>(
            TContext context,
            Func<TContext, TextSegment, ParseResult<T1>> first,
            Func<TContext, TextSegment, ParseResult<T2>> second,
            Func<TContext, T1, T2, TResult> selector
        )
            where TContext : allows ref struct
            where T1 : allows ref struct
            where T2 : allows ref struct
            where TResult : allows ref struct
        {
            var firstResult = first(context, input);
            if (!firstResult.HasValue)
                return ParseResult.CastEmpty<T1, TResult>(firstResult);

            var secondResult = second(context, firstResult.Remainder);
            if (!secondResult.HasValue)
                return ParseResult.CastEmpty<T2, TResult>(secondResult);

            return ParseResult.Success(
                selector(context, firstResult.Value, secondResult.Value),
                firstResult.Input,
                secondResult.Remainder
            );
        }

        public ParseResult<TResult> ParseSequence<TContext, T1, T2, T3, TResult>(
            TContext context,
            Func<TContext, TextSegment, ParseResult<T1>> first,
            Func<TContext, TextSegment, ParseResult<T2>> second,
            Func<TContext, TextSegment, ParseResult<T3>> third,
            Func<TContext, T1, T2, T3, TResult> selector
        )
            where TContext : allows ref struct
            where T1 : allows ref struct
            where T2 : allows ref struct
            where T3 : allows ref struct
            where TResult : allows ref struct
        {
            var firstResult = first(context, input);
            if (!firstResult.HasValue)
                return ParseResult.CastEmpty<T1, TResult>(firstResult);

            var secondResult = second(context, firstResult.Remainder);
            if (!secondResult.HasValue)
                return ParseResult.CastEmpty<T2, TResult>(secondResult);

            var thirdResult = third(context, secondResult.Remainder);
            if (!thirdResult.HasValue)
                return ParseResult.CastEmpty<T3, TResult>(thirdResult);

            return ParseResult.Success(
                selector(context, firstResult.Value, secondResult.Value, thirdResult.Value),
                firstResult.Input,
                secondResult.Remainder
            );
        }

        public ParseResult<TResult> ParseSequence<TContext, T1, T2, T3, T4, TResult>(
            TContext context,
            Func<TContext, TextSegment, ParseResult<T1>> first,
            Func<TContext, TextSegment, ParseResult<T2>> second,
            Func<TContext, TextSegment, ParseResult<T3>> third,
            Func<TContext, TextSegment, ParseResult<T4>> fourth,
            Func<TContext, T1, T2, T3, T4, TResult> selector
        )
            where TContext : allows ref struct
            where T1 : allows ref struct
            where T2 : allows ref struct
            where T3 : allows ref struct
            where T4 : allows ref struct
            where TResult : allows ref struct
        {
            var firstResult = first(context, input);
            if (!firstResult.HasValue)
                return ParseResult.CastEmpty<T1, TResult>(firstResult);

            var secondResult = second(context, firstResult.Remainder);
            if (!secondResult.HasValue)
                return ParseResult.CastEmpty<T2, TResult>(secondResult);

            var thirdResult = third(context, secondResult.Remainder);
            if (!thirdResult.HasValue)
                return ParseResult.CastEmpty<T3, TResult>(thirdResult);

            var fourthResult = fourth(context, thirdResult.Remainder);
            if (!fourthResult.HasValue)
                return ParseResult.CastEmpty<T4, TResult>(fourthResult);

            return ParseResult.Success(
                selector(context, firstResult.Value, secondResult.Value, thirdResult.Value, fourthResult.Value),
                firstResult.Input,
                secondResult.Remainder
            );
        }

        public ParseResult<TResult> ParseSequence<TContext, T1, T2, T3, T4, T5, TResult>(
            TContext context,
            Func<TContext, TextSegment, ParseResult<T1>> first,
            Func<TContext, TextSegment, ParseResult<T2>> second,
            Func<TContext, TextSegment, ParseResult<T3>> third,
            Func<TContext, TextSegment, ParseResult<T4>> fourth,
            Func<TContext, TextSegment, ParseResult<T5>> fifth,
            Func<TContext, T1, T2, T3, T4, T5, TResult> selector
        )
            where TContext : allows ref struct
            where T1 : allows ref struct
            where T2 : allows ref struct
            where T3 : allows ref struct
            where T4 : allows ref struct
            where T5 : allows ref struct
            where TResult : allows ref struct
        {
            var firstResult = first(context, input);
            if (!firstResult.HasValue)
                return ParseResult.CastEmpty<T1, TResult>(firstResult);

            var secondResult = second(context, firstResult.Remainder);
            if (!secondResult.HasValue)
                return ParseResult.CastEmpty<T2, TResult>(secondResult);

            var thirdResult = third(context, secondResult.Remainder);
            if (!thirdResult.HasValue)
                return ParseResult.CastEmpty<T3, TResult>(thirdResult);

            var fourthResult = fourth(context, thirdResult.Remainder);
            if (!fourthResult.HasValue)
                return ParseResult.CastEmpty<T4, TResult>(fourthResult);

            var fifthResult = fifth(context, fourthResult.Remainder);
            if (!fifthResult.HasValue)
                return ParseResult.CastEmpty<T5, TResult>(fifthResult);

            return ParseResult.Success(
                selector(
                    context,
                    firstResult.Value,
                    secondResult.Value,
                    thirdResult.Value,
                    fourthResult.Value,
                    fifthResult.Value
                ),
                firstResult.Input,
                secondResult.Remainder
            );
        }

        public ParseResult<TResult> ParseSequence<TContext, T1, T2, T3, T4, T5, T6, TResult>(
            TContext context,
            Func<TContext, TextSegment, ParseResult<T1>> first,
            Func<TContext, TextSegment, ParseResult<T2>> second,
            Func<TContext, TextSegment, ParseResult<T3>> third,
            Func<TContext, TextSegment, ParseResult<T4>> fourth,
            Func<TContext, TextSegment, ParseResult<T5>> fifth,
            Func<TContext, TextSegment, ParseResult<T6>> sixth,
            Func<TContext, T1, T2, T3, T4, T5, T6, TResult> selector
        )
            where TContext : allows ref struct
            where T1 : allows ref struct
            where T2 : allows ref struct
            where T3 : allows ref struct
            where T4 : allows ref struct
            where T5 : allows ref struct
            where T6 : allows ref struct
            where TResult : allows ref struct
        {
            var firstResult = first(context, input);
            if (!firstResult.HasValue)
                return ParseResult.CastEmpty<T1, TResult>(firstResult);

            var secondResult = second(context, firstResult.Remainder);
            if (!secondResult.HasValue)
                return ParseResult.CastEmpty<T2, TResult>(secondResult);

            var thirdResult = third(context, secondResult.Remainder);
            if (!thirdResult.HasValue)
                return ParseResult.CastEmpty<T3, TResult>(thirdResult);

            var fourthResult = fourth(context, thirdResult.Remainder);
            if (!fourthResult.HasValue)
                return ParseResult.CastEmpty<T4, TResult>(fourthResult);

            var fifthResult = fifth(context, fourthResult.Remainder);
            if (!fifthResult.HasValue)
                return ParseResult.CastEmpty<T5, TResult>(fifthResult);

            var sixthResult = sixth(context, fifthResult.Remainder);
            if (!sixthResult.HasValue)
                return ParseResult.CastEmpty<T6, TResult>(sixthResult);

            return ParseResult.Success(
                selector(
                    context,
                    firstResult.Value,
                    secondResult.Value,
                    thirdResult.Value,
                    fourthResult.Value,
                    fifthResult.Value,
                    sixthResult.Value
                ),
                firstResult.Input,
                secondResult.Remainder
            );
        }
    }

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
            return input => ParseResult.Success(value, input, input);
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

        public TextParser<TIdentity> Many<TIdentity>(
            Func<TIdentity> identity,
            Func<TIdentity, T, TIdentity> accumulator
        )
            where TIdentity : allows ref struct
        {
            return input =>
            {
                var result = identity();
                ParseResult<T> next;
                for (next = parser(input); next.HasValue; next = parser(next.Remainder))
                {
                    result = accumulator(result, next.Value);
                }

                return ParseResult.Success(result, input, next.Remainder);
            };
        }

        public TextParser<TIdentity> Many<TIdentity>(TIdentity identity, Func<TIdentity, T, TIdentity> accumulator)
        {
            return parser.Many(() => identity, accumulator);
        }

        public TextParser<TIdentity> ManyDelimitedBy<TIdentity, TDelimiter>(
            TextParser<TDelimiter> delimiter,
            Func<TIdentity> identity,
            Func<TIdentity, T, TIdentity> accumulator
        )
            where TDelimiter : allows ref struct
            where TIdentity : allows ref struct
        {
            return input =>
            {
                var result = identity();
                var next = parser(input);
                while (next.HasValue)
                {
                    result = accumulator(result, next.Value);

                    var delimiterResult = delimiter(next.Remainder);
                    if (!delimiterResult.HasValue)
                        break;

                    next = parser(delimiterResult.Remainder);
                }

                return ParseResult.Success(result, input, next.Remainder);
            };
        }

        public TextParser<TIdentity> ManyDelimitedBy<TIdentity, TDelimiter>(
            TextParser<TDelimiter> delimiter,
            TIdentity identity,
            Func<TIdentity, T, TIdentity> accumulator
        )
            where TDelimiter : allows ref struct
        {
            return parser.ManyDelimitedBy(delimiter, () => identity, accumulator);
        }

        public TextParser<Unit> IgnoreMany()
        {
            return parser.Many(Unit.Value, (_, _) => Unit.Value);
        }

        public TextParser<TIdentity> AtLeast<TIdentity>(
            int count,
            Func<TIdentity> identity,
            Func<TIdentity, T, TIdentity> accumulator
        )
            where TIdentity : allows ref struct
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
            return input =>
            {
                var elementFound = false;
                var result = default(TIdentity)!;
                var next = parser(input);
                var encountered = 0;
                while (next.HasValue)
                {
                    result = accumulator(elementFound ? result : identity(), next.Value);

                    elementFound = true;
                    next = parser(next.Remainder);
                    encountered++;
                }

                return encountered >= count
                    ? ParseResult.Success(result, input, next.Remainder)
                    : ParseResult.CastEmpty<T, TIdentity>(next);
            };
        }

        public TextParser<TIdentity> AtLeast<TIdentity>(
            int count,
            TIdentity identity,
            Func<TIdentity, T, TIdentity> accumulator
        )
        {
            return parser.AtLeast(count, () => identity, accumulator);
        }

        public TextParser<T> AtLeast(int count, Func<T, T, T> accumulator)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
            return input =>
            {
                var elementFound = false;
                var result = default(T)!;
                var next = parser(input);
                var encountered = 0;
                while (next.HasValue)
                {
                    result = elementFound ? accumulator(result, next.Value) : next.Value;

                    elementFound = true;
                    next = parser(next.Remainder);
                    encountered++;
                }

                return encountered >= count ? ParseResult.Success(result, input, next.Remainder) : next;
            };
        }

        public TextParser<TIdentity> AtLeastDelimitedBy<TDelimiter, TIdentity>(
            int count,
            TextParser<TDelimiter> delimiter,
            Func<TIdentity> identity,
            Func<TIdentity, T, TIdentity> accumulator
        )
            where TIdentity : allows ref struct
            where TDelimiter : allows ref struct
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
            return input =>
            {
                var elementFound = false;
                var result = default(TIdentity)!;
                var next = parser(input);
                var encountered = 0;
                while (next.HasValue)
                {
                    result = accumulator(elementFound ? result : identity(), next.Value);

                    elementFound = true;
                    encountered++;

                    var delimiterResult = delimiter(next.Remainder);
                    if (!delimiterResult.HasValue)
                        break;

                    next = parser(delimiterResult.Remainder);
                }

                return encountered >= count
                    ? ParseResult.Success(result, input, next.Remainder)
                    : ParseResult.CastEmpty<T, TIdentity>(next);
            };
        }

        public TextParser<TIdentity> AtLeastDelimitedBy<TDelimiter, TIdentity>(
            int count,
            TextParser<TDelimiter> delimiter,
            TIdentity identity,
            Func<TIdentity, T, TIdentity> accumulator
        )
            where TDelimiter : allows ref struct
        {
            return parser.AtLeastDelimitedBy(count, delimiter, () => identity, accumulator);
        }

        public TextParser<T> AtLeastDelimitedBy<TDelimiter>(
            int count,
            TextParser<TDelimiter> delimiter,
            Func<T, T, T> accumulator
        )
            where TDelimiter : allows ref struct
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
            return input =>
            {
                var elementFound = false;
                var result = default(T)!;
                var next = parser(input);
                var encountered = 0;
                while (next.HasValue)
                {
                    result = elementFound ? accumulator(result, next.Value) : next.Value;

                    elementFound = true;
                    encountered++;

                    var delimiterResult = delimiter(next.Remainder);
                    if (!delimiterResult.HasValue)
                        break;

                    next = parser(delimiterResult.Remainder);
                }

                return encountered >= count ? ParseResult.Success(result, input, next.Remainder) : next;
            };
        }

        public TextParser<TIdentity> AtLeastOnce<TIdentity>(
            Func<TIdentity> identity,
            Func<TIdentity, T, TIdentity> accumulator
        )
        {
            return parser.AtLeast(1, identity, accumulator);
        }

        public TextParser<TIdentity> AtLeastOnce<TIdentity>(
            TIdentity identity,
            Func<TIdentity, T, TIdentity> accumulator
        )
        {
            return parser.AtLeastOnce(() => identity, accumulator);
        }

        public TextParser<T> AtLeastOnce(Func<T, T, T> accumulator)
        {
            return parser.AtLeast(1, accumulator);
        }

        public TextParser<TIdentity> AtLeastOnceDelimitedBy<TDelimiter, TIdentity>(
            TextParser<TDelimiter> delimiter,
            Func<TIdentity> identity,
            Func<TIdentity, T, TIdentity> accumulator
        )
            where TDelimiter : allows ref struct
            where TIdentity : allows ref struct
        {
            return parser.AtLeastDelimitedBy(1, delimiter, identity, accumulator);
        }

        public TextParser<TIdentity> AtLeastOnceDelimitedBy<TDelimiter, TIdentity>(
            TextParser<TDelimiter> delimiter,
            TIdentity identity,
            Func<TIdentity, T, TIdentity> accumulator
        )
            where TDelimiter : allows ref struct
        {
            return parser.AtLeastDelimitedBy(1, delimiter, identity, accumulator);
        }

        public TextParser<T> AtLeastOnceDelimitedBy<TDelimiter>(
            TextParser<TDelimiter> delimiter,
            Func<T, T, T> accumulator
        )
            where TDelimiter : allows ref struct
        {
            return parser.AtLeastDelimitedBy(1, delimiter, accumulator);
        }

        public TextParser<TIdentity> AtMost<TIdentity>(
            int count,
            Func<TIdentity> identity,
            Func<TIdentity, T, TIdentity> accumulator
        )
            where TIdentity : allows ref struct
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
            return input =>
            {
                var result = identity();
                ParseResult<T> next;
                var encountered = 0;
                for (next = parser(input); next.HasValue; next = parser(next.Remainder))
                {
                    result = accumulator(result, next.Value);
                    encountered++;
                    if (encountered >= count)
                        break;
                }

                return ParseResult.Success(result, input, next.Remainder);
            };
        }

        public TextParser<TIdentity> AtMost<TIdentity>(
            int count,
            TIdentity identity,
            Func<TIdentity, T, TIdentity> accumulator
        )
        {
            return parser.AtMost(count, () => identity, accumulator);
        }

        public TextParser<TIdentity> AtMostDelimitedBy<TDelimiter, TIdentity>(
            int count,
            TextParser<TDelimiter> delimiter,
            Func<TIdentity> identity,
            Func<TIdentity, T, TIdentity> accumulator
        )
            where TIdentity : allows ref struct
            where TDelimiter : allows ref struct
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
            return input =>
            {
                var result = identity();
                var next = parser(input);
                var encountered = 0;
                while (next.HasValue)
                {
                    result = accumulator(result, next.Value);
                    encountered++;
                    if (encountered >= count)
                        break;

                    var delimiterResult = delimiter(next.Remainder);
                    if (!delimiterResult.HasValue)
                        break;

                    next = parser(delimiterResult.Remainder);
                }

                return ParseResult.Success(result, input, next.Remainder);
            };
        }

        public TextParser<TIdentity> AtMostDelimitedBy<TDelimiter, TIdentity>(
            int count,
            TextParser<TDelimiter> delimiter,
            TIdentity identity,
            Func<TIdentity, T, TIdentity> accumulator
        )
            where TDelimiter : allows ref struct
        {
            return parser.AtMostDelimitedBy(count, delimiter, () => identity, accumulator);
        }

        public TextParser<TIdentity> Repeat<TIdentity>(
            int count,
            Func<TIdentity> identity,
            Func<TIdentity, T, TIdentity> accumulator
        )
            where TIdentity : allows ref struct
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
            return input =>
            {
                var next = parser(input);
                if (!next.HasValue)
                    return ParseResult.CastEmpty<T, TIdentity>(next);

                var result = accumulator(identity(), next.Value);
                for (var i = 1; i < count; i++)
                {
                    if (!next.HasValue)
                        return ParseResult.CastEmpty<T, TIdentity>(next);

                    result = accumulator(result, next.Value);
                    next = parser(next.Remainder);
                }
                return ParseResult.Success(result, input, next.Remainder);
            };
        }

        public TextParser<TIdentity> Repeat<TIdentity>(
            int count,
            TIdentity identity,
            Func<TIdentity, T, TIdentity> accumulator
        )
        {
            return parser.Repeat(count, () => identity, accumulator);
        }

        public TextParser<T> Repeat(int count, Func<T, T, T> accumulator)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
            return input =>
            {
                var next = parser(input);
                if (!next.HasValue)
                    return next;

                var result = next.Value;
                for (var i = 1; i < count; i++)
                {
                    if (!next.HasValue)
                        return next;

                    result = accumulator(result, next.Value);
                    next = parser(next.Remainder);
                }
                return ParseResult.Success(result, input, next.Remainder);
            };
        }

        public TextParser<TIdentity> RepeatedRange<TIdentity>(
            int minimum,
            int maximum,
            Func<TIdentity> identity,
            Func<TIdentity, T, TIdentity> accumulator
        )
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(minimum);
            if (maximum < minimum)
                throw new ArgumentOutOfRangeException(nameof(maximum), "Maximum value must be less than the minimum");

            return input =>
            {
                var elementFound = false;
                var result = default(TIdentity)!;
                var next = parser(input);
                var encountered = 0;
                while (next.HasValue)
                {
                    result = accumulator(elementFound ? result : identity(), next.Value);

                    elementFound = true;
                    next = parser(next.Remainder);
                    encountered++;
                    if (encountered >= maximum)
                        break;
                }

                return encountered >= minimum
                    ? ParseResult.Success(result, input, next.Remainder)
                    : ParseResult.CastEmpty<T, TIdentity>(next);
            };
        }

        public TextParser<TIdentity> RepeatedRange<TIdentity>(
            int minimum,
            int maximum,
            TIdentity identity,
            Func<TIdentity, T, TIdentity> accumulator
        )
        {
            return parser.RepeatedRange(minimum, maximum, () => identity, accumulator);
        }

        public TextParser<T> RepeatedRange(int minimum, int maximum, Func<T, T, T> accumulator)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(minimum);
            if (maximum < minimum)
                throw new ArgumentOutOfRangeException(nameof(maximum), "Maximum value must be less than the minimum");

            return input =>
            {
                var elementFound = false;
                var result = default(T)!;
                var next = parser(input);
                var encountered = 0;
                while (next.HasValue)
                {
                    result = elementFound ? accumulator(result, next.Value) : next.Value;

                    elementFound = true;
                    next = parser(next.Remainder);
                    encountered++;
                    if (encountered >= maximum)
                        break;
                }

                return encountered >= minimum ? ParseResult.Success(result, input, next.Remainder) : next;
            };
        }
    }
}
