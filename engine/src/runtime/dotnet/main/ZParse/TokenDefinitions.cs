// // @file TokenDefinitions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using ZParse.Enumeration;

namespace ZParse;

public readonly record struct TokenDefinitions<T>(
    ImmutableArray<Func<ParseCursor, ParseResult<T>>> Definitions,
    bool IgnoreWhitespace = false
)
    where T : allows ref struct
{
    public TokenEnumerator<T> GetTokens(ReadOnlySpan<char> input) => new(input, this);
}
