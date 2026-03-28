// // @file Character.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using ZParse.Display;

namespace ZParse.Parsers;

/// <summary>
/// Parsers for matching individual characters.
/// </summary>
public static class Character
{
    private static StringParser<char> Matching(Func<char, bool> predicate, ImmutableArray<string> expectations)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return input =>
        {
            var next = input.TryGetNext();
            if (!next.Success || !predicate(next.Value))
                return Result.Empty<char>(input, expectations);

            return next;
        };
    }

    /// <summary>
    /// Match a single character that satisfies the predicate.
    /// </summary>
    /// <param name="predicate">The boolean predicate to apply</param>
    /// <param name="name">The name for the expectation</param>
    /// <returns>The composite parser</returns>
    public static StringParser<char> Matching(Func<char, bool> predicate, string name)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(name);

        return Matching(predicate, [name]);
    }

    /// <summary>
    /// Match a single character that satisfies the predicate.
    /// </summary>
    /// <param name="predicate">The boolean predicate to apply</param>
    /// <param name="name">The name for the expectation</param>
    /// <returns>The composite parser</returns>
    public static StringParser<char> Except(Func<char, bool> predicate, string name)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(name);

        return Matching(c => !predicate(c), $"any character except {name}");
    }

    public static StringParser<char> EqualTo(char ch)
    {
        return Matching(parsed => parsed == ch, Presentation.FormatLiteral(ch));
    }

    public static StringParser<char> EqualToIgnoreCase(char ch)
    {
        return Matching(parsed => char.ToUpper(parsed) == char.ToUpper(ch), Presentation.FormatLiteral(ch));
    }

    public static StringParser<char> In(params ImmutableArray<char> chars)
    {
        return Matching(chars.Contains, [.. chars.Select(Presentation.FormatLiteral)]);
    }

    public static StringParser<char> Except(char ch)
    {
        return Except(parsed => parsed == ch, Presentation.FormatLiteral(ch));
    }

    public static StringParser<char> ExceptIn(params ImmutableArray<char> chars)
    {
        return Matching(
            c => !chars.Contains(c),
            $"any character except {Friendly.List(chars.Select(Presentation.FormatLiteral))}"
        );
    }

    /// <summary>
    /// Parse any character.
    /// </summary>
    public static StringParser<char> AnyChar { get; } = Matching(_ => true, "any character");

    /// <summary>
    /// Parse a whitespace character.
    /// </summary>
    public static StringParser<char> WhiteSpace { get; } = Matching(char.IsWhiteSpace, "whitespace");

    /// <summary>
    /// Parse a digit.
    /// </summary>
    public static StringParser<char> Digit { get; } = Matching(char.IsDigit, "digit");

    /// <summary>
    /// Parse a letter.
    /// </summary>
    public static StringParser<char> Letter { get; } = Matching(char.IsLetter, "letter");

    /// <summary>
    /// Parse a letter or digit.
    /// </summary>
    public static StringParser<char> LetterOrDigit { get; } = Matching(char.IsLetterOrDigit, ["letter", "digit"]);

    /// <summary>
    /// Parse a lowercase letter.
    /// </summary>
    public static StringParser<char> Lower { get; } = Matching(char.IsLower, "lowercase letter");

    /// <summary>
    /// Parse an uppercase letter.
    /// </summary>
    public static StringParser<char> Upper { get; } = Matching(char.IsUpper, "uppercase letter");

    /// <summary>
    /// Parse a numeric character.
    /// </summary>
    public static StringParser<char> Numeric { get; } = Matching(char.IsNumber, "numeric character");

    /// <summary>
    /// Parse a hexadecimal digit (0-9, a-f, A-F).
    /// </summary>
    public static StringParser<char> HexDigit { get; } = Matching(char.IsAsciiHexDigit, "hex digit");
}
