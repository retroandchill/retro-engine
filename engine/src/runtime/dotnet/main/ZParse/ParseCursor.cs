// // @file ParseCursor.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using ZParse.Util;

namespace ZParse;

public readonly ref struct ParseCursor : IEquatable<ParseCursor>
{
    public ReadOnlySpan<char> Input { get; }

    public TextPosition Position { get; private init; }

    public bool IsAtEnd => Position.Index >= Input.Length;

    public ReadOnlySpan<char> Remaining => Input[Position.Index..];

    public ParseCursor(ReadOnlySpan<char> input)
        : this(input, TextPosition.Start) { }

    public ParseCursor(ReadOnlySpan<char> input, TextPosition position)
    {
        if (position.Index > input.Length)
            throw new ArgumentOutOfRangeException(nameof(position), "Position is out of bounds.");

        Input = input;
        Position = position;
    }

    public ParseResult<char> Advance()
    {
        if (IsAtEnd)
            return ParseResult.Empty<char>(this);

        var nextChar = Input[Position.Index];

        return ParseResult.Success(nextChar, this, new ParseCursor(Input, Position.Advance(nextChar)));
    }

    public char? Peek()
    {
        if (IsAtEnd)
            return null;

        return Input[Position.Index];
    }

    public ParseCursor Reset()
    {
        return new ParseCursor(Input, TextPosition.Start);
    }

    public static ReadOnlySpan<char> Between(ParseCursor start, ParseCursor end)
    {
        if (start.Input != end.Input)
            throw new ArgumentException("Start and end cursors must be from the same input.");

        if (start.Position.Index == end.Position.Index)
        {
            return ReadOnlySpan<char>.Empty;
        }

        return start.Position.Index > end.Position.Index
            ? start.Input[end.Position.Index..start.Position.Index]
            : start.Input[start.Position.Index..end.Position.Index];
    }

    public ParseResult<ReadOnlySpan<char>> ParseToEnd()
    {
        var next = Advance();
        if (!next.HasValue)
            return ParseResult.Success(ReadOnlySpan<char>.Empty, this, this);

        ParseCursor remainder;
        do
        {
            remainder = next.Remainder;
            next = remainder.Advance();
        } while (next.HasValue);

        return ParseResult.Success(Between(this, remainder), this, remainder);
    }

    public bool Equals(ParseCursor other)
    {
        return Input == other.Input && Position == other.Position;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        throw new NotSupportedException("Ref structs cannot be compared for equality.");
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException("Ref structs cannot be compared for equality.");
    }

    public static bool operator ==(ParseCursor lhs, ParseCursor rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(ParseCursor lhs, ParseCursor rhs)
    {
        return !(lhs == rhs);
    }
}
