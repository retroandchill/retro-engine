// // @file TextTransformer.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Localization.Cultures;

namespace RetroEngine.Portable.Localization;

internal static class TextTransformer
{
    public static string ToUpper(string text)
    {
        return text.ToUpper(Culture.CurrentCulture.CultureInfo);
    }

    public static string ToLower(string text)
    {
        return text.ToLower(Culture.CurrentCulture.CultureInfo);
    }
}
