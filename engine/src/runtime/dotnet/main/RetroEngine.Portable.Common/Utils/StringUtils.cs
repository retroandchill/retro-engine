// // @file StringUtils.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using LinkDotNet.StringBuilder;

namespace RetroEngine.Portable.Utils;

public static class StringUtils
{
    public static string ReplaceCharWithEscapedChar(this string str)
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
}
