// // @file TokenPosition.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Numerics;

namespace ZParse;

public readonly record struct TextPosition(int Index, int Line, int Column)
    : IAdditionOperators<TextPosition, TextPosition, TextPosition>
{
    public static TextPosition Start => new(0, 1, 1);

    public static TextPosition Empty => default;

    public TextPosition Advance(char character)
    {
        return character == '\n'
            ? new TextPosition(Index + 1, Line + 1, 1)
            : new TextPosition(Index + 1, Line, Column + 1);
    }

    public static TextPosition operator +(TextPosition left, TextPosition right)
    {
        // Line and column are 1-indexed, so we need to subtract 1 so that a right position of 1,1 will leave the left position unchanged.
        return new TextPosition(left.Index + right.Index, left.Line + right.Line - 1, left.Column + right.Column - 1);
    }
}
