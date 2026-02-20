// // @file TextComparison.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Localization.Cultures;

namespace RetroEngine.Portable.Localization;

public static class TextComparison
{
    public static int Compare(ReadOnlySpan<char> left, ReadOnlySpan<char> right, TextComparisonLevel level)
    {
        var currentCulture = Culture.CurrentCulture;
        var collator = currentCulture.GetCollator(level);
        return (int)collator.Compare(left, right);
    }

    public static bool Equals(ReadOnlySpan<char> left, ReadOnlySpan<char> right, TextComparisonLevel level)
    {
        return Compare(left, right, level) == 0;
    }
}
