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

    private static readonly TextParser<string> ArgumentValue = StringLiterals.QuotedString.Or(
        Characters.Except(',').AtLeastOnce(() => new StringBuilder(), (sb, c) => sb.Append(c), sb => sb.ToString())
    );

    private static readonly TextParser<KeyValuePair<string, string>> KeyValueArg = Sequences
        .OptionalWhitespace.IgnoreThen(Symbols.Identifier)
        .Then(
            Sequences
                .OptionalWhitespace.IgnoreThen(Characters.EqualTo('='))
                .IgnoreThen(Sequences.OptionalWhitespace)
                .IgnoreThen(ArgumentValue)
                .FollowedBy(Sequences.OptionalWhitespace),
            (k, v) => new KeyValuePair<string, string>(k.ToString(), v)
        );

    private static readonly TextParser<ImmutableOrderedDictionary<string, string>> KeyValueArgs =
        KeyValueArg.ManyDelimitedBy(
            Characters.EqualTo(','),
            () => ImmutableOrderedDictionary.CreateBuilder<string, string>(),
            (builder, arg) =>
            {
                builder.Add(arg.Key, arg.Value);
                return builder;
            },
            builder => builder.ToImmutable()
        );

    private static readonly TextParser<ImmutableArray<string>> StringArray = ArgumentValue.ManyDelimitedBy(
        Characters.EqualTo(','),
        ImmutableArray.CreateBuilder<string>,
        (builder, arg) =>
        {
            builder.Add(arg);
            return builder;
        },
        builder => builder.ToImmutable()
    );

    protected static ImmutableOrderedDictionary<string, string>? ParseKeyValueArgs(ReadOnlySpan<char> cursor)
    {
        var result = KeyValueArgs.TryParse(cursor);
        return result.HasValue ? result.Value : null;
    }

    protected static ImmutableArray<string>? ParseStringArray(ReadOnlySpan<char> argsString)
    {
        var result = StringArray.TryParse(argsString);
        return result.HasValue ? result.Value : null;
    }
}
