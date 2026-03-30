// // @file TokenPosition.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ZParse;

public readonly record struct TokenPosition(int Index, int Line, int Column)
{
    public static TokenPosition Start => new(0, 1, 1);

    public TokenPosition Advance(char character)
    {
        return character == '\n'
            ? new TokenPosition(Index + 1, Line + 1, 1)
            : new TokenPosition(Index + 1, Line, Column + 1);
    }
}
