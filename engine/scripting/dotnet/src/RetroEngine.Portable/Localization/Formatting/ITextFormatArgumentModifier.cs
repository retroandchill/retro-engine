// // @file ITextFormatArgumentModifier.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;
using RetroEngine.Portable.Collections.Immutable;
using Superpower;

namespace RetroEngine.Portable.Localization.Formatting;

public interface ITextFormatArgumentModifier
{
    string ModifierPattern { get; }

    (bool UsesFormatArgs, int Length) EstimateLength();

    void Evaluate<TContext>(in FormatArg arg, in TContext context, StringBuilder builder)
        where TContext : ITextFormatContext, allows ref struct;

    protected static ImmutableOrderedDictionary<string, string>? ParseKeyValueArgs(string argsString)
    {
        var result = KeyValueArgsParser.TryParse(argsString);
        return result.HasValue ? result.Value : null;
    }

    protected static ImmutableArray<string>? ParseStringArray(string argsString)
    {
        var result = StringArrayParser.TryParse(argsString);
        return result.HasValue ? result.Value : null;
    }

    private static readonly TextParser<ImmutableOrderedDictionary<string, string>> KeyValueArgsParser =
        TextFormatParsingUtils
            .KeyValueArg.Between(TextFormatParsingUtils.Whitespace, TextFormatParsingUtils.Whitespace)
            .ManyDelimitedBy(TextFormatParsingUtils.Comma)
            .Select(kv => kv.ToImmutableOrderedDictionary());

    private static readonly TextParser<ImmutableArray<string>> StringArrayParser = TextFormatParsingUtils
        .ArgValue.ManyDelimitedBy(TextFormatParsingUtils.Comma)
        .Select(x => x.ToImmutableArray());
}
