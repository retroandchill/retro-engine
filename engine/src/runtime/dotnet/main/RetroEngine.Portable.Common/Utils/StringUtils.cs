// // @file StringUtils.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using LinkDotNet.StringBuilder;

namespace RetroEngine.Portable.Utils;

public static class StringUtils
{
    extension(ReadOnlySpan<char> str)
    {
        public bool IsNumeric
        {
            get
            {
                if (str.IsEmpty)
                    return false;

                if (str[0] is '-' or '+')
                {
                    str = str[1..];
                }

                var hasDot = false;
                while (str.Length > 0)
                {
                    if (str[0] == '.')
                    {
                        if (hasDot)
                        {
                            return false;
                        }

                        hasDot = true;
                    }
                    else if (!char.IsDigit(str[0]))
                    {
                        return false;
                    }

                    str = str[1..];
                }

                return true;
            }
        }

        public int ParseOctal()
        {
            if (str.IsEmpty)
                throw new InvalidOperationException("Cannot parse empty string.");

            var output = 0;
            while (!str.IsEmpty)
            {
                var character = str[0];
                if (character is < '0' or > '7')
                {
                    throw new InvalidOperationException($"Invalid octal digit: {character}");
                }

                output = output * 8 + (character - '0');
                str = str[1..];
            }

            return output;
        }
    }

    public static string ReplaceQuotesWithEscapedQuotes(this string str)
    {
        var quoteCount = str.Count(c => c == '"');
        if (quoteCount == 0)
            return str;

        using var builder = new ValueStringBuilder(str.Length + quoteCount);

        var escaped = false;
        var remaining = str.AsSpan();
        while (!remaining.IsEmpty)
        {
            var c = remaining[0];
            if (escaped)
            {
                escaped = false;
            }
            else if (c == '\\')
            {
                escaped = true;
            }
            else if (c == '"')
            {
                builder.Append('\\');
            }

            builder.Append(c);
            remaining = remaining[1..];
        }

        return builder.ToString();
    }

    extension(char)
    {
        public static bool IsOctalDigit(char c)
        {
            return c - '0' < 8u;
        }
    }
}
