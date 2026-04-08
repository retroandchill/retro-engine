// // @file TextFormatDefinition.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using RetroEngine.Utilities;
using ZParse;
using ZParse.Parsers;

namespace RetroEngine.Portable.Localization.Formatting;

internal readonly record struct StringLiteralToken;

internal readonly record struct ArgumentToken(string Name);

internal readonly record struct ArgumentModifierToken(ITextFormatArgumentModifier Modifier);

internal readonly record struct EscapeCharacterToken(char Character);

internal enum TextFormatTokenKind
{
    StringLiteral,
    Argument,
    ArgumentModifier,
    EscapeCharacter,
}

internal readonly struct TextFormatToken
{
    public TextFormatTokenKind Kind { get; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly object? _objectValue;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly char _escapeChar;

    public TextFormatToken(StringLiteralToken token)
    {
        Kind = TextFormatTokenKind.StringLiteral;
        _objectValue = null;
        _escapeChar = '\0';
    }

    public TextFormatToken(ArgumentToken token)
    {
        Kind = TextFormatTokenKind.Argument;
        _objectValue = token.Name;
        _escapeChar = '\0';
    }

    public TextFormatToken(ArgumentModifierToken token)
    {
        Kind = TextFormatTokenKind.ArgumentModifier;
        _objectValue = token.Modifier;
        _escapeChar = '\0';
    }

    public TextFormatToken(EscapeCharacterToken token)
    {
        Kind = TextFormatTokenKind.EscapeCharacter;
        _objectValue = null;
        _escapeChar = token.Character;
    }

    public bool TryGetValue(out StringLiteralToken value)
    {
        if (Kind == TextFormatTokenKind.StringLiteral)
        {
            value = default;
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue(out ArgumentToken value)
    {
        if (Kind == TextFormatTokenKind.Argument)
        {
            value = new ArgumentToken((string)_objectValue!);
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue(out ArgumentModifierToken value)
    {
        if (Kind == TextFormatTokenKind.ArgumentModifier)
        {
            value = new ArgumentModifierToken((ITextFormatArgumentModifier)_objectValue!);
            return true;
        }

        value = default;
        return false;
    }

    public bool TryGetValue(out EscapeCharacterToken value)
    {
        if (Kind == TextFormatTokenKind.EscapeCharacter)
        {
            value = new EscapeCharacterToken(_escapeChar);
            return true;
        }

        value = default;
        return false;
    }

    public static implicit operator TextFormatToken(StringLiteralToken token) => new(token);

    public static implicit operator TextFormatToken(ArgumentToken token) => new(token);

    public static implicit operator TextFormatToken(ArgumentModifierToken token) => new(token);

    public static implicit operator TextFormatToken(EscapeCharacterToken token) => new(token);
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
            .Select(x => new TextFormatToken(new ArgumentToken(x.ToString())));

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
            .Select(m => new TextFormatToken(new ArgumentModifierToken(m)));

        var parseEscapeCharacter = Characters
            .EqualTo(EscapeChar)
            .IgnoreThen(Characters.In(EscapeChar, ArgStartChar, ArgEndChar, ArgModChar))
            .Select(e => new TextFormatToken(new EscapeCharacterToken(e)));

        var parseStringLiteral = Characters
            .AnyChar.IgnoreThen(Characters.Matching(c => !IsLiteralBreakCharacter(c), "string literal").IgnoreMany())
            .Value(new TextFormatToken(new StringLiteralToken()));

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
