// // @file ReferenceUtils.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Utilities;

public static class ReferenceUtils
{
    extension<T>(WeakReference<T> weakRef)
        where T : class
    {
        public T? Target => weakRef.TryGetTarget(out var target) ? target : null;
    }
}
