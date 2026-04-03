// // @file Characters.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using ZLinq;
using ZParse.Display;
using ZParse.Util;

namespace ZParse.Parsers;

public static class Characters
{
    private static TextParser<char> Matching(Func<char, bool> predicate, ImmutableArray<ParseExpectation> expectations)
    {
        return input =>
        {
            var next = input.ConsumeChar();
            if (!next.HasValue || !predicate(next.Value))
                return ParseResult.Empty<char>(input, expectations);

            return next;
        };
    }

    public static TextParser<char> Matching(Func<char, bool> predicate, string name)
    {
        return Matching(predicate, [name]);
    }

    public static TextParser<char> Except(Func<char, bool> predicate, string description)
    {
        return Matching(c => !predicate(c), $"any character except {description}");
    }

    public static TextParser<char> EqualTo(char ch)
    {
        return Matching(parsed => parsed == ch, Presentation.FormatLiteral(ch));
    }

    public static TextParser<char> EqualToIgnoreCase(char ch)
    {
        return Matching(parsed => char.ToUpper(parsed) == char.ToUpperInvariant(ch), Presentation.FormatLiteral(ch));
    }

    public static TextParser<char> In(params ImmutableArray<char> chars)
    {
        return Matching(
            chars.Contains,
            chars
                .AsValueEnumerable()
                .Select(Presentation.FormatLiteral)
                .Select(x => new ParseExpectation(x))
                .ToImmutableArray()
        );
    }

    /// <summary>
    /// Parse a single character except <paramref name="ch"/>.
    /// </summary>
    public static TextParser<char> Except(char ch)
    {
        return Except(parsed => parsed == ch, Presentation.FormatLiteral(ch));
    }

    /// <summary>
    /// Parse any single character except those in <paramref name="chars"/>.
    /// </summary>
    public static TextParser<char> ExceptIn(params ImmutableArray<char> chars)
    {
        return Matching(
            c => !chars.Contains(c),
            $"any character except {Friendly.List(chars.AsValueEnumerable().Select(Presentation.FormatLiteral))}"
        );
    }

    /// <summary>
    /// Parse any character.
    /// </summary>
    public static TextParser<char> AnyChar { get; } = Matching(_ => true, "any character");

    /// <summary>
    /// Parse a whitespace character.
    /// </summary>
    public static TextParser<char> WhiteSpace { get; } = Matching(char.IsWhiteSpace, "whitespace");

    /// <summary>
    /// Parse a digit.
    /// </summary>
    public static TextParser<char> Digit { get; } = Matching(char.IsDigit, "digit");

    /// <summary>
    /// Parse a letter.
    /// </summary>
    public static TextParser<char> Letter { get; } = Matching(char.IsLetter, "letter");

    /// <summary>
    /// Parse a letter or digit.
    /// </summary>
    public static TextParser<char> LetterOrDigit { get; } = Matching(char.IsLetterOrDigit, ["letter", "digit"]);

    public static TextParser<char> LetterOrDigitOrUnderscore { get; } =
        Matching(c => char.IsLetterOrDigit(c) || c == '_', ["letter", "digit", "underscore"]);

    /// <summary>
    /// Parse a lowercase letter.
    /// </summary>
    public static TextParser<char> Lower { get; } = Matching(char.IsLower, "lowercase letter");

    /// <summary>
    /// Parse an uppercase letter.
    /// </summary>
    public static TextParser<char> Upper { get; } = Matching(char.IsUpper, "uppercase letter");

    /// <summary>
    /// Parse a numeric character.
    /// </summary>
    public static TextParser<char> Numeric { get; } = Matching(char.IsNumber, "numeric character");

    public static TextParser<char> OctalDigit { get; } = Matching(char.IsOctalDigit, "octal digit");

    /// <summary>
    /// Parse a hexadecimal digit (0-9, a-f, A-F).
    /// </summary>
    public static TextParser<char> HexDigit { get; } = Matching(char.IsHexDigit, "hex digit");
}
