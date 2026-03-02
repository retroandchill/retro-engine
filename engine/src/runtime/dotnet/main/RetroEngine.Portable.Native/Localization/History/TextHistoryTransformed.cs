// // @file TextHistoryTransformed.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization.History;

internal sealed class TextHistoryTransformed : TextHistoryGenerated
{
    public enum TransformType
    {
        ToUpper,
        ToLower,
    }

    private readonly Text _sourceText;
    private readonly TransformType _transformType;

    public TextHistoryTransformed()
    {
        _sourceText = Text.Empty;
        _transformType = TransformType.ToUpper;
    }

    public TextHistoryTransformed(string displayString, Text sourceText, TransformType transformType)
        : base(displayString)
    {
        _sourceText = sourceText;
        _transformType = transformType;
    }

    public override string BuildInvariantDisplayString()
    {
        _sourceText.Rebuild();

        return _transformType switch
        {
            TransformType.ToUpper => _sourceText.BuildSourceString().ToUpperInvariant(),
            TransformType.ToLower => _sourceText.BuildSourceString().ToLowerInvariant(),
            _ => throw new ArgumentOutOfRangeException(nameof(_transformType), _transformType, null),
        };
    }

    public override bool IdenticalTo(TextHistory other, TextIdenticalModeFlags flags)
    {
        return other is TextHistoryTransformed otherTransformed
            && _sourceText.IdenticalTo(otherTransformed._sourceText, flags)
            && _transformType == otherTransformed._transformType;
    }

    public override IEnumerable<HistoricTextFormatData> GetHistoricFormatData(Text text)
    {
        return _sourceText.HistoricFormatData;
    }

    public override HistoricTextNumericData? GetHistoricNumericData(Text text)
    {
        return _sourceText.HistoricNumericData;
    }

    protected override string BuildLocalizedDisplayString()
    {
        _sourceText.Rebuild();

        return _transformType switch
        {
            TransformType.ToUpper => TextTransformer.ToUpper(_sourceText.ToString()),
            TransformType.ToLower => TextTransformer.ToLower(_sourceText.ToString()),
            _ => throw new ArgumentOutOfRangeException(nameof(_transformType), _transformType, null),
        };
    }
}
