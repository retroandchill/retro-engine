// // @file SearchExtensions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Utilities;

public static class SearchExtensions
{
    extension<TValue>(IEnumerable<TValue> source)
        where TValue : struct
    {
        public TValue? FirstOrNull()
        {
            using var enumerator = source.GetEnumerator();
            return enumerator.MoveNext() ? enumerator.Current : null;
        }

        public TValue? FirstOrNull(Func<TValue, bool> predicate)
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
}
