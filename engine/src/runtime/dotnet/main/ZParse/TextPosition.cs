// // @file TextPosition.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;

namespace ZParse;

/// <summary>
/// Represents the position of a character in a text.
/// </summary>
public readonly struct TextPosition : IEquatable<TextPosition>, IEqualityOperators<TextPosition, TextPosition, bool>
{
    /// <summary>
    /// The zero-based absolute index of the character in the text.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// The one-based line number of the character in the text.
    /// </summary>
    public int Line { get; }

    /// <summary>
    /// The one-based column number of the character in the text.
    /// </summary>
    public int Column { get; }

    /// <summary>
    /// Creates a new <see cref="TextPosition"/>.
    /// </summary>
    /// <param name="index">The absolute index</param>
    /// <param name="line">The line number</param>
    /// <param name="column">The column number</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public TextPosition(int index, int line, int column)
    {
#if CHECKED
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(line), "Absolute positions start at 0.");
        if (line < 1)
            throw new ArgumentOutOfRangeException(nameof(line), "Line numbering starts at 1.");
        if (column < 1)
            throw new ArgumentOutOfRangeException(nameof(column), "Column numbering starts at 1.");
#endif
        Index = index;
        Line = line;
        Column = column;
    }

    /// <summary>
    /// Represents the state at the start of the string.
    /// </summary>
    public static TextPosition Zero { get; } = new(0, 1, 1);

    /// <summary>
    /// Represents a position with no value.
    /// </summary>
    public static TextPosition Empty => default;

    /// <summary>
    /// Returns true if the position is valid.
    /// </summary>
    public bool IsValid => Line > 0;

    /// <summary>
    /// Advance over <paramref name="over"/>, advancing the line and column numbers accordingly.
    /// </summary>
    /// <param name="over">The character to advance over</param>
    /// <returns>The new position</returns>
    public TextPosition Advance(char over)
    {
        return over == '\n' ? new TextPosition(Index + 1, Line + 1, 1) : new TextPosition(Index + 1, Line, Column + 1);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Index} (line {Line}, column {Column})";
    }

    /// <inheritdoc />
    public bool Equals(TextPosition other)
    {
        return Index == other.Index && Line == other.Line && Column == other.Column;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is TextPosition other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Index, Line, Column);
    }

    /// <inheritdoc />
    public static bool operator ==(TextPosition left, TextPosition right)
    {
        return left.Equals(right);
    }

    /// <inheritdoc />
    public static bool operator !=(TextPosition left, TextPosition right)
    {
        return !(left == right);
    }
}
