// // @file ParseResult.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ZParse;

public readonly struct Unit
{
    public static Unit Value { get; } = new();
}

public readonly ref struct ParseResult<T>
    where T : allows ref struct
{
    public bool HasValue { get; }

    public T Value => HasValue ? field : throw new InvalidOperationException("No value available.");

    public ReadOnlySpan<char> Input { get; }

    public TextPosition Before { get; }

    public TextPosition After { get; }

    public ParseCursor Cursor => new(Input, Before);

    public ParseCursor Remainder => new(Input, After);

    internal ParseResult(T value, ReadOnlySpan<char> input, TextPosition before, TextPosition after)
    {
        HasValue = true;
        Value = value;
        Input = input;
        Before = before;
        After = after;
    }

    internal ParseResult(ReadOnlySpan<char> input, TextPosition before, TextPosition after)
    {
        HasValue = false;
        Value = default!;
        Input = input;
        Before = before;
        After = after;
    }
}

public static class ParseResult
{
    public static ParseResult<T> Success<T>(T value, ParseCursor input, ParseCursor remainder)
        where T : allows ref struct
    {
        return input.Input == remainder.Input
            ? new ParseResult<T>(value, input.Input, input.Position, remainder.Position)
            : throw new InvalidOperationException("Input and remainder must be from the same input.");
    }

    public static ParseResult<T> Empty<T>(ParseCursor parse)
        where T : allows ref struct
    {
        return new ParseResult<T>(parse.Input, parse.Position, parse.Position);
    }

    public static ParseResult<T> Empty<T>(ParseCursor input, ParseCursor remainder)
        where T : allows ref struct
    {
        return input.Input == remainder.Input
            ? new ParseResult<T>(input.Input, input.Position, remainder.Position)
            : throw new InvalidOperationException("Input and remainder must be from the same input.");
    }

    public static ParseResult<TOther> CastEmpty<T, TOther>(ParseResult<T> result)
        where T : allows ref struct
        where TOther : allows ref struct
    {
        return new ParseResult<TOther>(result.Input, result.Before, result.After);
    }
}
