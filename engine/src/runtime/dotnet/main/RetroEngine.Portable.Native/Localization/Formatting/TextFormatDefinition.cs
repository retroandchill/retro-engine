// // @file TextFormatDefinition.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Localization.Stringification;
using RetroEngine.Portable.Parsers;
using RetroEngine.Portable.Utils;
using Superpower.Tokenizers;
using ZParse;

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

    private TokenResult<TextFormatToken> ParseArgument(TokenCursor input)
    {
        var openingChar = input.ParseChar(ArgStartChar);
        if (!openingChar.HasValue)
            return TokenResult.CastEmpty<char, TextFormatToken>(openingChar);

        var identifier = openingChar.Remainder.ParseUntilChar(ArgEndChar);
        if (!identifier.HasValue)
            return TokenResult.CastEmpty<Unit, TextFormatToken>(identifier);

        var endChar = identifier.Remainder.ParseChar(ArgEndChar);
        if (!endChar.HasValue)
            return TokenResult.CastEmpty<char, TextFormatToken>(endChar);

        return TokenResult.Success(TextFormatToken.Argument(identifier.TokenText.ToString()), input, endChar.Remainder);
    }

    private TokenResult<TextFormatToken> ParseArgumentModifier(TokenCursor input)
    {
        var pipeToken = input.ParseChar(ArgModChar);
        if (!pipeToken.HasValue)
            return TokenResult.CastEmpty<char, TextFormatToken>(pipeToken);

        var identifier = pipeToken.Remainder.ParseIdentifier();
        if (!identifier.HasValue)
            return TokenResult.CastEmpty<Unit, TextFormatToken>(identifier);

        var openParen = identifier.Remainder.ParseChar('(');
        if (!openParen.HasValue)
            return TokenResult.CastEmpty<char, TextFormatToken>(openParen);

        var identifierName = identifier.TokenText.ToString();
        var compileTextArgumentModifier = TextFormatter.Instance.FindArgumentModifier(identifierName);
        if (compileTextArgumentModifier == null)
            return TokenResult.CastEmpty<char, TextFormatToken>(openParen);

        var parameterState = (QuoteChar: '\0', NumConsecutiveSlashes: 0);
        var parameters = ProcessArgumentModifierParameters(openParen.Remainder);

        if (!parameters.HasValue)
            return TokenResult.CastEmpty<Unit, TextFormatToken>(parameters);

        var createdItem = compileTextArgumentModifier(parameters.TokenText);
        if (createdItem.HasValue)
            return TokenResult.Success(
                TextFormatToken.ArgumentModifier(createdItem.Value),
                input,
                parameters.Remainder
            );

        var innerPosition = createdItem.Remainder.Position;
        var compositePosition = parameters.Cursor.Position + innerPosition;
        var newRemainder = new TokenCursor(parameters.Cursor.Input, compositePosition);
        return TokenResult.Empty<TextFormatToken>(input, newRemainder);
    }

    private static TokenResult<Unit> ProcessArgumentModifierParameters(TokenCursor input)
    {
        var next = input.Advance();
        if (!next.HasValue)
            return TokenResult.CastEmpty<char, Unit>(next);

        TokenCursor remainder;
        var quoteChar = '\0';
        var numConsecutiveSlashes = 0;
        do
        {
            remainder = next.Remainder;
            var c = next.Value;
            if (c == ')' && quoteChar == '\0')
            {
                break;
            }

            if (c == '"' && c == quoteChar)
            {
                if (numConsecutiveSlashes % 2 == 0)
                {
                    quoteChar = '\0';
                }
            }
            else if (c == '"')
            {
                quoteChar = c;
            }

            if (c == '\\')
            {
                ++numConsecutiveSlashes;
            }
            else
            {
                numConsecutiveSlashes = 0;
            }

            next = remainder.Advance();
        } while (next.HasValue);

        return TokenResult.Success(Unit.Value, input, remainder);
    }

    private TokenResult<TextFormatToken> ParseEscapeCharacter(TokenCursor input)
    {
        var next = input.ParseChar(EscapeChar);
        if (!next.HasValue)
            return TokenResult.CastEmpty<char, TextFormatToken>(next);

        var remainder = next.Remainder;
        next = remainder.ParseCharIn(EscapeChar, ArgStartChar, ArgEndChar, ArgModChar);
        return next.HasValue
            ? TokenResult.Success(TextFormatToken.EscapeCharacter(next.Value), input, remainder)
            : TokenResult.CastEmpty<char, TextFormatToken>(next);
    }

    private TokenResult<TextFormatToken> ParseStringLiteral(TokenCursor input)
    {
        var next = input.Advance();
        if (!next.HasValue)
            return TokenResult.CastEmpty<char, TextFormatToken>(next);

        TokenCursor remainder;
        do
        {
            remainder = next.Remainder;
            next = remainder.Advance();
        } while (next.HasValue && !IsLiteralBreakCharacter(next.Value));

        return TokenResult.Success(TextFormatToken.StringLiteral(), input, remainder);
    }

    private bool IsLiteralBreakCharacter(char c) => c == EscapeChar || c == ArgStartChar;
}
