// // @file Parse.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Globalization;
using System.Text;
using LinkDotNet.StringBuilder;

namespace RetroEngine.Portable.Utils;

public static class Parse
{
    public static bool QuotedString(ReadOnlySpan<char> buffer, StringBuilder builder, out int numCharsRead)
    {
        numCharsRead = 0;

        var start = buffer;

        if (buffer[0] != '"')
            return false;

        buffer = buffer[1..];

        const string stopCharacters = "\"\n\r";
        const string stopAndEscapeCharacters = $"{stopCharacters}\\";

        while (true)
        {
            var unescapedEnd = FindFirstOrEnd(buffer, stopAndEscapeCharacters);
            var unescapedSubstring = buffer[..unescapedEnd.Length];
            builder.Append(unescapedSubstring);
            buffer = unescapedEnd;

            // Found a stop character
            if (buffer[0] != '\\')
                break;

            buffer = buffer[1..];
            switch (buffer[0])
            {
                case '\\':
                    builder.Append('\\');
                    buffer = buffer[1..];
                    break;
                case '"':
                    builder.Append('"');
                    buffer = buffer[1..];
                    break;
                case '\'':
                    builder.Append('\'');
                    buffer = buffer[1..];
                    break;
                case 'n':
                    builder.Append('\n');
                    buffer = buffer[1..];
                    break;
                case 'r':
                    builder.Append('\r');
                    buffer = buffer[1..];
                    break;
                case 't':
                    builder.Append('\t');
                    buffer = buffer[1..];
                    break;
                default:
                {
                    if (char.IsOctalDigit(buffer[0]))
                    {
                        using var octSequence = new ValueStringBuilder();
                        while (ShouldParse(buffer[0]) && char.IsOctalDigit(buffer[0]) && octSequence.Length < 3)
                        {
                            octSequence.Append(buffer[0]);
                            buffer = buffer[1..];
                        }

                        builder.Append((char)octSequence.AsSpan().ParseOctal());
                    }
                    else
                        switch (buffer[0])
                        {
                            case 'x' when char.IsAsciiHexDigit(buffer[1]):
                            {
                                buffer = buffer[1..];

                                using var hexSequence = new ValueStringBuilder();
                                while (ShouldParse(buffer[0]) && char.IsAsciiHexDigit(buffer[0]))
                                {
                                    hexSequence.Append(buffer[0]);
                                    buffer = buffer[1..];
                                }

                                builder.Append((char)ushort.Parse(hexSequence.AsSpan(), NumberStyles.HexNumber));
                                break;
                            }
                            case 'u' when char.IsAsciiHexDigit(buffer[1]):
                            {
                                buffer = buffer[1..];

                                using var unicodeSequence = new ValueStringBuilder();
                                while (
                                    ShouldParse(buffer[0])
                                    && char.IsAsciiHexDigit(buffer[0])
                                    && unicodeSequence.Length < 4
                                )
                                {
                                    unicodeSequence.Append(buffer[0]);
                                    buffer = buffer[1..];
                                }

                                builder.Append((char)ushort.Parse(unicodeSequence.AsSpan(), NumberStyles.HexNumber));
                                break;
                            }
                            case 'U' when char.IsAsciiHexDigit(buffer[1]):
                            {
                                buffer = buffer[1..];

                                using var unicodeSequence = new ValueStringBuilder();
                                while (
                                    ShouldParse(buffer[0])
                                    && char.IsAsciiHexDigit(buffer[0])
                                    && unicodeSequence.Length < 8
                                )
                                {
                                    unicodeSequence.Append(buffer[0]);
                                    buffer = buffer[1..];
                                }
                                unchecked
                                {
                                    var utf32Data = (int)uint.Parse(unicodeSequence.AsSpan(), NumberStyles.HexNumber);
                                    builder.Append(char.ConvertFromUtf32(utf32Data));
                                }

                                break;
                            }
                            default:
                                builder.Append('\\');
                                builder.Append(buffer[0]);
                                buffer = buffer[1..];
                                break;
                        }

                    break;
                }
            }
        }

        if (buffer[0] != '"')
            return false;
        buffer = buffer[1..];

        numCharsRead = start.Length - buffer.Length;
        return true;

        static bool ShouldParse(char c) => stopCharacters.Contains(c);
    }

    private static ReadOnlySpan<char> FindFirstOrEnd(ReadOnlySpan<char> buffer, ReadOnlySpan<char> markers)
    {
        while (!buffer.IsEmpty)
        {
            var indexOfChar = markers.IndexOf(buffer[0]);
            buffer = buffer[1..];
            if (indexOfChar != -1)
                return buffer;
        }

        return "";
    }
}
