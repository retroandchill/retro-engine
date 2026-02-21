// // @file TextHistoryNamedFormat.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using RetroEngine.Portable.Collections.Immutable;
using RetroEngine.Portable.Localization.Formatting;

namespace RetroEngine.Portable.Localization.History;

internal sealed class TextHistoryNamedFormat : TextHistoryGenerated
{
    private readonly TextFormat _sourceFormat = TextFormat.Empty;
    private readonly ImmutableDictionary<string, FormatArg> _args = [];

    public TextHistoryNamedFormat() { }

    public TextHistoryNamedFormat(
        string displayString,
        TextFormat sourceFormat,
        ImmutableDictionary<string, FormatArg> args
    )
        : base(displayString)
    {
        _sourceFormat = sourceFormat;
        _args = args;
    }

    public TextHistoryNamedFormat(
        string displayString,
        TextFormat sourceFormat,
        IReadOnlyDictionary<string, FormatArg> args
    )
        : this(displayString, sourceFormat, args.ToImmutableDictionary()) { }

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

    public override IEnumerable<HistoricTextFormatData> GetHistoricFormatData(Text text)
    {
        foreach (var sourceHistoric in _sourceFormat.SourceText.HistoricFormatData)
        {
            yield return sourceHistoric;
        }

        foreach (var (_, value) in _args)
        {
            if (!value.TryGetTextData(out var textData))
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
