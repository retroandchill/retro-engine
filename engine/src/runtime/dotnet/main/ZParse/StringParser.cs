// // @file StringParser.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ZParse;

/// <summary>
/// Basic delegate for parsing a string.
/// </summary>
/// <typeparam name="T">The type of the result from the parse</typeparam>
public delegate Result<T> StringParser<T>(StringView input)
    where T : allows ref struct;

/// <summary>
/// Extension methods for <see cref="StringParser{T}"/>
/// </summary>
public static class StringParserExtensions
{
    /// <param name="parser">The parser to use</param>
    /// <typeparam name="T">The type of the result</typeparam>
    extension<T>(StringParser<T> parser)
    {
        /// <summary>
        /// Attempt to parse the specified input string.
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>The result of the parse</returns>
        public Result<T> TryParse(ReadOnlySpan<char> input)
        {
            ArgumentNullException.ThrowIfNull(parser);
            return parser(new StringView(input));
        }

        /// <summary>
        /// Parse the specified input string.
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>The result of the parse</returns>
        /// <exception cref="ParsingException">If the parsing fails</exception>
        public T Parse(ReadOnlySpan<char> input)
        {
            ArgumentNullException.ThrowIfNull(parser);
            var result = parser.TryParse(input);

            return result.Success ? result.Value : throw new ParsingException(result.ToString(), result.ErrorPosition);
        }
    }
}
