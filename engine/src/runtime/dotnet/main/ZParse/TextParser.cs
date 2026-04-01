// // @file TextParser.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ZParse;

public delegate ParseResult<T> TextParser<T>(TextSegment input)
    where T : allows ref struct;

public static class TextParserExtensions
{
    extension<T>(TextParser<T> parser)
        where T : allows ref struct
    {
        public ParseResult<T> TryParse(ReadOnlySpan<char> input) => parser(new TextSegment(input));

        public ParseResult<T> TryParse(TextSegment input) => parser(input);
    }
}
