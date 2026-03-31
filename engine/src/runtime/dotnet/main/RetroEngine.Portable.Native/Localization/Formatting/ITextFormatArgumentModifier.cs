// // @file ITextFormatArgumentModifier.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;
using RetroEngine.Portable.Collections.Immutable;
using Superpower;
using ZParse;
using ZParse.Parsers;

namespace RetroEngine.Portable.Localization.Formatting;

public interface ITextFormatArgumentModifier
{
    (bool UsesFormatArgs, int Length) EstimateLength();

    IEnumerable<string> FormatArgumentNames { get; }

    void Evaluate<TContext>(in FormatArg arg, in TContext context, StringBuilder builder)
        where TContext : ITextFormatContext, allows ref struct;

    protected static TokenResult<ImmutableOrderedDictionary<string, string>> ParseKeyValueArgs(TokenCursor cursor)
    {
        var builder = ImmutableOrderedDictionary.CreateBuilder<string, string>();
        var remainder = cursor.ParseOptionalWhitespace().Remainder;
        while (!remainder.IsAtEnd)
        {
            var key = remainder.ParseIdentifier();
            if (!key.HasValue)
                return TokenResult.Empty<ImmutableOrderedDictionary<string, string>>(cursor);

            remainder = key.Remainder.ParseOptionalWhitespace().Remainder;

            var equals = remainder.ParseChar('=');
            if (!equals.HasValue)
                return TokenResult.Empty<ImmutableOrderedDictionary<string, string>>(cursor);

            remainder = equals.Remainder.ParseOptionalWhitespace().Remainder;

            var quotedString = remainder.ParseQuotedString();
            if (quotedString.HasValue)
            {
                remainder = quotedString.Remainder.ParseOptionalWhitespace().Remainder;
                builder.Add(key.TokenText.ToString(), quotedString.Value);
            }
            else
            {
                var arg = remainder.ParseUntilChar(',');
                if (arg.TokenText.Length == 0)
                    return TokenResult.Empty<ImmutableOrderedDictionary<string, string>>(cursor, remainder);

                remainder = arg.Remainder.ParseOptionalWhitespace().Remainder;
                builder.Add(key.TokenText.ToString(), arg.TokenText.ToString());
            }

            if (remainder.IsAtEnd)
                break;

            var comma = remainder.ParseChar(',');
            if (!comma.HasValue)
                return TokenResult.Empty<ImmutableOrderedDictionary<string, string>>(cursor, remainder);

            remainder = comma.Remainder;
        }

        return TokenResult.Success(builder.ToImmutable(), cursor, remainder);
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
