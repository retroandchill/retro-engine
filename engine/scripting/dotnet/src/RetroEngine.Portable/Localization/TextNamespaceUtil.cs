// // @file TextNamespaceUtil.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization;

public static class TextNamespaceUtil
{
    private const char PackageNamespaceStartMarker = '[';
    private const char PackageNamespaceEndMarker = ']';

    public static ReadOnlySpan<char> StripPackageNamespace(ReadOnlySpan<char> textNamespace)
    {
        var endMarkerIndex = textNamespace.Length - 1;
        if (textNamespace.Length <= 0 || textNamespace[endMarkerIndex] != PackageNamespaceEndMarker)
            return textNamespace;
        var startMarkerIndex = textNamespace.IndexOf(PackageNamespaceStartMarker);
        return startMarkerIndex != -1
            ? textNamespace.Slice(startMarkerIndex, (endMarkerIndex - startMarkerIndex) + 1).TrimEnd()
            : textNamespace;
    }
}
