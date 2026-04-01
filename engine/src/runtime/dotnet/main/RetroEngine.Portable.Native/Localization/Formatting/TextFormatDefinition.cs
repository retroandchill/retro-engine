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
        Definitions = new TokenDefinitions<TextFormatToken>([
            ParseArgument,
            ParseArgumentModifier,
            ParseEscapeCharacter,
            ParseStringLiteral,
        ]);
    }

    private ParseResult<TextFormatToken> ParseArgument(TextSegment input)
    {
        var openingChar = input.ParseChar(ArgStartChar);
        if (!openingChar.HasValue)
            return ParseResult.CastEmpty<char, TextFormatToken>(openingChar);

        var identifier = openingChar.Remainder.ParseUntilChar(ArgEndChar);
        if (!identifier.HasValue)
            return ParseResult.CastEmpty<TextSegment, TextFormatToken>(identifier);

        var endChar = identifier.Remainder.ParseChar(ArgEndChar);
        if (!endChar.HasValue)
            return ParseResult.CastEmpty<char, TextFormatToken>(endChar);

        return ParseResult.Success(TextFormatToken.Argument(identifier.Value.ToString()), input, endChar.Remainder);
    }

    private ParseResult<TextFormatToken> ParseArgumentModifier(TextSegment input)
    {
        var pipeToken = input.ParseChar(ArgModChar);
        if (!pipeToken.HasValue)
            return ParseResult.CastEmpty<char, TextFormatToken>(pipeToken);

        var identifier = pipeToken.Remainder.ParseIdentifier();
        if (!identifier.HasValue)
            return ParseResult.CastEmpty<TextSegment, TextFormatToken>(identifier);

        var openParen = identifier.Remainder.ParseChar('(');
        if (!openParen.HasValue)
            return ParseResult.CastEmpty<char, TextFormatToken>(openParen);

        var identifierName = identifier.Value.ToString();
        var compileTextArgumentModifier = TextFormatter.Instance.FindArgumentModifier(identifierName);
        if (compileTextArgumentModifier == null)
            return ParseResult.CastEmpty<char, TextFormatToken>(openParen);

        var parameters = ProcessArgumentModifierParameters(openParen.Remainder);

        if (!parameters.HasValue)
            return ParseResult.CastEmpty<TextSegment, TextFormatToken>(parameters);

        var closeParen = parameters.Remainder.ParseChar(')');
        if (!closeParen.HasValue)
            return ParseResult.CastEmpty<char, TextFormatToken>(closeParen);

        var createdItem = compileTextArgumentModifier(parameters.Value);
        if (createdItem.HasValue)
            return ParseResult.Success(
                TextFormatToken.ArgumentModifier(createdItem.Value),
                input,
                closeParen.Remainder
            );

        var innerPosition = createdItem.Remainder.Position;
        var newRemainder = parameters.Input + innerPosition;
        return ParseResult.Empty<TextFormatToken>(input, newRemainder);
    }

    private static ParseResult<TextSegment> ProcessArgumentModifierParameters(TextSegment input)
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

    private ParseResult<TextFormatToken> ParseEscapeCharacter(TextSegment input)
    {
        var next = input.ParseChar(EscapeChar);
        if (!next.HasValue)
            return ParseResult.CastEmpty<char, TextFormatToken>(next);

        var remainder = next.Remainder;
        next = remainder.ParseCharIn(EscapeChar, ArgStartChar, ArgEndChar, ArgModChar);
        return next.HasValue
            ? ParseResult.Success(TextFormatToken.EscapeCharacter(next.Value), input, next.Remainder)
            : ParseResult.CastEmpty<char, TextFormatToken>(next);
    }

    private ParseResult<TextFormatToken> ParseStringLiteral(TextSegment input)
    {
        var next = input.ConsumeChar();
        if (!next.HasValue)
            return ParseResult.CastEmpty<char, TextFormatToken>(next);

        TextSegment remainder;
        do
        {
            remainder = next.Remainder;
            next = remainder.ConsumeChar();
        } while (next.HasValue && !IsLiteralBreakCharacter(next.Value));

        return ParseResult.Success(TextFormatToken.StringLiteral(), input, remainder);
    }

    private bool IsLiteralBreakCharacter(char c) => c == EscapeChar || c == ArgStartChar;
}
