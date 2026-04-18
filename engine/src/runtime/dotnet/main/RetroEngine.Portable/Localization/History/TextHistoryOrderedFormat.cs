// // @file TextHistoryOrderedFormat.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;
using MagicArchive;
using RetroEngine.Portable.Localization.Formatting;
using RetroEngine.Portable.Localization.Stringification;
using ZParse;

namespace RetroEngine.Portable.Localization.History;

[Archivable]
internal sealed partial class TextHistoryOrderedFormat : TextHistoryGenerated, ITextHistory
{
    [ArchiveInclude]
    private readonly TextFormat _sourceFormat;

    [ArchiveInclude]
    private readonly ImmutableArray<FormatArg> _args;

    public TextHistoryOrderedFormat(TextFormat sourceFormat, IReadOnlyList<FormatArg> args)
        : this(sourceFormat, args.ToImmutableArray()) { }

    [ArchivableConstructor]
    public TextHistoryOrderedFormat(TextFormat sourceFormat, ImmutableArray<FormatArg> args)
    {
        _sourceFormat = sourceFormat;
        _args = args;
        UpdateDisplayString();
    }

    public TextHistoryOrderedFormat(TextFormat sourceFormat, ReadOnlySpan<FormatArg> args)
        : this(sourceFormat, args.ToImmutableArray()) { }

    public override string BuildInvariantDisplayString()
    {
        return TextFormatter.FormatStr(_sourceFormat, _args, true, true);
    }

    public override bool IdenticalTo(TextHistory other, TextIdenticalModeFlags flags)
    {
        return other is TextHistoryOrderedFormat otherOrderedFormat
            && _sourceFormat.IdenticalTo(otherOrderedFormat._sourceFormat, flags)
            && _args.SequenceEqual(otherOrderedFormat._args, (a, b) => a.IdenticalTo(b, flags));
    }

    private static readonly TextParser<ITextData> Parser = TextStringReader.Marked(
        Markers.LocGenFormatOrdered,
        TextStringReader.QuotedText.Then(
            TextStringReader
                .CommaSeparator.IgnoreThen(FormatArg.Parser)
                .Many(
                    ImmutableArray.CreateBuilder<FormatArg>,
                    (b, a) =>
                    {
                        b.Add(a);
                        return b;
                    },
                    b => b.ToImmutableArray()
                ),
            ITextData (t, a) => new TextHistoryOrderedFormat(t, a)
        )
    );

    public static ParseResult<ITextData> ImportFromString(TextSegment input, string? textNamespace)
    {
        return Parser(input);
    }

    public override bool ExportToString(StringBuilder buffer)
    {
        buffer.WriteTextFormat(
            Markers.LocGenFormatOrdered,
            _sourceFormat,
            _args.Select(a => new KeyValuePair<string?, FormatArg>(null, a))
        );
        return true;
    }

    public override IEnumerable<HistoricTextFormatData> GetHistoricFormatData(Text text)
    {
        foreach (var sourceHistoric in _sourceFormat.SourceText.HistoricFormatData)
        {
            yield return sourceHistoric;
        }

        foreach (var arg in _args)
        {
            if (!arg.TryGetValue(out Text textData))
                continue;
            foreach (var historic in textData.HistoricFormatData)
            {
                yield return historic;
            }
        }

        var namedArgs = _args
            .Select((arg, i) => (Arg: arg, Index: i))
            .ToImmutableSortedDictionary(x => x.Index.ToString(), x => x.Arg);
        yield return new HistoricTextFormatData(text, _sourceFormat, namedArgs);
    }

    protected override string BuildLocalizedDisplayString()
    {
        return TextFormatter.FormatStr(_sourceFormat, _args, true, false);
    }
}
