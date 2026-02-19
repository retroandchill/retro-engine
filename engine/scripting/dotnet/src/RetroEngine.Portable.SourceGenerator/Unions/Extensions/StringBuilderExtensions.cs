// // @file StringBuilderExtensions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;

namespace RetroEngine.Portable.SourceGenerator.Unions.Extensions;

public static class StringBuilderExtensions
{
    public static bool EndsWith(this StringBuilder stringBuilder, string value)
    {
        if (stringBuilder.Length < value.Length)
        {
            return false;
        }

        var sbIndex = stringBuilder.Length - value.Length;
        foreach (var ch in value)
        {
            if (stringBuilder[sbIndex] != ch)
            {
                return false;
            }

            sbIndex++;
        }

        return true;
    }
}
