// // @file TokenEnumerator.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections;
using ZParse.Parsers;

namespace ZParse.Enumeration;

public ref struct TokenEnumerator<T>(TextSegment input, TokenDefinitions<T> definitions) : IEnumerator<Token<T>>
    where T : allows ref struct
{
    private readonly TextSegment _input = input;
    private TextSegment _cursor = input;

    object IEnumerator.Current => throw new NotSupportedException("Token's cannot be cast to object.");
    public Token<T> Current { get; private set; }

    public TokenEnumerator<T> GetEnumerator() => this;

    public bool MoveNext()
    {
        if (_cursor.IsAtEnd)
            return false;

        if (definitions.IgnoreWhitespace)
        {
            var next = _cursor.ConsumeChar();
            while (next.HasValue && char.IsWhiteSpace(next.Value))
            {
                next = next.Remainder.ConsumeChar();
            }

            if (!next.HasValue)
                return false;
        }

        foreach (var parser in definitions.Definitions)
        {
            ParseResult<T> result;
            try
            {
                result = parser(_cursor);
            }
            catch (Exception ex)
            {
                throw new ParseException(_cursor.Position, "Unexpected error when parsing token.", ex);
            }

            if (!result.HasValue)
                continue;

            _cursor = result.Remainder;
            Current = new Token<T>(TextSegment.Between(result.Input, result.Remainder), result.Value);
            return true;
        }

        throw new ParseException(_cursor.Position, "Unexpected token.");
    }

    public void Reset()
    {
        _cursor = _input;
    }

    public void Dispose()
    {
        // Nothing to dispose
    }
}
