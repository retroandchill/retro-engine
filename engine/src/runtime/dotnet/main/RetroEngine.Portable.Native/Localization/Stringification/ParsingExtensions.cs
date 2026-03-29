// // @file ParsingExtensions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Superpower.Model;

namespace RetroEngine.Portable.Localization.Stringification;

internal static class ParsingExtensions
{
    public static ReadOnlySpan<char> AsReadOnlySpan(this TextSpan span)
    {
        return span.Source is not null
            ? span.Source.AsSpan(span.Position.Absolute, span.Length)
            : throw new InvalidOperationException("TextSpan must have a source to be converted to a ReadOnlySpan.");
    }
}
