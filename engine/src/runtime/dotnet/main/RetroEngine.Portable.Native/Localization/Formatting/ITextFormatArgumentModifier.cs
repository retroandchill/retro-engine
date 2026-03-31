// // @file ITextFormatArgumentModifier.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Text;
using RetroEngine.Portable.Collections.Immutable;
using ZParse;
using ZParse.Parsers;

namespace RetroEngine.Portable.Localization.Formatting;

public interface ITextFormatArgumentModifier
{
    (bool UsesFormatArgs, int Length) EstimateLength();

    IEnumerable<string> FormatArgumentNames { get; }

    void Evaluate<TContext>(in FormatArg arg, in TContext context, StringBuilder builder)
        where TContext : ITextFormatContext, allows ref struct;

    protected static ParseResult<ImmutableOrderedDictionary<string, string>> ParseKeyValueArgs(ParseCursor cursor)
    {
        var builder = ImmutableOrderedDictionary.CreateBuilder<string, string>();
        var remainder = cursor.ParseOptionalWhitespace().Remainder;
        while (!remainder.IsAtEnd)
        {
            var key = remainder.ParseIdentifier();
            if (!key.HasValue)
                return ParseResult.Empty<ImmutableOrderedDictionary<string, string>>(cursor);

            remainder = key.Remainder.ParseOptionalWhitespace().Remainder;

            var equals = remainder.ParseChar('=');
            if (!equals.HasValue)
                return ParseResult.Empty<ImmutableOrderedDictionary<string, string>>(cursor);

            remainder = equals.Remainder.ParseOptionalWhitespace().Remainder;

            var quotedString = remainder.ParseQuotedString();
            if (quotedString.HasValue)
            {
                remainder = quotedString.Remainder.ParseOptionalWhitespace().Remainder;
                builder.Add(key.Value.ToString(), quotedString.Value);
            }
            else
            {
                var arg = remainder.ParseUntilChar(',');
                if (arg.Value.Length == 0)
                    return ParseResult.Empty<ImmutableOrderedDictionary<string, string>>(cursor, remainder);

                remainder = arg.Remainder.ParseOptionalWhitespace().Remainder;
                builder.Add(key.Value.ToString(), arg.Value.ToString());
            }

            if (remainder.IsAtEnd)
                break;

            var comma = remainder.ParseChar(',');
            if (!comma.HasValue)
                return ParseResult.Empty<ImmutableOrderedDictionary<string, string>>(cursor, remainder);

            remainder = comma.Remainder;
        }

        return ParseResult.Success(builder.ToImmutable(), cursor, remainder);
    }

    protected static ParseResult<ImmutableArray<string>> ParseStringArray(string argsString)
    {
        throw new NotImplementedException();
    }
}
