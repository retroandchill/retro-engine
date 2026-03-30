// // @file Token.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ZParse;

public readonly ref struct Token<T>(ReadOnlySpan<char> text, T value, int line, int column)
    where T : allows ref struct
{
    public ReadOnlySpan<char> Text { get; } = text;

    public T Value { get; } = value;

    public int Line { get; } = line;

    public int Column { get; } = column;
}
