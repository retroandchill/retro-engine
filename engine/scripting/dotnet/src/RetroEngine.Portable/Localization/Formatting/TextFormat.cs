// // @file TextFormat.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using Superpower;

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
    private readonly List<FormatSegment> _compiledSegments = [];
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

        if (_compiledSegments.Count == 0)
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
        _compiledSegments.Clear();
        if (_sourceType == SourceType.Text)
        {
            _sourceExpression = _sourceText.ToString();
            _compiledTextSnapshot = new TextSnapshot(_sourceText);
        }

        _expressionType = CompiledExpressionType.Simple;
        _baseFormatStringLength = 0;
        _formatArgumentEstimateMultiplier = 1;
        var result = PatternDefinition.Format.TryParse(_sourceExpression);
        var validExpression = result.HasValue;
        if (!validExpression)
        {
            _expressionType = CompiledExpressionType.Invalid;
            _lastErrorMessage = result.ErrorMessage;
            return;
        }

        _compiledSegments.AddRange(result.Value);

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
