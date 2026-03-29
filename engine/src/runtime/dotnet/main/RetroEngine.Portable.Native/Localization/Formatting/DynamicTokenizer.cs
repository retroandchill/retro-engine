// // @file FormatTextTokenizer.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using RetroEngine.Portable.Utils;
using Superpower;
using Superpower.Model;

namespace RetroEngine.Portable.Localization.Formatting;

internal sealed class DynamicTokenizer<T> : Tokenizer<T>
{
    private readonly ImmutableArray<TextParser<T>> _parsers;

    public bool IgnoreWhitespace { get; init; }

    public DynamicTokenizer(params ImmutableArray<TextParser<T>> parsers)
    {
        if (parsers.IsEmpty)
            throw new ArgumentException("Must have at least one parser", nameof(parsers));
        _parsers = parsers;
    }

    protected override IEnumerable<Result<T>> Tokenize(TextSpan span)
    {
        var remainder = span;
        while (!remainder.IsAtEnd)
        {
            var next = remainder.ConsumeChar();

            if (IgnoreWhitespace)
            {
                while (next.HasValue && char.IsWhiteSpace(next.Value))
                {
                    next = next.Remainder.ConsumeChar();
                }
            }

            if (!next.HasValue)
                yield break;

            var emptyResult = Result.Empty<T>(remainder);
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            var resultFound = false;
            foreach (var parser in _parsers)
            {
                var result = parser(remainder);
                if (result.HasValue)
                {
                    yield return result;
                    remainder = result.Remainder;
                    resultFound = true;
                    break;
                }

                emptyResult = Result.CombineEmpty(emptyResult, result);
            }

            if (resultFound)
                continue;

            yield return emptyResult;
            yield break;
        }
    }
}
