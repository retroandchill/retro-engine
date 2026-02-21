// // @file SearchExtensions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Utils;

public static class SearchExtensions
{
    public static TValue? FirstOrNull<TValue>(this IEnumerable<TValue> source)
        where TValue : struct
    {
        using var enumerator = source.GetEnumerator();
        return enumerator.MoveNext() ? enumerator.Current : null;
    }

    public static TValue? FirstOrNull<TValue>(this IEnumerable<TValue> source, Func<TValue, bool> predicate)
        where TValue : struct
    {
        foreach (var value in source)
        {
            if (predicate(value))
            {
                return value;
            }
        }

        return null;
    }
}
