// // @file TextFormatParsingUtils.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace RetroEngine.Portable.Localization.Formatting;

internal static class TextFormatParsingUtils
{
    public static TextParser<Unit> Whitespace { get; } = Character.WhiteSpace.Many().Value(Unit.Value);

    // [A-Za-z][A-Za-z0-9_]*
    public static TextParser<string> Identifier { get; } =
        from first in Character.Letter
        from rest in Character.LetterOrDigit.Or(Character.EqualTo('_')).Many()
        select new string([first, .. rest]);

    public static TextParser<Unit> Comma { get; } =
        Character.EqualTo(',').Between(Whitespace, Whitespace).Value(Unit.Value);

    // Optional quotes:
    // - quoted supports commas
    // - bare reads until ',' or ')'
    public static TextParser<string> QuotedValue { get; } =
        Character
            .EqualTo('"')
            .SelectMany(
                _ => Character.EqualTo('\\').IgnoreThen(Character.AnyChar).Or(Character.Except('"')).Many(),
                (open, content) => new { open, content }
            )
            .SelectMany(_ => Character.EqualTo('"'), (t, _) => new string(t.content.ToArray()));

    public static TextParser<string> BareValue { get; } =
        Character
            .Matching(c => c != ',' && c != ')', "argument value character")
            .AtLeastOnce()
            .Select(cs => new string(cs.ToArray()).Trim())
            .Where(s => s.Length > 0);

    public static TextParser<string> ArgValue { get; } =
        QuotedValue.Between(Whitespace, Whitespace).Try().Or(BareValue.Between(Whitespace, Whitespace));

    public static TextParser<IReadOnlyList<T>> ParenList<T>(TextParser<T> element)
    {
        return Character
            .EqualTo('(')
            .Between(Whitespace, Whitespace)
            .SelectMany(
                _ => element.ManyDelimitedBy(Comma).OptionalOrDefault(Array.Empty<T>()),
                (l, items) => new { l, items }
            )
            .SelectMany(
                _ => Character.EqualTo(')').Between(Whitespace, Whitespace),
                IReadOnlyList<T> (t, _) => @t.items.ToArray()
            );
    }
}
