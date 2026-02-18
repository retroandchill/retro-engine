// // @file TextFormatDefinition.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace RetroEngine.Portable.Localization.Formatting;

public sealed class TextFormatDefinition
{
    public char EscapeChar { get; }
    public char ArgStartChar { get; }
    public char ArgEndChar { get; }
    public char ArgModChar { get; }

    public TextParser<IReadOnlyList<FormatSegment>> Format { get; }

    public static TextFormatDefinition Default { get; } = new();

    public TextFormatDefinition(
        char escapeChar = '`',
        char argStartChar = '{',
        char argEndChar = '}',
        char argModChar = '|'
    )
    {
        EscapeChar = escapeChar;
        ArgStartChar = argStartChar;
        ArgEndChar = argEndChar;
        ArgModChar = argModChar;

        var escapedChar = Character
            .EqualTo(EscapeChar)
            .SelectMany(_ => Character.In(EscapeChar, ArgStartChar, ArgEndChar, ArgModChar), (_, ch) => ch);
        var placeholderSegment = PlaceholderWithOptionalModifier(
            ArgStartChar,
            ArgEndChar,
            ArgModChar,
            escapedChar,
            EscapeChar
        );
        var literalChar = escapedChar.Or(
            Character.Except(c => c != ArgStartChar && c != EscapeChar, "literal character")
        );
        var literalSegment = literalChar
            .AtLeastOnce()
            .Select(chars => new string(chars.ToArray()))
            .Select(FormatSegment (text) => new LiteralSegment(text));
        var segment = placeholderSegment.Or(literalSegment);
        Format = segment.Many().Select(IReadOnlyList<FormatSegment> (segments) => segments);
    }

    private static readonly TextParser<string> Identifier = Character.Letter.SelectMany(
        _ => Character.LetterOrDigit.Or(Character.EqualTo('_')).Many(),
        (first, rest) => new string([first, .. rest])
    );

    private static readonly TextParser<Unit> Whitespace = Character.WhiteSpace.Many().Value(Unit.Value);

    private static readonly TextParser<string> QuotedValue = Character
        .EqualTo('"')
        .SelectMany(
            _ => Character.EqualTo('\\').IgnoreThen(Character.AnyChar).Or(Character.Except('"')).Many(),
            (open, content) => new { open, content }
        )
        .SelectMany(_ => Character.EqualTo('"'), (t, _) => new string(t.content.ToArray()));

    private static TextParser<string> BareValue =>
        Character
            .Matching(c => c != ',' && c != ')', "argument value character")
            .AtLeastOnce()
            .Select(cs => new string(cs).Trim())
            .Where(s => s.Length > 0);

    private static TextParser<string> ArgValue =>
        QuotedValue.Between(Whitespace, Whitespace).Try().Or(BareValue.Between(Whitespace, Whitespace));

    private static readonly TextParser<Unit> Comma = Character
        .EqualTo(',')
        .Between(Whitespace, Whitespace)
        .Value(Unit.Value);

    private static TextParser<IReadOnlyList<T>> ParenList<T>(TextParser<T> element)
    {
        return from l in Character.EqualTo('(').Between(Whitespace, Whitespace)
            from items in element.ManyDelimitedBy(Comma).OptionalOrDefault(Array.Empty<T>())
            from r in Character.EqualTo(')').Between(Whitespace, Whitespace)
            select (IReadOnlyList<T>)items.ToArray();
    }

    private static readonly TextParser<KeyValuePair<string, string>> NamedArg = Identifier
        .Between(Whitespace, Whitespace)
        .SelectMany(_ => Character.EqualTo('=').Between(Whitespace, Whitespace), (key, eq) => new { key, eq })
        .SelectMany(_ => ArgValue, (t, val) => new KeyValuePair<string, string>(t.key, val));

    private static TextParser<string> PositionalArg => ArgValue;

    private static readonly TextParser<ModifierArgs> ModifierArgsParser = ParenList(NamedArg)
        .Select(ModifierArgs (pairs) => new NamedArgs(pairs.ToDictionary(p => p.Key, p => p.Value)))
        .Try()
        .Or(ParenList(PositionalArg).Select(ModifierArgs (values) => new PositionalArgs(values)));

    private static TextParser<ArgModifier> ArgModifierParser(char argModChar)
    {
        return Character
            .EqualTo(argModChar)
            .SelectMany(bar => Identifier.Between(Whitespace, Whitespace), (bar, name) => new { bar, name })
            .SelectMany(@t => ModifierArgsParser, (@t, args) => new ArgModifier(@t.name, args));
    }

    private static TextParser<string> PlaceholderKeyParser(
        char argStartChar,
        char argEndChar,
        TextParser<char> escapedChar,
        char escapeChar
    )
    {
        return Character
            .EqualTo(argStartChar)
            .SelectMany(
                _ =>
                    escapedChar
                        .Or(Character.Matching(c => c != argEndChar && c != escapeChar, "placeholder character"))
                        .Many(),
                (open, keyChars) => new { open, keyChars }
            )
            .SelectMany(_ => Character.EqualTo(argEndChar), (@t, _) => new string(t.keyChars.ToArray()));
    }

    private static TextParser<FormatSegment> PlaceholderWithOptionalModifier(
        char argStartChar,
        char argEndChar,
        char argModChar,
        TextParser<char> escapedChar,
        char escapeChar
    )
    {
        return PlaceholderKeyParser(argStartChar, argEndChar, escapedChar, escapeChar)
            .SelectMany(
                rawKey => ArgModifierParser(argModChar)!.OptionalOrDefault(),
                FormatSegment (rawKey, mod) => BuildPlaceholder(rawKey, mod)
            );
    }

    private static PlaceholderSegment BuildPlaceholder(string raw, ArgModifier? modifier)
    {
        var key = raw.Trim();
        return !string.IsNullOrEmpty(key)
            ? new PlaceholderSegment(key, modifier)
            : throw new FormatException("Invalid placeholder format.");
    }
}
