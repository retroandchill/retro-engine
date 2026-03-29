// // @file TextFormat.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;
using LinkDotNet.StringBuilder;
using RetroEngine.Portable.Localization.Stringification;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace RetroEngine.Portable.Localization.Formatting;

[Flags]
public enum TextFormatFlags : byte
{
    None = 0,
    EvaluateArgumentModifiers = 1 << 0,
    Default = EvaluateArgumentModifiers,
}

public sealed class TextFormat
{
    private enum SourceType : byte
    {
        Text,
        String,
    }

    public enum CompiledExpressionType : byte
    {
        Invalid,
        Simple,
        Complex,
    }

    private readonly SourceType _sourceType;
    private readonly Text _sourceText;
    private string _sourceExpression = "";
    private readonly TextFormatFlags _formatFlags;
    private readonly Lock _compiledDataLock = new();
    private ImmutableArray<FormatSegment> _compiledSegments = [];
    private TextSnapshot _compiledTextSnapshot;
    private int _baseFormatStringLength;
    private int _formatArgumentEstimateMultiplier;
    private string? _lastErrorMessage;

    public Text SourceText => _sourceType == SourceType.Text ? _sourceText : new Text(_sourceExpression);
    public string SourceString => _sourceType == SourceType.String ? _sourceExpression : _sourceText.ToString();

    public TextFormatDefinition PatternDefinition { get; }

    private CompiledExpressionType _expressionType;
    public CompiledExpressionType ExpressionType
    {
        get
        {
            using var scope = _compiledDataLock.EnterScope();
            return _expressionType;
        }
    }

    public bool IsValid
    {
        get
        {
            using var scope = _compiledDataLock.EnterScope();
            return ExpressionType != CompiledExpressionType.Invalid;
        }
    }

    public IEnumerable<string> FormatArgumentNames
    {
        get
        {
            ImmutableArray<FormatSegment> segments;
            using (_compiledDataLock.EnterScope())
            {
                segments = _compiledSegments;
            }

            if (ExpressionType == CompiledExpressionType.Invalid)
                yield break;

            var nameSeen = new HashSet<string>();
            foreach (var token in segments)
            {
                if (!token.TryGetPlaceholderData(out var key, out var modifier))
                    continue;

                if (nameSeen.Add(key.Name))
                {
                    yield return key.Name;
                }

                if (modifier is null)
                    continue;

                foreach (var argName in modifier.FormatArgumentNames.Where(nameSeen.Add))
                {
                    yield return argName;
                }
            }
        }
    }

    private static readonly TokenListParser<TextFormatToken, FormatSegment> LiteralTokenParser = input =>
    {
        var next = input.ConsumeToken();
        if (!next.HasValue)
            return TokenListParserResult.Empty<TextFormatToken, FormatSegment>(input);

        using var builder = new ValueStringBuilder();
        var remaining = input;
        do
        {
            if (next.Value.Kind.TryGetStringLiteralData())
            {
                builder.Append(next.Value.Span.AsReadOnlySpan());
            }
            else if (next.Value.Kind.TryGetEscapeCharacterData(out var ch))
            {
                builder.Append(ch);
            }
            else
            {
                break;
            }

            remaining = next.Remainder;
            next = remaining.ConsumeToken();
        } while (next.HasValue);

        return remaining != input
            ? TokenListParserResult.Value(FormatSegment.Literal(builder.ToString()), input, remaining)
            : TokenListParserResult.Empty<TextFormatToken, FormatSegment>(input);
    };

    private static readonly TokenListParser<TextFormatToken, FormatSegment> ArgTokenParser = Parse
        .Sequence(
            Token
                .Matching((TextFormatToken t) => t.IsArgument, "argument token")
                .Select(t => t.Kind.TryGetArgumentData(out var arg) ? arg : throw new InvalidOperationException()),
            Token
                .Matching((TextFormatToken t) => t.IsArgumentModifier, "argument modifier token")
                .Select(t =>
                    t.Kind.TryGetArgumentModifierData(out var mod) ? mod : throw new InvalidOperationException()
                )
                .OptionalOrDefault()
        )
        .Select(t =>
        {
            var (argument, modifier) = t;
            return FormatSegment.Placeholder(new PlaceholderKey(argument.ToStringValue()), modifier ?? null);
        });

    private static readonly TokenListParser<TextFormatToken, FormatSegment[]> ValidFormatSegments = LiteralTokenParser
        .Or(ArgTokenParser)
        .Many()
        .AtEnd();

    public static TextFormat Empty { get; } = new(Text.Empty);

    public TextFormat(
        Text text,
        TextFormatDefinition? patternDefinition = null,
        TextFormatFlags formatFlags = TextFormatFlags.Default
    )
    {
        _sourceType = SourceType.Text;
        _sourceText = text;
        PatternDefinition = patternDefinition ?? TextFormatDefinition.Default;
        _formatFlags = formatFlags;
        Compile();
    }

    public TextFormat(
        string sourceString,
        TextFormatDefinition? patternDefinition = null,
        TextFormatFlags formatFlags = TextFormatFlags.Default
    )
    {
        _sourceType = SourceType.String;
        _sourceExpression = sourceString;
        PatternDefinition = patternDefinition ?? TextFormatDefinition.Default;
        _formatFlags = formatFlags;
        Compile();
    }

    public static implicit operator TextFormat(Text text) => new(text);

    public static implicit operator TextFormat(string sourceString) => new(sourceString);

    public bool IdenticalTo(TextFormat other, TextIdenticalModeFlags flags)
    {
        if (_sourceType != other._sourceType)
            return false;

        return _sourceType switch
        {
            SourceType.Text => _sourceText.IdenticalTo(other._sourceText, flags),
            SourceType.String => flags.HasFlag(TextIdenticalModeFlags.LexicalCompareInvariants)
                && string.Equals(_sourceExpression, other._sourceExpression, StringComparison.Ordinal),
            _ => throw new InvalidOperationException(),
        };
    }

    internal string Format<TContext>(in TContext context)
        where TContext : ITextFormatContext, allows ref struct
    {
        using var scope = _compiledDataLock.EnterScope();
        return FormatInternal(context);
    }

    private string FormatInternal<TContext>(in TContext context)
        where TContext : ITextFormatContext, allows ref struct
    {
        if (_sourceType == SourceType.Text && context.RebuildText)
        {
            _sourceText.Rebuild();
        }

        ConditionalCompile();

        if (_compiledSegments.Length == 0)
        {
            return _sourceExpression;
        }

        var resultBuilder = new StringBuilder(
            _baseFormatStringLength + context.EstimatedArgLength * _formatArgumentEstimateMultiplier
        );

        var argumentIndex = 0;
        foreach (var segment in _compiledSegments)
        {
            segment.Match(
                context,
                (_, str) => resultBuilder.Append(str),
                (ctx, key, mod) =>
                {
                    // ReSharper disable once AccessToModifiedClosure
                    var possibleArg = ctx.ResolveArg(key, argumentIndex);
                    if (possibleArg is not null)
                    {
                        if (mod is not null)
                        {
                            if (_formatFlags.HasFlag(TextFormatFlags.EvaluateArgumentModifiers))
                            {
                                mod.Evaluate(possibleArg.Value, in ctx, resultBuilder);
                            }
                            else
                            {
                                resultBuilder.Append(PatternDefinition.ArgModChar);
                                resultBuilder.Append(mod.ModifierPattern);
                            }
                        }
                        else
                        {
                            possibleArg.Value.ToFormattedString(ctx.RebuildText, ctx.RebuildAsSource, resultBuilder);
                        }
                    }
                    else
                    {
                        resultBuilder.Append(PatternDefinition.ArgStartChar);
                        resultBuilder.Append(key.Name);
                        resultBuilder.Append(PatternDefinition.ArgEndChar);
                    }
                }
            );
            argumentIndex++;
        }

        return resultBuilder.ToString();
    }

    private void Compile()
    {
        _compiledSegments = _compiledSegments.Clear();
        if (_sourceType == SourceType.Text)
        {
            _sourceExpression = _sourceText.ToString();
            _compiledTextSnapshot = new TextSnapshot(_sourceText);
        }

        _expressionType = CompiledExpressionType.Simple;
        _baseFormatStringLength = 0;
        _formatArgumentEstimateMultiplier = 1;
        var tokenList = PatternDefinition.Tokenizer.TryTokenize(_sourceExpression);
        if (!tokenList.HasValue)
        {
            _expressionType = CompiledExpressionType.Invalid;
            _lastErrorMessage = tokenList.FormatErrorMessageFragment();
            return;
        }

        if (tokenList.Value.IsAtEnd)
            return;

        var segments = ValidFormatSegments.TryParse(tokenList.Value);
        if (!segments.HasValue)
        {
            _expressionType = CompiledExpressionType.Invalid;
            _lastErrorMessage = segments.FormatErrorMessageFragment();
            return;
        }

        _compiledSegments = [.. segments.Value];

        foreach (var token in _compiledSegments)
        {
            token.Match(
                this,
                (self, str) => self._baseFormatStringLength += str.Length,
                (self, _, modifier) =>
                {
                    self._expressionType = CompiledExpressionType.Complex;
                    if (modifier is null)
                        return;

                    if (_formatFlags.HasFlag(TextFormatFlags.EvaluateArgumentModifiers))
                    {
                        var (argModUsesFormatArgs, argModLength) = modifier.EstimateLength();
                        self._baseFormatStringLength += argModLength;
                        self._formatArgumentEstimateMultiplier += argModUsesFormatArgs ? 1 : 0;
                    }
                    else
                    {
                        self._baseFormatStringLength += modifier.ModifierPattern.Length + 1;
                    }
                }
            );
        }
    }

    private void ConditionalCompile()
    {
        var requiresCompile = _sourceType == SourceType.Text && !_sourceText.IdenticalTo(Text.Empty);

        if (requiresCompile)
        {
            requiresCompile = false;
            if (!_compiledTextSnapshot.IsIdenticalTo(_sourceText))
            {
                if (!_compiledTextSnapshot.IsDisplayStringEqualTo(_sourceText))
                {
                    requiresCompile = true;
                }
                _compiledTextSnapshot = new TextSnapshot(_sourceText);
            }
        }

        if (requiresCompile)
        {
            Compile();
        }
    }
}
