// // @file Friendly.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using LinkDotNet.StringBuilder;
using ZLinq;

namespace ZParse.Display;

internal static class Friendly
{
    extension(ref ValueStringBuilder builder)
    {
        public void AppendFriendlyList(IEnumerable<string> items)
        {
            ArgumentNullException.ThrowIfNull(items);

            // Keep the order stable
            var unique = items.AsValueEnumerable().Distinct().ToList();
            builder.AppendFriendlyListInternal(unique);
        }

        public void AppendFriendlyList(ImmutableArray<string> items)
        {
            builder.AppendFriendlyList(items.AsSpan());
        }

        [OverloadResolutionPriority(int.MaxValue)]
        public void AppendFriendlyList(scoped ReadOnlySpan<string> items)
        {
            // Keep the order stable
            var unique = items.AsValueEnumerable().Distinct().ToList();

            builder.AppendFriendlyListInternal(unique);
        }

        private void AppendFriendlyListInternal(List<string> items)
        {
            switch (items.Count)
            {
                case 0:
                    throw new ArgumentException(
                        "Friendly list formatting requires at least one element.",
                        nameof(items)
                    );
                case 1:
                    builder.Append(items.Single());
                    break;
                default:
                    for (var i = 0; i < items.Count - 1; i++)
                    {
                        builder.Append(items[i]);
                        builder.Append(", ");
                    }
                    builder.Append("or ");
                    builder.Append(items.Last());
                    break;
            }
        }
    }

    public static string List(IEnumerable<string> items)
    {
        var builder = new ValueStringBuilder();
        try
        {
            builder.AppendFriendlyList(items);
            return builder.ToString();
        }
        finally
        {
            builder.Dispose();
        }
    }
}
