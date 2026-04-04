// // @file TextHistoryTransformed.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using RetroEngine.Portable.Localization.Cultures;
using RetroEngine.Portable.Localization.Stringification;
using ZParse;
using ZParse.Parsers;

namespace RetroEngine.Portable.Localization.History;

internal sealed class TextHistoryTransformed : TextHistoryGenerated, ITextHistory
{
    public enum TransformType
    {
        ToUpper,
        ToLower,
    }

    private readonly Text _sourceText;
    private readonly TransformType _transformType;

    public TextHistoryTransformed(Text sourceText, TransformType transformType)
    {
        _sourceText = sourceText;
        _transformType = transformType;
        UpdateDisplayString();
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

    private static readonly TextParser<ITextData> Parser = Sequences
        .EqualTo(Markers.LocGenToLower)
        .Value(TransformType.ToLower)
        .Or(Sequences.EqualTo(Markers.LocGenToUpper).Value(TransformType.ToUpper))
        .Then(
            TextStringReader
                .WhitespaceAndOpeningParen.IgnoreThen(TextStringReader.QuotedText)
                .FollowedBy(TextStringReader.WhitespaceAndClosingParen),
            ITextData (type, text) => new TextHistoryTransformed(text, type)
        );

    public static ParseResult<ITextData> ImportFromString(TextSegment input, string? textNamespace)
    {
        return Parser(input);
    }

    public override bool ExportToString(StringBuilder buffer)
    {
        switch (_transformType)
        {
            case TransformType.ToUpper:
                buffer.Append($"{Markers.LocGenToUpper}(");
                break;
            case TransformType.ToLower:
                buffer.Append($"{Markers.LocGenToLower}(");
                break;
            default:
                throw new InvalidOperationException("Invalid transform type.");
        }
        TextStringifier.ExportToString(buffer, _sourceText, true);
        buffer.Append(')');
        return true;
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
            TransformType.ToUpper => _sourceText.ToString().ToUpper(CultureManager.Instance.CurrentLocale.CultureInfo),
            TransformType.ToLower => _sourceText.ToString().ToLower(CultureManager.Instance.CurrentLocale.CultureInfo),
            _ => throw new ArgumentOutOfRangeException(nameof(_transformType), _transformType, null),
        };
    }
}
