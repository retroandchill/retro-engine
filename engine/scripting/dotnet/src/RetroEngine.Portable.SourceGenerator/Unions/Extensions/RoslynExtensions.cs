// // @file RoslynExtensions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace RetroEngine.Portable.SourceGenerator.Unions.Extensions;

internal static class RoslynExtensions
{
    public static string ToCodeString(this Accessibility accessibility) =>
        accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            Accessibility.Protected => "protected",
            Accessibility.ProtectedAndInternal => "protected internal",
            Accessibility.Private => "private",
            _ => throw new ArgumentOutOfRangeException(nameof(accessibility), "Invalid type accessibility"),
        };
}
