// // @file CharacterExtensions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ZParse.Util;

public static class CharacterExtensions
{
    extension(char)
    {
        public static bool IsIdentifier(char c)
        {
            return char.IsLetterOrDigit(c) || c == '_';
        }

        public static bool IsOctalDigit(char c)
        {
            return c - '0' < 8u;
        }

        public static int ToOctalValue(char c)
        {
            return c - '0';
        }

        public static int ToHexValue(char c)
        {
            return c switch
            {
                >= '0' and <= '9' => c - '0',
                >= 'a' and <= 'f' => c - 'a' + 10,
                >= 'A' and <= 'F' => c - 'A' + 10,
                _ => throw new InvalidOperationException(),
            };
        }
    }
}
