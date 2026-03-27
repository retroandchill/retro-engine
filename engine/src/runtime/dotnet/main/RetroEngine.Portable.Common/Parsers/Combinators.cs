// // @file Combinators.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Superpower;
using Superpower.Model;

namespace RetroEngine.Portable.Parsers;

public static class Combinators
{
    public static TextParser<T> FollowedBy<T, TIgnored>(this TextParser<T> parser, TextParser<TIgnored> ignoredParser)
    {
        return input =>
        {
            var required = parser(input);
            if (!required.HasValue)
            {
                return required;
            }

            var ignored = ignoredParser(input);
            return ignored.HasValue ? required : Result.Empty<T>(input);
        };
    }
}
