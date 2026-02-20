// // @file LocalizationUtilities.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization;

public static class LocalizationUtilities
{
    extension(char c)
    {
        public bool IsValidCurrencyCode => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
    }
}
