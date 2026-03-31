// // @file TokenCursor.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using ZParse.Util;

namespace ZParse;

public readonly ref struct TokenCursor : IEquatable<TokenCursor>
{
    public ReadOnlySpan<char> Input { get; }

    public TextPosition Position { get; private init; }

    public bool IsAtEnd => Position.Index >= Input.Length;

    public ReadOnlySpan<char> Remaining => Input[Position.Index..];

    public TokenCursor(ReadOnlySpan<char> input)
        : this(input, TextPosition.Start) { }

    public TokenCursor(ReadOnlySpan<char> input, TextPosition position)
    {
        if (position.Index > input.Length)
            throw new ArgumentOutOfRangeException(nameof(position), "Position is out of bounds.");

        Input = input;
        Position = position;
    }

    public TokenResult<char> Advance()
    {
        if (IsAtEnd)
            return TokenResult.Empty<char>(this);

        var nextChar = Input[Position.Index];

        return TokenResult.Success(nextChar, this, new TokenCursor(Input, Position.Advance(nextChar)));
    }

    public char? Peek()
    {
        if (IsAtEnd)
            return null;

        return Input[Position.Index];
    }

    public TokenResult<Unit> ParseSymbol(ReadOnlySpan<char> symbol)
    {
        if (symbol.IsEmpty)
            throw new ArgumentException("Symbol cannot be empty.", nameof(symbol));

        var remainder = this;
        foreach (var c in symbol)
        {
            var next = remainder.Advance();
            if (!next.HasValue || next.Value != c)
                return TokenResult.Empty<Unit>(this);

            remainder = next.Remainder;
        }

        return TokenResult.Success(Unit.Value, this, remainder);
    }

    public TokenResult<Unit> ParseSymbolIgnoreCase(ReadOnlySpan<char> symbol)
    {
        if (symbol.IsEmpty)
            throw new ArgumentException("Symbol cannot be empty.", nameof(symbol));

        var remainder = this;
        foreach (var c in symbol)
        {
            var next = remainder.Advance();
            if (!next.HasValue || char.ToUpper(next.Value) != char.ToUpper(c))
                return TokenResult.Empty<Unit>(this);

            remainder = next.Remainder;
        }

        return TokenResult.Success(Unit.Value, this, remainder);
    }

    public TokenResult<char> ParseChar(char c)
    {
        var next = Advance();
        return next.HasValue && next.Value == c ? next : TokenResult.Empty<char>(this);
    }

    public TokenResult<char> ParseCharIn(params ReadOnlySpan<char> chars)
    {
        var next = Advance();
        return next.HasValue && chars.Contains(next.Value) ? next : TokenResult.Empty<char>(this);
    }

    public TokenResult<char> ParseAnyCharExcept(char c)
    {
        var next = Advance();
        return next.HasValue && next.Value != c ? next : TokenResult.Empty<char>(this);
    }

    public TokenResult<char> ParseAnyCharExceptIn(params ReadOnlySpan<char> chars)
    {
        var next = Advance();
        return next.HasValue && !chars.Contains(next.Value) ? next : TokenResult.Empty<char>(this);
    }

    public TokenResult<Unit> ParseUntilChar(char c)
    {
        var next = Advance();
        if (!next.HasValue || next.Value == c)
            return TokenResult.Success(Unit.Value, this, this);

        TokenCursor remainder;
        do
        {
            remainder = next.Remainder;
            next = remainder.Advance();
        } while (next.HasValue && next.Value != c);

        return TokenResult.Success(Unit.Value, this, remainder);
    }

    public TokenResult<Unit> ParseUntilCharIn(params ReadOnlySpan<char> chars)
    {
        var next = Advance();
        if (!next.HasValue || chars.Contains(next.Value))
            return TokenResult.Success(Unit.Value, this, this);

        TokenCursor remainder;
        do
        {
            remainder = next.Remainder;
            next = remainder.Advance();
        } while (next.HasValue && !chars.Contains(next.Value));

        return TokenResult.Success(Unit.Value, this, remainder);
    }

    public TokenResult<Unit> ParseIdentifier()
    {
        var next = Advance();
        if (!next.HasValue || !char.IsIdentifier(next.Value))
            return TokenResult.Empty<Unit>(this);

        TokenCursor remainder;
        do
        {
            remainder = next.Remainder;
            next = remainder.Advance();
        } while (next.HasValue && char.IsIdentifier(next.Value));

        return TokenResult.Success(Unit.Value, this, remainder);
    }

    public TokenResult<Unit> ParseWhitespace()
    {
        var next = Advance();
        if (!next.HasValue || !char.IsWhiteSpace(next.Value))
            return TokenResult.Empty<Unit>(this, next.Remainder);

        TokenCursor remainder;
        do
        {
            remainder = next.Remainder;
            next = remainder.Advance();
        } while (next.HasValue && char.IsWhiteSpace(next.Value));

        return TokenResult.Success(Unit.Value, this, remainder);
    }

    public TokenResult<Unit> ParseOptionalWhitespace()
    {
        var next = Advance();
        if (!next.HasValue || !char.IsWhiteSpace(next.Value))
            return TokenResult.Success(Unit.Value, this, this);

        TokenCursor remainder;
        do
        {
            remainder = next.Remainder;
            next = remainder.Advance();
        } while (next.HasValue && char.IsWhiteSpace(next.Value));

        return TokenResult.Success(Unit.Value, this, remainder);
    }

    public TokenResult<Unit> GenerateToken(int characterCount)
    {
        var remainder = this;
        for (var i = 0; i < characterCount; i++)
        {
            var next = remainder.Advance();
            if (!next.HasValue)
                return TokenResult.Empty<Unit>(this, remainder);

            remainder = next.Remainder;
        }

        return TokenResult.Success(Unit.Value, this, remainder);
    }

    public TokenCursor Reset()
    {
        return new TokenCursor(Input, TextPosition.Start);
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
