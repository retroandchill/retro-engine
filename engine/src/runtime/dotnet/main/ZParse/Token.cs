// // @file Token.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ZParse;

public readonly ref struct Token<T>(TextSegment text, T value)
    where T : allows ref struct
{
    public TextSegment Text { get; } = text;

    public T Value { get; } = value;
}
