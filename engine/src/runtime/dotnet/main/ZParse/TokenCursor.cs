// // @file TokenCursor.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace ZParse;

public enum ParseState
{
    Continue,
    StopBefore,
    StopAfter,
    Cancel,
}

public readonly ref struct TokenCursor : IEquatable<TokenCursor>
{
    public ReadOnlySpan<char> Input { get; }

    public TokenPosition Position { get; private init; }

    public bool IsAtEnd => Position.Index >= Input.Length;

    public ReadOnlySpan<char> Remaining => Input[Position.Index..];

    public TokenCursor(ReadOnlySpan<char> input)
        : this(input, TokenPosition.Start) { }

    internal TokenCursor(ReadOnlySpan<char> input, TokenPosition position)
    {
        Input = input;
        Position = position;
    }

    public TokenResult<char> ParseNext()
    {
        if (IsAtEnd)
            return TokenResult.Empty<char>(this);

        var nextChar = Input[Position.Index];

        return TokenResult.Success(nextChar, this, new TokenCursor(Input, Position.Advance(nextChar)));
    }

    public bool Equals(TokenCursor other)
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

    public static bool operator ==(TokenCursor lhs, TokenCursor rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(TokenCursor lhs, TokenCursor rhs)
    {
        return !(lhs == rhs);
    }
}
