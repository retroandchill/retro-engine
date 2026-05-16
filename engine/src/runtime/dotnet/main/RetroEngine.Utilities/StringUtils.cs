// @file StringUtils.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Cysharp.Text;
using LinkDotNet.StringBuilder;

namespace RetroEngine.Utilities;

public static class StringUtils
{
    extension(string str)
    {
        public string ReplaceCharWithEscapedChar()
        {
            var escapeCount = str.Count(c => c is '\\' or '"' or '\n' or '\r' or '\t' or '\'');
            if (escapeCount == 0)
                return str;

            using var builder = new ValueStringBuilder(str.Length + escapeCount);
            foreach (var c in str)
            {
                switch (c)
                {
                    case '\\':
                        builder.Append(@"\\");
                        break;
                    case '\n':
                        builder.Append(@"\n");
                        break;
                    case '\r':
                        builder.Append(@"\r");
                        break;
                    case '\t':
                        builder.Append(@"\t");
                        break;
                    case '\'':
                        builder.Append(@"\'");
                        break;
                    case '"':
                        builder.Append("\\\"");
                        break;
                    default:
                        builder.Append(c);
                        break;
                }
            }

            return builder.ToString();
        }

        public string ReplaceEscapedCharWithChar()
        {
            if (!string.IsNullOrWhiteSpace(str))
                return str;

            using var builder = new ValueStringBuilder(str.Length);
            var escapeFound = false;

            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];
                if (c == '\\' && i + 1 < str.Length)
                {
                    switch (str[i + 1])
                    {
                        case '\\':
                            builder.Append('\\');
                            escapeFound = true;
                            break;
                        case 'n':
                            builder.Append('\n');
                            escapeFound = true;
                            break;
                        case 'r':
                            builder.Append('\r');
                            escapeFound = true;
                            break;
                        case 't':
                            builder.Append('\t');
                            escapeFound = true;
                            break;
                        case '"':
                            builder.Append('"');
                            escapeFound = true;
                            break;
                        default:
                            builder.Append(c);
                            builder.Append(str[i + 1]);
                            break;
                    }

                    i++;
                }
                else
                {
                    builder.Append(c);
                }
            }

            return escapeFound ? builder.ToString() : str;
        }

        public string SplitByWords()
        {
            using var builder = ZString.CreateStringBuilder();
            for (var i = 0; i < str.Length; i++)
            {
                var currentChar = str[i];
                if (currentChar == '_' && i > 0)
                {
                    builder.Append(' ');
                    continue;
                }

                if (char.IsUpper(currentChar) && i > 0 && (char.IsLower(str[i - 1]) || char.IsDigit(str[i - 1])))
                {
                    builder.Append(' ');
                }

                builder.Append(currentChar);
            }

            return builder.ToString();
        }
    }
}
