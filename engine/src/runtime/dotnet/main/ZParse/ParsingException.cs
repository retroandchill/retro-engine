// // @file ParsingException.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ZParse;

/// <summary>
/// Exception thrown when parsing fails.
/// </summary>
public sealed class ParsingException : Exception
{
    /// <summary>
    /// The position of the error in the input text, or <see cref="TextPosition.Empty"/> if no position is specified.
    /// </summary>
    public TextPosition ErrorPosition { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParsingException" /> class with a default error message.
    /// </summary>
    public ParsingException()
        : this("Parsing failed.", TextPosition.Empty) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParsingException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public ParsingException(string message)
        : this(message, TextPosition.Empty) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParsingException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ParsingException(string message, Exception? innerException)
        : this(message, TextPosition.Empty, innerException) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParsingException" /> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="errorPosition">The position of the error in the input text.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ParsingException(string message, TextPosition errorPosition, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorPosition = errorPosition;
    }
}
