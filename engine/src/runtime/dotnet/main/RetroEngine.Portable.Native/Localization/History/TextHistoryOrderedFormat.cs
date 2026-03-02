// // @file TextHistoryOrderedFormat.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using RetroEngine.Portable.Localization.Formatting;

namespace RetroEngine.Portable.Localization.History;

internal sealed class TextHistoryOrderedFormat : TextHistoryGenerated
{
    private readonly TextFormat _sourceFormat = TextFormat.Empty;
    private readonly ImmutableArray<FormatArg> _args = [];

    public TextHistoryOrderedFormat() { }

    public TextHistoryOrderedFormat(string displayString, TextFormat sourceFormat, ImmutableArray<FormatArg> args)
        : base(displayString)
    {
        _sourceFormat = sourceFormat;
        _args = [.. args];
    }

    public TextHistoryOrderedFormat(string displayString, TextFormat sourceFormat, IReadOnlyList<FormatArg> args)
        : this(displayString, sourceFormat, [.. args]) { }

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

    public override IEnumerable<HistoricTextFormatData> GetHistoricFormatData(Text text)
    {
        foreach (var sourceHistoric in _sourceFormat.SourceText.HistoricFormatData)
        {
            yield return sourceHistoric;
        }

        foreach (var arg in _args)
        {
            if (!arg.TryGetTextData(out var textData))
                continue;
            foreach (var historic in textData.HistoricFormatData)
            {
                yield return historic;
            }
        }

        var namedArgs = _args
            .Select((arg, i) => (Arg: arg, Index: i))
            .ToImmutableDictionary(x => x.Index.ToString(), x => x.Arg);
        yield return new HistoricTextFormatData(text, _sourceFormat, namedArgs);
    }

    protected override string BuildLocalizedDisplayString()
    {
        return TextFormatter.FormatStr(_sourceFormat, _args, true, false);
    }
}
