// // @file TextFormatDefinition.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Localization.Stringification;
using RetroEngine.Portable.Parsers;
using RetroEngine.Portable.Utils;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace RetroEngine.Portable.Localization.Formatting;

[Union]
internal readonly partial struct TextFormatToken
{
    [UnionCase]
    public static partial TextFormatToken StringLiteral();

    [UnionCase]
    public static partial TextFormatToken Argument(TextSpan name);

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

    internal Tokenizer<TextFormatToken> Tokenizer { get; }

    private static readonly TextParser<TextSpan> ParametersList = input =>
    {
        var next = input.ConsumeChar();
        if (!next.HasValue)
            return Result.Empty<TextSpan>(input);

        var quoteChar = '\0';
        var numConsecutiveSlashes = 0;

        var remainder = input;
        do
        {
            var c = next.Value;
            if (c == ')' && quoteChar == '\0')
                break;

            remainder = next.Remainder;
            if (c == '"')
            {
                if (c == quoteChar)
                {
                    if (numConsecutiveSlashes % 2 == 0)
                    {
                        quoteChar = '\0';
                    }
                }
                else
                {
                    quoteChar = c;
                }
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

        return Result.Value(input.Until(remainder), input, remainder);
    };

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

        var notArgEnd = Character.Except(ArgEndChar);

        var parseArgument = Span.MatchedBy(notArgEnd.IgnoreThen(notArgEnd.IgnoreMany()))
            .Between(Character.EqualTo(ArgStartChar), Character.EqualTo(ArgEndChar))
            .Select(TextFormatToken.Argument);

        var argumentIdentifier = Character
            .EqualTo(ArgModChar)
            .IgnoreThen(Span.MatchedBy(TextParsers.AlphaNumeric.IgnoreThen(TextParsers.AlphaNumeric.IgnoreMany())))
            .Text();

        var textParameters = ParametersList.Between(Character.EqualTo('('), Character.EqualTo(')'));

        TextParser<TextFormatToken> argumentModifier = input =>
        {
            var identifierResult = argumentIdentifier(input);
            if (!identifierResult.HasValue)
                return Result.CastEmpty<string, TextFormatToken>(identifierResult);

            var remainder = identifierResult.Remainder;
            var identifier = identifierResult.Value;

            var getterDelegate = TextFormatter.Instance.FindArgumentModifier(identifier);
            if (getterDelegate is null)
                return Result.Empty<TextFormatToken>(input);

            var argsList = textParameters(remainder);
            if (!argsList.HasValue)
                return Result.CastEmpty<TextSpan, TextFormatToken>(argsList);

            var args = argsList.Value;
            var result = getterDelegate(input, args);
            return result.HasValue
                ? Result.Value(TextFormatToken.ArgumentModifier(result.Value), input, result.Remainder)
                : Result.CastEmpty<ITextFormatArgumentModifier, TextFormatToken>(result);
        };

        var escapeCharacter = Character
            .EqualTo(EscapeChar)
            .IgnoreThen(Character.In(EscapeChar, ArgStartChar, ArgEndChar, ArgModChar))
            .Select(TextFormatToken.EscapeCharacter);

        var literal = Character
            .AnyChar.IgnoreThen(Character.ExceptIn(EscapeChar, ArgStartChar).IgnoreMany())
            .Value(TextFormatToken.StringLiteral());

        Tokenizer = new DynamicTokenizer<TextFormatToken>(parseArgument, argumentModifier, escapeCharacter, literal);
    }
}
