// // @file StringView.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ZLinq;
using ZLinq.Internal;

namespace ZParse;

/// <summary>
/// Represents a view over a character buffer, containing information about the intial position,
/// as well as the offset data.
/// </summary>
public readonly ref struct StringView : IEquatable<StringView>
{
    private const string CannotBeBoxed = "StringView is a ref struct, and thus cannot be boxed.";

    private readonly ref char _start;

    /// <summary>
    /// The position of the first character in the view.
    /// </summary>
    public TextPosition Position { get; }

    /// <summary>
    /// The length of the view.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Construct a view encompassing an entire string.
    /// </summary>
    /// <param name="source">The source string.</param>
    public StringView(ReadOnlySpan<char> source)
        : this(source, TextPosition.Zero, source.Length) { }

    /// <summary>
    /// Construct a string view for a substring of <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="position">The start of the view.</param>
    /// <param name="length">The length of the view.</param>
    public StringView(ReadOnlySpan<char> source, TextPosition position, int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length), "The length must be non-negative.");
        if (source.Length < position.Index + length)
            throw new ArgumentOutOfRangeException(nameof(length), "The token extends beyond the end of the input.");
        _start = ref Unsafe.AsRef(in source.GetPinnableReference());
        Position = position;
        Length = length;
    }

    private StringView(ref readonly char start, TextPosition position, int length)
    {
        _start = ref Unsafe.AsRef(in start);
        Position = position;
        Length = length;
    }

    /// <summary>
    /// A view with no value.
    /// </summary>
    public static StringView None => default;

    /// <summary>
    /// A view with no value.
    /// </summary>
    public static StringView Empty => new([], TextPosition.Zero, 0);

    /// <summary>
    /// True if there are no characters left in the view.
    /// </summary>
    public bool IsAtEnd
    {
        get
        {
            EnsureValid();
            return Length == 0;
        }
    }

    private bool IsValid => Position != TextPosition.Empty;

    private void EnsureValid()
    {
        if (!IsValid)
            throw new InvalidOperationException("The view does not have a value.");
    }

    /// <summary>
    /// Get the next character in the view.
    /// </summary>
    /// <returns>The next character if valid.</returns>
    public Result<char> TryGetNext()
    {
        EnsureValid();

        if (IsAtEnd)
            return Result.Empty<char>(this);

        var ch = Unsafe.Add(ref _start, Position.Index);
        return Result.Value(ch, this, new StringView(ref _start, Position.Advance(ch), Length - 1));
    }

    /// <summary>
    /// This method is not supported as views cannot be boxed. To compare two views, use operator==.
    /// </summary>
    /// <exception cref="NotSupportedException">
    /// Always thrown by this method.
    /// </exception>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj)
    {
        throw new NotSupportedException(CannotBeBoxed);
    }

    /// <inheritdoc />
    public bool Equals(StringView other)
    {
        return Unsafe.AreSame(ref _start, ref other._start)
            && Position.Equals(other.Position)
            && Length == other.Length;
    }

    /// <summary>
    /// Compare two views using value semantics.
    /// </summary>
    /// <param name="other">The span representing the other view to compare with.</param>
    /// <param name="comparison">The string comparison type to use for the comparison.</param>
    /// <returns>True if the views are equal, false otherwise.</returns>
    public bool Equals(ReadOnlySpan<char> other, StringComparison comparison)
    {
        EnsureValid();
        return other.Equals(AsSpan(), comparison);
    }

    /// <summary>
    /// This method is not supported as views cannot be boxed. To compare two views, use operator==.
    /// </summary>
    /// <exception cref="NotSupportedException">
    /// Always thrown by this method.
    /// </exception>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode()
    {
        throw new NotSupportedException(CannotBeBoxed);
    }

    /// <summary>
    /// Compare two views using source identity semantics.
    /// </summary>
    /// <param name="lhs">One view.</param>
    /// <param name="rhs">Another view.</param>
    /// <returns>True if the views are the same.</returns>
    public static bool operator ==(StringView lhs, StringView rhs)
    {
        return lhs.Equals(rhs);
    }

    /// <summary>
    /// Compare two views using source identity semantics.
    /// </summary>
    /// <param name="lhs">One view.</param>
    /// <param name="rhs">Another view.</param>
    /// <returns>True if the views are the different.</returns>
    public static bool operator !=(StringView lhs, StringView rhs)
    {
        return !(lhs == rhs);
    }

    /// <summary>
    /// Return a new view from the start of this view to the beginning of another.
    /// </summary>
    /// <param name="next">The next view.</param>
    /// <returns>A sub-view.</returns>
    /// <exception cref="ArgumentException">If the views are not on the same source string.</exception>
    public StringView Until(StringView next)
    {
        next.EnsureValid();
        if (!Unsafe.AreSame(ref _start, ref next._start))
            throw new ArgumentException("The views are on different source strings.", nameof(next));

        var charCount = next.Position.Index - Position.Index;
        return First(charCount);
    }

    /// <summary>
    /// Return a view comprising the first <paramref name="length"/> characters of this view.
    /// </summary>
    /// <param name="length">The number of characters to return.</param>
    /// <returns>The sub-view.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the length exceeds the view's length.</exception>
    public StringView First(int length)
    {
        if (length > Length)
            throw new ArgumentOutOfRangeException(nameof(length), "Length exceeds the source view's length.");

        return new StringView(ref _start, Position, length);
    }

    /// <summary>
    /// Advance the view by <paramref name="count"/> characters.
    /// </summary>
    /// <param name="count">The number of characters to advance over</param>
    /// <returns>The updated view</returns>
    /// <exception cref="ArgumentOutOfRangeException">If cannot skip over that many characters.</exception>
    public StringView Skip(int count)
    {
        EnsureValid();
        if (count > Length)
            throw new ArgumentOutOfRangeException(nameof(count), "Count exceeds the source view's length.");

        var p = Position;
        for (var i = 0; i < count; ++i)
        {
            p = p.Advance(Unsafe.Add(ref _start, Position.Index));
        }

        return new StringView(ref _start, p, Length - count);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return IsValid ? AsSpan().ToString() : string.Empty;
    }

    /// <summary>
    /// Returns a span representing the view.
    /// </summary>
    /// <returns>The resultant span.</returns>
    public ReadOnlySpan<char> AsSpan()
    {
        EnsureValid();
        return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref _start, Position.Index), Length);
    }

    /// <summary>
    /// Implicitly convert a view to a span.
    /// </summary>
    /// <param name="view">The view to convert</param>
    /// <returns>The value as a span.</returns>
    public static implicit operator ReadOnlySpan<char>(StringView view)
    {
        return view.AsSpan();
    }

    /// <summary>
    /// Implicitly convert a span to a view.
    /// </summary>
    /// <param name="index"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public char this[int index]
    {
        get
        {
            EnsureValid();
            if ((uint)index >= (uint)Length)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index exceeds the source view's length.");

            return Unsafe.Add(ref _start, Position.Index + index);
        }
    }

    /// <summary>
    /// Forms a slice of the view starting at <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The index to start the slice at.</param>
    /// <returns>A view that consists of all elements starting at <paramref name="index"/> until the end of the string.</returns>
    public StringView Slice(int index)
    {
        return Skip(index);
    }

    /// <summary>
    /// Forms a slice of the specified length out of the current view starting at the specified index.
    /// </summary>
    /// <param name="index">The index at which to begin the slice.</param>
    /// <param name="count">The desired length of the slice.</param>
    /// <returns>A view of <paramref name="count"/> elements starting at <paramref name="index"/>.</returns>
    public StringView Slice(int index, int count)
    {
        return Skip(index).First(count);
    }

    /// <summary>
    /// Get an enumerator for the view.
    /// </summary>
    /// <returns>An enumerator to iterate over the view</returns>
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    /// <summary>
    /// Gets a ZLinq compatible value enumerable for the view.
    /// </summary>
    /// <returns>The value enumerable for the view.</returns>
    public ValueEnumerable<Enumerator, char> AsValueEnumerable()
    {
        return new ValueEnumerable<Enumerator, char>(GetEnumerator());
    }

    /// <summary>
    /// Enumerator for <see cref="StringView"/>.
    /// </summary>
    /// <param name="owner">The view to enumerate.</param>
    public ref struct Enumerator(StringView owner) : IEnumerator<char>, IValueEnumerator<char>
    {
        private readonly StringView _owner = owner;

        private int _index;

        object IEnumerator.Current => Current;

        /// <inheritdoc />
        public char Current => _owner[_index];

        /// <inheritdoc />
        public bool MoveNext()
        {
            if (_index >= _owner.Length)
                return false;

            _index++;
            return true;
        }

        /// <inheritdoc />
        public bool TryGetNext(out char current)
        {
            if (MoveNext())
            {
                current = Current;
                return true;
            }

            current = '\0';
            return false;
        }

        /// <inheritdoc />
        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = _owner.Length;
            return true;
        }

        /// <inheritdoc />
        public bool TryGetSpan(out ReadOnlySpan<char> span)
        {
            span = _owner.AsSpan();
            return true;
        }

        /// <inheritdoc />
        public bool TryCopyTo(scoped Span<char> destination, Index offset)
        {
            if (!EnumeratorHelper.TryGetSlice(_owner.AsSpan(), offset, destination.Length, out var slice))
                return false;

            slice.CopyTo(destination);
            return true;
        }

        /// <inheritdoc />
        public void Reset()
        {
            _index = -1;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // No resources to dispose of
        }
    }
}
