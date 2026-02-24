// // @file TextFormatDefinition.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Superpower;
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
            EscapeChar,
            TextFormatter.Instance
        );
        var literalChar = escapedChar.Or(
            Character.Except(c => c == ArgStartChar || c == EscapeChar || c == ArgModChar, "literal character")
        );
        var literalSegment = literalChar
            .AtLeastOnce()
            .Select(chars => new string(chars.ToArray()))
            .Select(FormatSegment.Literal);
        var segment = placeholderSegment.Or(literalSegment);
        Format = segment.Many().Select(IReadOnlyList<FormatSegment> (segments) => segments);
    }

    private static GetTextArgumentModifier? RegisteredModifierParser(TextFormatter formatter, string keyword)
    {
        return formatter.FindArgumentModifier(keyword);
    }

    private static TextParser<ITextFormatArgumentModifier?> ArgModifierParser(
        string fullString,
        char argModChar,
        TextFormatter formatter
    )
    {
        return Character
            .EqualTo(argModChar)
            .SelectMany(
                _ =>
                    TextFormatParsingUtils.Identifier.Between(
                        TextFormatParsingUtils.Whitespace,
                        TextFormatParsingUtils.Whitespace
                    ),
                (bar, name) => (bar, name)
            )
            .SelectMany(_ => TextFormatParsingUtils.ParenRawString, (t, args) => (fullString, t.name, args))
            .Select(t => RegisteredModifierParser(formatter, t.name)?.Invoke(t.fullString, t.name, t.args));
    }

    private static TextParser<FormatSegment> PlaceholderWithOptionalModifier(
        char argStartChar,
        char argEndChar,
        char argModChar,
        TextParser<char> escapedChar,
        char escapeChar,
        TextFormatter formatter
    )
    {
        return PlaceholderKeyParser(argStartChar, argEndChar, escapedChar, escapeChar)
            .SelectMany(s => ArgModifierParser(s, argModChar, formatter).OptionalOrDefault(), BuildPlaceholder);
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

    private static FormatSegment BuildPlaceholder(string raw, ITextFormatArgumentModifier? modifier)
    {
        var key = raw.Trim();
        return !string.IsNullOrEmpty(key)
            ? FormatSegment.Placeholder(key, modifier)
            : throw new FormatException("Invalid placeholder format.");
    }
}
