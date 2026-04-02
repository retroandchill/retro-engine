// // @file TextFormatDefinition.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Utils;
using ZParse;
using ZParse.Parsers;

namespace RetroEngine.Portable.Localization.Formatting;

[Union]
internal readonly partial struct TextFormatToken
{
    [UnionCase]
    public static partial TextFormatToken StringLiteral();

    [UnionCase]
    public static partial TextFormatToken Argument(string name);

    [UnionCase]
    public static partial TextFormatToken ArgumentModifier(ITextFormatArgumentModifier? modifier);

    [UnionCase]
    public static partial TextFormatToken EscapeCharacter(char character);
}

public sealed class TextFormatDefinition
{
    public char EscapeChar { get; }
    public char ArgStartChar { get; }
    public char ArgEndChar { get; }
    public char ArgModChar { get; }

    internal TokenDefinitions<TextFormatToken> Definitions { get; }

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

        var parseArgument = Characters
            .EqualTo(ArgStartChar)
            .IgnoreThen(Sequences.UntilChar(ArgEndChar))
            .FollowedBy(Characters.EqualTo(ArgEndChar))
            .Select(x => TextFormatToken.Argument(x.ToString()));

        var argumentModifier = Characters
            .EqualTo(ArgModChar)
            .IgnoreThen(Symbols.Identifier)
            .Select(x => TextFormatter.Instance.FindArgumentModifier(x.ToString()))
            .NotNull()
            .Then(
                Characters.EqualTo('(').IgnoreThen(ParseArgumentModifierParameters).FollowedBy(Characters.EqualTo(')')),
                (getModifier, segment) => getModifier(segment)
            )
            .NotNull()
            .Select(TextFormatToken.ArgumentModifier);

        var parseEscapeCharacter = Characters
            .EqualTo(EscapeChar)
            .IgnoreThen(Characters.In(EscapeChar, ArgStartChar, ArgEndChar, ArgModChar))
            .Select(TextFormatToken.EscapeCharacter);

        var parseStringLiteral = Characters
            .AnyChar.IgnoreThen(Characters.Matching(c => !IsLiteralBreakCharacter(c), "string literal").IgnoreMany())
            .Value(TextFormatToken.StringLiteral());

        Definitions = new TokenDefinitions<TextFormatToken>([
            parseArgument,
            argumentModifier,
            parseEscapeCharacter,
            parseStringLiteral,
        ]);
    }

    private static ParseResult<TextSegment> ParseArgumentModifierParameters(TextSegment input)
    {
        var next = input.ConsumeChar();
        if (!next.HasValue)
            return ParseResult.CastEmpty<char, TextSegment>(next);

        var remainder = input;
        var quoteChar = '\0';
        var numConsecutiveSlashes = 0;
        do
        {
            var c = next.Value;
            if (c == ')' && quoteChar == '\0')
            {
                break;
            }

            remainder = next.Remainder;
            switch (c)
            {
                case '"' when c == quoteChar:
                {
                    if (numConsecutiveSlashes % 2 == 0)
                    {
                        quoteChar = '\0';
                    }

                    break;
                }
                case '"':
                    quoteChar = c;
                    break;
            }

            if (c == '\\')
            {
                ++numConsecutiveSlashes;
            }
            else
            {
                numConsecutiveSlashes = 0;
            }

            next = remainder.ConsumeChar();
        } while (next.HasValue);

        return ParseResult.Success(TextSegment.Between(input, remainder), input, remainder);
    }

    private bool IsLiteralBreakCharacter(char c) => c == EscapeChar || c == ArgStartChar;
}
