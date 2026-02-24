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
        Character.Letter.SelectMany(
            _ => Character.LetterOrDigit.Or(Character.EqualTo('_')).Many(),
            (first, rest) => new string([first, .. rest])
        );

    public static TextParser<Unit> Comma { get; } =
        Character.EqualTo(',').Between(Whitespace, Whitespace).Value(Unit.Value);

    private static TextParser<Unit> EqualsSign { get; } =
        Character.EqualTo('=').Between(Whitespace, Whitespace).Value(Unit.Value);

    // Optional quotes:
    // - quoted supports commas
    // - bare reads until ',' or ')'
    private static TextParser<string> QuotedValue { get; } =
        Character
            .EqualTo('"')
            .SelectMany(
                _ => Character.EqualTo('\\').IgnoreThen(Character.AnyChar).Or(Character.Except('"')).Many(),
                (open, content) => new { open, content }
            )
            .SelectMany(_ => Character.EqualTo('"'), (t, _) => new string(t.content.ToArray()));

    private static TextParser<string> BareValue { get; } =
        Character
            .Matching(c => c != ',' && c != ')', "argument value character")
            .AtLeastOnce()
            .Select(cs => new string(cs.ToArray()).Trim())
            .Where(s => s.Length > 0);

    public static TextParser<string> ArgValue { get; } =
        QuotedValue.Between(Whitespace, Whitespace).Try().Or(BareValue.Between(Whitespace, Whitespace));

    public static TextParser<KeyValuePair<string, string>> KeyValueArg { get; } =
        Identifier
            .Between(Whitespace, Whitespace)
            .SelectMany(_ => EqualsSign, (key, equal) => (key, equal))
            .SelectMany(_ => ArgValue, (t, value) => new KeyValuePair<string, string>(t.key, value));

    /// <summary>
    /// Reads the raw string between '(' and ')'. The closing ')' is only recognized when it is outside quotes.
    /// Inside a quoted segment, any characters are allowed (including ')').
    /// The returned string excludes the outer parentheses.
    /// </summary>
    public static TextParser<string> ParenRawString { get; } =
        Character
            .EqualTo('(')
            .Between(Whitespace, Whitespace)
            .SelectMany(_ => ParenRawContent, (_, content) => content)
            .SelectMany(_ => Whitespace.IgnoreThen(Character.EqualTo(')')), (t, _) => @t);

    private static TextParser<IEnumerable<char>> ParenRawUnquotedChar { get; } =
        Character
            .Except(')') // stop only on ')' when not in quotes
            .Select(c => (IEnumerable<char>)[c]);

    private static TextParser<IEnumerable<char>> ParenRawEscape { get; } =
        Character.EqualTo('\\').SelectMany(_ => Character.AnyChar, IEnumerable<char> (slash, c) => [slash, c]);

    private static TextParser<IEnumerable<char>> ParenRawNonQuoteChar { get; } =
        Character.Except('"').Select(c => (IEnumerable<char>)[c]);

    private static TextParser<IEnumerable<char>> ParenRawQuotedChar { get; } =
        ParenRawEscape.Try().Or(ParenRawNonQuoteChar);

    private static TextParser<IEnumerable<char>> ParenRawQuotedSegment { get; } =
        Character
            .EqualTo('"')
            .SelectMany(_ => ParenRawQuotedChar.Many(), (open, inner) => new { open, inner })
            .SelectMany(
                _ => Character.EqualTo('"'),
                (t, close) => new[] { @t.open }.Concat(@t.inner.SelectMany(x => x)).Concat([close])
            );

    private static TextParser<IEnumerable<char>> ParenRawPiece { get; } =
        ParenRawQuotedSegment.Try().Or(ParenRawUnquotedChar);

    private static TextParser<string> ParenRawContent { get; } =
        ParenRawPiece.Many().Select(pieces => new string(pieces.SelectMany(p => p).ToArray()));
}
