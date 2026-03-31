// // @file Combinators.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ZParse;

public static class Combinators
{
    extension<T>(TokenResult<T> result)
        where T : allows ref struct
    {
        public TokenResult<TResult> Select<TResult>(Func<T, ReadOnlySpan<char>, TResult> selector)
            where TResult : allows ref struct
        {
            return result.HasValue
                ? TokenResult.Success(selector(result.Value, result.TokenText), result.Cursor, result.Remainder)
                : TokenResult.CastEmpty<T, TResult>(result);
        }

        public TokenResult<TResult> SelectMany<TResult>(Func<T, ReadOnlySpan<char>, TokenResult<TResult>> selector)
            where TResult : allows ref struct
        {
            return result.HasValue
                ? selector(result.Value, result.TokenText)
                : TokenResult.CastEmpty<T, TResult>(result);
        }

        public TokenResult<T> Where(Func<T, ReadOnlySpan<char>, bool> predicate)
        {
            if (!result.HasValue || predicate(result.Value, result.TokenText))
                return result;

            return TokenResult.Empty<T>(result.Cursor, result.Remainder);
        }
    }
}
