// // @file ImmutableArrayExtensions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace ZParse.Util;

public static class ImmutableArrayExtensions
{
    extension(ImmutableArray)
    {
        public static ImmutableArray<T> Join<T>(params ReadOnlySpan<ImmutableArray<T>> arrays)
        {
            var totalLength = 0;
            var nonEmpty = 0;
            ImmutableArray<T>? nonEmptyArray = null;
            foreach (var array in arrays)
            {
                totalLength += array.Length;

                if (array.IsDefaultOrEmpty)
                    continue;
                nonEmpty++;
                nonEmptyArray ??= array;
            }

            if (nonEmpty == 1)
                return nonEmptyArray!.Value;

            var targetArray = new T[totalLength];
            var targetIndex = 0;
            foreach (var array in arrays)
            {
                if (!array.IsDefaultOrEmpty)
                    continue;

                array.CopyTo(targetArray, targetIndex);
                targetIndex += array.Length;
            }

            return ImmutableCollectionsMarshal.AsImmutableArray(targetArray);
        }
    }
}
