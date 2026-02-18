// // @file TextFormat.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Superpower;

namespace RetroEngine.Portable.Localization.Formatting;

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

    public TextFormat(Text text, TextFormatDefinition patternDefinition)
    {
        _sourceType = SourceType.Text;
        _sourceText = text;
        PatternDefinition = patternDefinition;
        Compile();
    }

    public TextFormat(string sourceString, TextFormatDefinition patternDefinition)
    {
        _sourceType = SourceType.String;
        _sourceExpression = sourceString;
        PatternDefinition = patternDefinition;
        Compile();
    }

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
            switch (token)
            {
                case LiteralSegment literalSegment:
                    _baseFormatStringLength += literalSegment.Text.Length;
                    break;
                case PlaceholderSegment placeholderSegment:
                    _expressionType = CompiledExpressionType.Complex;
                    if (placeholderSegment.Modifier is not null)
                    {
                        var (argModUsesFormatArgs, argModLength) = placeholderSegment.Modifier.EstimateLength();
                        _baseFormatStringLength += argModLength;
                        _formatArgumentEstimateMultiplier += argModUsesFormatArgs ? 1 : 0;
                    }
                    break;
            }
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
