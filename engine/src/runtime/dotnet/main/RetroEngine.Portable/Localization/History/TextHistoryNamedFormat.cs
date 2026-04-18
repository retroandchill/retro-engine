// // @file TextHistoryNamedFormat.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;
using MagicArchive;
using RetroEngine.Portable.Localization.Formatting;
using RetroEngine.Portable.Localization.Stringification;
using ZLinq;
using ZParse;

namespace RetroEngine.Portable.Localization.History;

[Archivable]
internal sealed partial class TextHistoryNamedFormat : TextHistoryGenerated, ITextHistory
{
    [ArchiveInclude]
    private readonly TextFormat _sourceFormat;

    [ArchiveInclude]
    private readonly ImmutableSortedDictionary<string, FormatArg> _args;

    [ArchivableConstructor]
    public TextHistoryNamedFormat(TextFormat sourceFormat, ImmutableSortedDictionary<string, FormatArg> args)
    {
        _sourceFormat = sourceFormat;
        _args = args;
        UpdateDisplayString();
    }

    public TextHistoryNamedFormat(TextFormat sourceFormat, IReadOnlyDictionary<string, FormatArg> args)
        : this(sourceFormat, args.ToImmutableSortedDictionary()) { }

    public TextHistoryNamedFormat(TextFormat sourceFormat, ReadOnlySpan<(string Name, FormatArg Value)> args)
        : this(sourceFormat, args.AsValueEnumerable().ToImmutableSortedDictionary(x => x.Name, x => x.Value)) { }

    public override string BuildInvariantDisplayString()
    {
        return TextFormatter.FormatStr(_sourceFormat, _args, true, true);
    }

    public override bool IdenticalTo(TextHistory other, TextIdenticalModeFlags flags)
    {
        if (other is not TextHistoryNamedFormat otherNamedFormat)
            return false;

        if (!_sourceFormat.IdenticalTo(otherNamedFormat._sourceFormat, flags))
        {
            return false;
        }

        if (_args.Count != otherNamedFormat._args.Count)
            return false;

        foreach (var (key, value) in _args)
        {
            if (otherNamedFormat._args.TryGetValue(key, out var otherValue) && value.IdenticalTo(otherValue, flags))
                continue;

            return false;
        }

        return true;
    }

    private static readonly TextParser<ITextData> Parser = TextStringReader.Marked(
        Markers.LocGenFormatNamed,
        TextStringReader.QuotedText.Then(
            TextStringReader
                .CommaSeparator.IgnoreThen(TextStringReader.TextLiteral)
                .Then(TextStringReader.CommaSeparator.IgnoreThen(FormatArg.Parser), (t, a) => (Key: t, Value: a))
                .Many(
                    ImmutableSortedDictionary.CreateBuilder<string, FormatArg>,
                    (b, a) =>
                    {
                        b.Add(a.Key, a.Value);
                        return b;
                    },
                    b => b.ToImmutable()
                ),
            ITextData (t, a) => new TextHistoryNamedFormat(t, a)
        )
    );

    public static ParseResult<ITextData> ImportFromString(TextSegment input, string? textNamespace)
    {
        return Parser(input);
    }

    public override bool ExportToString(StringBuilder buffer)
    {
        buffer.WriteTextFormat(Markers.LocGenFormatNamed, _sourceFormat, _args!);
        return true;
    }

    public override IEnumerable<HistoricTextFormatData> GetHistoricFormatData(Text text)
    {
        foreach (var sourceHistoric in _sourceFormat.SourceText.HistoricFormatData)
        {
            yield return sourceHistoric;
        }

        foreach (var (_, value) in _args)
        {
            if (!value.TryGetValue(out Text textData))
                continue;
            foreach (var historic in textData.HistoricFormatData)
            {
                yield return historic;
            }
        }

        yield return new HistoricTextFormatData(text, _sourceFormat, _args);
    }

    protected override string BuildLocalizedDisplayString()
    {
        return TextFormatter.FormatStr(_sourceFormat, _args, true, false);
    }
}
