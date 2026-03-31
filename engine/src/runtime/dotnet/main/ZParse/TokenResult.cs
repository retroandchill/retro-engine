// // @file TokenResult.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ZParse;

public readonly struct Unit
{
    public static Unit Value { get; } = new();
}

public readonly ref struct TokenResult<T>
    where T : allows ref struct
{
    public bool HasValue { get; }

    public T Value => HasValue ? field : throw new InvalidOperationException("No value available.");

    public ReadOnlySpan<char> Input { get; }

    public TextPosition Before { get; }

    public TextPosition After { get; }

    public ReadOnlySpan<char> TokenText => Input.Slice(Before.Index, After.Index - Before.Index);

    public TokenCursor Cursor => new(Input, Before);

    public TokenCursor Remainder => new(Input, After);

    internal TokenResult(T value, ReadOnlySpan<char> input, TextPosition before, TextPosition after)
    {
        HasValue = true;
        Value = value;
        Input = input;
        Before = before;
        After = after;
    }

    internal TokenResult(ReadOnlySpan<char> input, TextPosition before, TextPosition after)
    {
        HasValue = false;
        Value = default!;
        Input = input;
        Before = before;
        After = after;
    }
}

public static class TokenResult
{
    public static TokenResult<T> Success<T>(T value, TokenCursor input, TokenCursor remainder)
        where T : allows ref struct
    {
        return input.Input == remainder.Input
            ? new TokenResult<T>(value, input.Input, input.Position, remainder.Position)
            : throw new InvalidOperationException("Input and remainder must be from the same input.");
    }

    public static TokenResult<T> Empty<T>(TokenCursor token)
        where T : allows ref struct
    {
        return new TokenResult<T>(token.Input, token.Position, token.Position);
    }

    public static TokenResult<T> Empty<T>(TokenCursor input, TokenCursor remainder)
        where T : allows ref struct
    {
        return input.Input == remainder.Input
            ? new TokenResult<T>(input.Input, input.Position, remainder.Position)
            : throw new InvalidOperationException("Input and remainder must be from the same input.");
    }

    public static TokenResult<TOther> CastEmpty<T, TOther>(TokenResult<T> result)
        where T : allows ref struct
        where TOther : allows ref struct
    {
        return new TokenResult<TOther>(result.Input, result.Before, result.After);
    }
}
