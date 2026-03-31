// // @file ParseException.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ZParse;

public sealed class ParseException : Exception
{
    public TextPosition Position { get; }

    public ParseException(TextPosition position, string message)
        : base(message)
    {
        Position = position;
    }

    public ParseException(TextPosition position, string message, Exception innerException)
        : base(message, innerException)
    {
        Position = position;
    }
}
