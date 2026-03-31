// // @file TokenEnumerator.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections;

namespace ZParse.Enumeration;

public ref struct TokenEnumerator<T>(ReadOnlySpan<char> input, TokenDefinitions<T> definitions) : IEnumerator<Token<T>>
    where T : allows ref struct
{
    private TokenCursor _cursor = new(input);

    object IEnumerator.Current => throw new NotSupportedException("Token's cannot be cast to object.");
    public Token<T> Current { get; private set; }

    public TokenEnumerator<T> GetEnumerator() => this;

    public bool MoveNext()
    {
        if (_cursor.IsAtEnd)
            return false;

        if (definitions.IgnoreWhitespace)
        {
            _cursor = _cursor.ParseOptionalWhitespace().Remainder;
        }

        foreach (var parser in definitions.Definitions)
        {
            TokenResult<T> result;
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
            Current = new Token<T>(result.TokenText, result.Value, result.Before.Line, result.Before.Column);
            return true;
        }

        throw new ParseException(_cursor.Position, "Unexpected token.");
    }

    public void Reset()
    {
        _cursor = _cursor.Reset();
    }

    public void Dispose()
    {
        // Nothing to dispose
    }
}
