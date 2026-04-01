// // @file ParseCursor.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ZParse;

public readonly ref struct TextSegment : IEquatable<TextSegment>
{
    private readonly ref readonly char _start;

    public TextPosition Position { get; }

    public int Length { get; }

    public bool IsAtEnd => Position.Index >= Length;

    public TextSegment(ReadOnlySpan<char> input)
        : this(input, TextPosition.Start) { }

    public TextSegment(ReadOnlySpan<char> input, TextPosition position)
    {
        if (position.Index > input.Length)
            throw new ArgumentOutOfRangeException(nameof(position), "Position is out of bounds.");

        _start = ref input.GetPinnableReference();
        Position = position;
        Length = input.Length;
    }

    private TextSegment(ref readonly char start, TextPosition position, int length)
    {
        _start = ref start;
        Position = position;
        Length = length;
    }

    public static TextSegment None => default;

    public static TextSegment Empty => new(ReadOnlySpan<char>.Empty, TextPosition.Start);

    public ParseResult<char> ConsumeChar()
    {
        if (IsAtEnd)
            return ParseResult.Empty<char>(this);

        var nextChar = Unsafe.Add(ref Unsafe.AsRef(in _start), Position.Index);

        return ParseResult.Success(nextChar, this, new TextSegment(in _start, Position.Advance(nextChar), Length - 1));
    }

    public char? PeekChar()
    {
        if (IsAtEnd)
            return null;

        return Unsafe.Add(ref Unsafe.AsRef(in _start), Position.Index);
    }

    public static TextSegment Between(TextSegment start, TextSegment end)
    {
        if (!Unsafe.AreSame(in start._start, in end._start))
            throw new ArgumentException("Start and end cursors must be from the same input.");

        return start.Position.Index > end.Position.Index
            ? new TextSegment(in start._start, end.Position, start.Position.Index - end.Position.Index)
            : new TextSegment(in start._start, start.Position, end.Position.Index - start.Position.Index);
    }

    public ParseResult<TextSegment> ParseToEnd()
    {
        var next = ConsumeChar();
        if (!next.HasValue)
            return ParseResult.Success(this, this, this);

        TextSegment remainder;
        do
        {
            remainder = next.Remainder;
            next = remainder.ConsumeChar();
        } while (next.HasValue);

        return ParseResult.Success(Between(this, remainder), this, remainder);
    }

    public TextSegment Until(TextSegment next)
    {
        var charCount = next.Position.Index - Position.Index;
        return First(charCount);
    }

    public TextSegment First(int length)
    {
        return new TextSegment(in _start, Position, length);
    }

    public TextSegment Skip(int count)
    {
        var p = Position;
        for (var i = 0; i < count; ++i)
        {
            p = p.Advance(Unsafe.Add(ref Unsafe.AsRef(in _start), p.Index));
        }

        return new TextSegment(in _start, p, Length - count);
    }

    public ref readonly char this[int index]
    {
        get
        {
            if (index < 0 || index >= Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return ref Unsafe.Add(ref Unsafe.AsRef(in _start), index);
        }
    }

    public static TextSegment operator +(TextSegment segment, TextPosition position)
    {
        var compositePosition = segment.Position + position;
        var distance = position.Index - segment.Position.Index;
        return new TextSegment(in segment._start, compositePosition, segment.Length - distance);
    }

    public TextSegment Slice(int start)
    {
        return Skip(start);
    }

    public TextSegment Slice(int start, int length)
    {
        return Skip(start).First(length);
    }

    public bool SameSourceAs(TextSegment other)
    {
        return Unsafe.AreSame(in _start, in other._start);
    }

    public bool Equals(TextSegment other)
    {
        return Unsafe.AreSame(in _start, in other._start) && Position == other.Position && Length == other.Length;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        throw new NotSupportedException("Ref structs cannot be compared for equality.");
    }

    public override int GetHashCode()
    {
        throw new NotSupportedException("Ref structs cannot be compared for equality.");
    }

    public static bool operator ==(TextSegment lhs, TextSegment rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(TextSegment lhs, TextSegment rhs)
    {
        return !(lhs == rhs);
    }

    public override string ToString()
    {
        return AsSpan().ToString();
    }

    public ReadOnlySpan<char> AsSpan()
    {
        return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef(in _start), Position.Index), Length);
    }

    public static implicit operator ReadOnlySpan<char>(TextSegment segment) => segment.AsSpan();
}
