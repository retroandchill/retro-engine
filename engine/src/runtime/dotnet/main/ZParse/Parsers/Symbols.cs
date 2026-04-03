// // @file Symbols.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace ZParse.Parsers;

public static class Symbols
{
    public static TextParser<TextSegment> Identifier { get; } =
        Sequences.MatchedBy(Characters.LetterOrDigitOrUnderscore.IgnoreAtLeastOnce());

    public static TextParser<bool> Boolean { get; } =
        Sequences.EqualTo("true").Value(true).Or(Sequences.EqualTo("false").Value(false));

    public static TextParser<T> EnumLiteral<T>(string? prefix = null)
        where T : unmanaged, Enum
    {
        var parseEnumValue = Identifier.TrySelect((TextSegment s, out T r) => Enum.TryParse(s, out r));
        return prefix is null ? parseEnumValue : Sequences.EqualTo(prefix).IgnoreThen(parseEnumValue);
    }
}
