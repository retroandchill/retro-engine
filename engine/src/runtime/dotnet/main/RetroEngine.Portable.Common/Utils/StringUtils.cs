// // @file StringUtils.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

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
        return str.Contains('\"', StringComparison.Ordinal) ? str.Replace("\"", "\\\"") : str;
    }

    extension(char)
    {
        public static bool IsOctalDigit(char c)
        {
            return c - '0' < 8u;
        }
    }
}
