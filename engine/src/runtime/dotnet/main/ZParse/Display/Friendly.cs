// // @file Friendly.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using LinkDotNet.StringBuilder;
using ZLinq;

namespace ZParse.Display;

internal static class Friendly
{
    public static string Pluralize(string noun, int count)
    {
        ArgumentNullException.ThrowIfNull(noun);
        return count == 1 ? noun : $"{noun}s";
    }

    public static string List<TEnumerator>(ValueEnumerable<TEnumerator, string> expectations)
        where TEnumerator : struct, IValueEnumerator<string>, allows ref struct
    {
        var builder = new ValueStringBuilder();
        try
        {
            builder.AppendFriendlyList(expectations);
            return builder.ToString();
        }
        finally
        {
            builder.Dispose();
        }
    }

    extension(ref ValueStringBuilder builder)
    {
        public void AppendFriendlyList(ReadOnlySpan<string> expectations)
        {
            builder.AppendFriendlyList(expectations.AsValueEnumerable());
        }

        private void AppendFriendlyList<TEnumerator>(ValueEnumerable<TEnumerator, string> expectations)
            where TEnumerator : struct, IValueEnumerator<string>, allows ref struct
        {
            var count = 0;
            string? previous = null;
            foreach (var str in expectations.Distinct())
            {
                if (previous is not null)
                {
                    builder.Append(previous);
                    builder.Append(", ");
                }

                previous = str;
                count++;
            }

            if (count > 1)
            {
                builder.Append("or ");
            }

            if (previous is not null)
            {
                builder.Append(previous);
            }
        }
    }
}
