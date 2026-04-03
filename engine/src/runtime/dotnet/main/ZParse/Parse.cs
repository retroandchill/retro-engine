// // @file Parse.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ZParse;

public static class Parse
{
    public static TextParser<T> Return<T>(T value)
    {
        return input => ParseResult.Success(value, input, input);
    }
}
