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
                ? ParseResult.Success(selector(result.Value), result.Cursor, result.Remainder)
                : ParseResult.CastEmpty<T, TResult>(result);
        }

        public ParseResult<TResult> Select<TContext, TResult>(TContext context, Func<TContext, T, TResult> selector)
            where TContext : allows ref struct
            where TResult : allows ref struct
        {
            return result.HasValue
                ? ParseResult.Success(selector(context, result.Value), result.Cursor, result.Remainder)
                : ParseResult.CastEmpty<T, TResult>(result);
        }

        public ParseResult<T> Where(Func<T, bool> predicate)
        {
            if (!result.HasValue || predicate(result.Value))
                return result;

            return ParseResult.Empty<T>(result.Cursor, result.Remainder);
        }

        public ParseResult<T> Where<TContext>(TContext context, Func<TContext, T, bool> predicate)
        {
            if (!result.HasValue || predicate(context, result.Value))
                return result;

            return ParseResult.Empty<T>(result.Cursor, result.Remainder);
        }

        public ParseResult<T> OrElse(Func<ParseCursor, ParseResult<T>> fallback)
        {
            return result.HasValue ? result : fallback(result.Cursor);
        }

        public ParseResult<T> OrElse<TContext>(TContext context, Func<TContext, ParseCursor, ParseResult<T>> fallback)
            where TContext : allows ref struct
        {
            return result.HasValue ? result : fallback(context, result.Cursor);
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

    extension(ParseCursor input)
    {
        public ParseResult<TResult> ParseSequence<T1, T2, TResult>(
            Func<ParseCursor, ParseResult<T1>> first,
            Func<ParseCursor, ParseResult<T2>> second,
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
                firstResult.Cursor,
                secondResult.Remainder
            );
        }

        public ParseResult<TResult> ParseSequence<T1, T2, T3, TResult>(
            Func<ParseCursor, ParseResult<T1>> first,
            Func<ParseCursor, ParseResult<T2>> second,
            Func<ParseCursor, ParseResult<T3>> third,
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
                firstResult.Cursor,
                secondResult.Remainder
            );
        }

        public ParseResult<TResult> ParseSequence<T1, T2, T3, T4, TResult>(
            Func<ParseCursor, ParseResult<T1>> first,
            Func<ParseCursor, ParseResult<T2>> second,
            Func<ParseCursor, ParseResult<T3>> third,
            Func<ParseCursor, ParseResult<T4>> fourth,
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
                firstResult.Cursor,
                secondResult.Remainder
            );
        }

        public ParseResult<TResult> ParseSequence<T1, T2, T3, T4, T5, TResult>(
            Func<ParseCursor, ParseResult<T1>> first,
            Func<ParseCursor, ParseResult<T2>> second,
            Func<ParseCursor, ParseResult<T3>> third,
            Func<ParseCursor, ParseResult<T4>> fourth,
            Func<ParseCursor, ParseResult<T5>> fifth,
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
                firstResult.Cursor,
                secondResult.Remainder
            );
        }

        public ParseResult<TResult> ParseSequence<T1, T2, T3, T4, T5, T6, TResult>(
            Func<ParseCursor, ParseResult<T1>> first,
            Func<ParseCursor, ParseResult<T2>> second,
            Func<ParseCursor, ParseResult<T3>> third,
            Func<ParseCursor, ParseResult<T4>> fourth,
            Func<ParseCursor, ParseResult<T5>> fifth,
            Func<ParseCursor, ParseResult<T6>> sixth,
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
                firstResult.Cursor,
                secondResult.Remainder
            );
        }

        public ParseResult<TResult> ParseSequence<TContext, T1, T2, TResult>(
            TContext context,
            Func<TContext, ParseCursor, ParseResult<T1>> first,
            Func<TContext, ParseCursor, ParseResult<T2>> second,
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
                firstResult.Cursor,
                secondResult.Remainder
            );
        }

        public ParseResult<TResult> ParseSequence<TContext, T1, T2, T3, TResult>(
            TContext context,
            Func<TContext, ParseCursor, ParseResult<T1>> first,
            Func<TContext, ParseCursor, ParseResult<T2>> second,
            Func<TContext, ParseCursor, ParseResult<T3>> third,
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
                firstResult.Cursor,
                secondResult.Remainder
            );
        }

        public ParseResult<TResult> ParseSequence<TContext, T1, T2, T3, T4, TResult>(
            TContext context,
            Func<TContext, ParseCursor, ParseResult<T1>> first,
            Func<TContext, ParseCursor, ParseResult<T2>> second,
            Func<TContext, ParseCursor, ParseResult<T3>> third,
            Func<TContext, ParseCursor, ParseResult<T4>> fourth,
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
                firstResult.Cursor,
                secondResult.Remainder
            );
        }

        public ParseResult<TResult> ParseSequence<TContext, T1, T2, T3, T4, T5, TResult>(
            TContext context,
            Func<TContext, ParseCursor, ParseResult<T1>> first,
            Func<TContext, ParseCursor, ParseResult<T2>> second,
            Func<TContext, ParseCursor, ParseResult<T3>> third,
            Func<TContext, ParseCursor, ParseResult<T4>> fourth,
            Func<TContext, ParseCursor, ParseResult<T5>> fifth,
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
                firstResult.Cursor,
                secondResult.Remainder
            );
        }

        public ParseResult<TResult> ParseSequence<TContext, T1, T2, T3, T4, T5, T6, TResult>(
            TContext context,
            Func<TContext, ParseCursor, ParseResult<T1>> first,
            Func<TContext, ParseCursor, ParseResult<T2>> second,
            Func<TContext, ParseCursor, ParseResult<T3>> third,
            Func<TContext, ParseCursor, ParseResult<T4>> fourth,
            Func<TContext, ParseCursor, ParseResult<T5>> fifth,
            Func<TContext, ParseCursor, ParseResult<T6>> sixth,
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
                firstResult.Cursor,
                secondResult.Remainder
            );
        }
    }
}
