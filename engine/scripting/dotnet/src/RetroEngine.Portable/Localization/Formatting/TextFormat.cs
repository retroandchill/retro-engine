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

    public Text SourceText => _sourceType == SourceType.Text ? _sourceText : new Text(_sourceExpression);
    public string SourceString => _sourceType == SourceType.String ? _sourceExpression : _sourceText.ToString();

    public TextFormatDefinition PatternDefinition { get; } = TextFormatDefinition.Default;

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
        if (validExpression)
        {
            _compiledSegments.AddRange(result.Value);

            for (var tokenIndex = 0; tokenIndex < _compiledSegments.Count; tokenIndex++)
            {
                var token = _compiledSegments[tokenIndex];

                switch (token)
                {
                    case LiteralSegment literalSegment:
                        _baseFormatStringLength += literalSegment.Text.Length;
                        break;
                    case PlaceholderSegment placeholderSegment:
                        _expressionType = CompiledExpressionType.Complex;

                        break;
                }
            }
        }
    }
}
