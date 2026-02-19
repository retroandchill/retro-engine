// // @file TextFormatter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using Superpower;
using ZLinq;

namespace RetroEngine.Portable.Localization.Formatting;

internal delegate FormatArg? TextFormatArgResolver<in T>(T context, PlaceholderKey key, int argNumber)
    where T : allows ref struct;

public interface ITextFormatContext
{
    internal int EstimatedArgLength { get; }
    internal bool RebuildText { get; }
    internal bool RebuildAsSource { get; }
    internal FormatArg? ResolveArg(PlaceholderKey key, int argNumber);
}

internal static class TextFormatContext
{
    public static TextFormatContext<T> Create<T>(
        T context,
        TextFormatArgResolver<T> argResolver,
        int estimatedArgLength,
        bool rebuildText,
        bool rebuildAsSource
    )
        where T : allows ref struct
    {
        return new TextFormatContext<T>(context, argResolver, estimatedArgLength, rebuildText, rebuildAsSource);
    }
}

internal readonly ref struct TextFormatContext<T>(
    T context,
    TextFormatArgResolver<T> argResolver,
    int estimatedArgLength,
    bool rebuildText,
    bool rebuildAsSource
) : ITextFormatContext
    where T : allows ref struct
{
    private readonly T _context = context;
    public int EstimatedArgLength { get; } = estimatedArgLength;
    public bool RebuildText { get; } = rebuildText;
    public bool RebuildAsSource { get; } = rebuildAsSource;

    public FormatArg? ResolveArg(PlaceholderKey key, int argNumber) => argResolver(_context, key, argNumber);
}

public delegate ITextFormatArgumentModifier? GetTextArgumentModifier(string fullString, string name, string argsString);

public sealed class TextFormatter
{
    private readonly ConcurrentDictionary<string, GetTextArgumentModifier> _argumentModifiers = new();

    private TextFormatter()
    {
        _argumentModifiers.TryAdd(
            "plural",
            (f, _, a) => PluralFormatArgumentModifier.Create(f, TextPluralType.Cardinal, a)
        );
        _argumentModifiers.TryAdd(
            "ordinal",
            (f, _, a) => PluralFormatArgumentModifier.Create(f, TextPluralType.Ordinal, a)
        );
    }

    public static TextFormatter Instance { get; } = new();

    public void RegisterTextArgumentModifier(string keyword, GetTextArgumentModifier compileDelegate)
    {
        _argumentModifiers.TryAdd(keyword, compileDelegate);
    }

    public void UnregisterTextArgumentModifier(string keyword)
    {
        _argumentModifiers.Remove(keyword, out _);
    }

    public GetTextArgumentModifier? FindArgumentModifier(string keyword)
    {
        return _argumentModifiers.GetValueOrDefault(keyword);
    }

    public static Text Format(
        TextFormat format,
        IReadOnlyDictionary<string, FormatArg> arguments,
        bool rebuildText,
        bool rebuildAsSource
    )
    {
        throw new NotImplementedException();
    }

    public static Text Format(
        TextFormat format,
        IReadOnlyList<FormatArg> arguments,
        bool rebuildText,
        bool rebuildAsSource
    )
    {
        throw new NotImplementedException();
    }

    public static Text Format(
        TextFormat format,
        ReadOnlySpan<FormatArg> arguments,
        bool rebuildText,
        bool rebuildAsSource
    )
    {
        throw new NotImplementedException();
    }

    public static string FormatStr(
        TextFormat format,
        IReadOnlyDictionary<string, FormatArg> arguments,
        bool rebuildText,
        bool rebuildAsSource
    )
    {
        var estimatedArgLength = arguments.Sum(p => EstimateArgumentValueLength(p.Value));
        return Format(
            format,
            TextFormatContext.Create(
                arguments,
                (args, key, _) => args.TryGetValue(key.Name, out var arg) ? arg : null,
                estimatedArgLength,
                rebuildText,
                rebuildAsSource
            )
        );
    }

    public static string FormatStr(
        TextFormat format,
        IReadOnlyList<FormatArg> arguments,
        bool rebuildText,
        bool rebuildAsSource
    )
    {
        var estimatedArgLength = arguments.Sum(EstimateArgumentValueLength);
        return Format(
            format,
            TextFormatContext.Create(
                arguments,
                (args, key, argNumber) =>
                {
                    var argIndex = key.Index;
                    if (key.Index == -1)
                    {
                        argIndex = argNumber;
                    }

                    return args.Count > argIndex ? args[argIndex] : null;
                },
                estimatedArgLength,
                rebuildText,
                rebuildAsSource
            )
        );
    }

    public static string FormatStr(
        TextFormat format,
        ReadOnlySpan<FormatArg> arguments,
        bool rebuildText,
        bool rebuildAsSource
    )
    {
        var estimatedArgLength = arguments.AsValueEnumerable().Sum(EstimateArgumentValueLength);
        return Format(
            format,
            TextFormatContext.Create(
                arguments,
                (args, key, argNumber) =>
                {
                    var argIndex = key.Index;
                    if (key.Index == -1)
                    {
                        argIndex = argNumber;
                    }

                    return args.Length > argIndex ? args[argIndex] : null;
                },
                estimatedArgLength,
                rebuildText,
                rebuildAsSource
            )
        );
    }

    public static string Format<TContext>(TextFormat format, in TContext context)
        where TContext : ITextFormatContext, allows ref struct
    {
        var fmtPattern = format;
        if (context.RebuildAsSource)
        {
            var formatText = format.SourceText;

            if (context.RebuildText)
            {
                formatText.Rebuild();
            }

            fmtPattern = new TextFormat(formatText, fmtPattern.PatternDefinition);
        }

        return fmtPattern.Format(context);
    }

    private static int EstimateArgumentValueLength(FormatArg arg)
    {
        return arg.Match(_ => 20, _ => 20, _ => 20, _ => 20, text => text.ToString().Length, _ => 0);
    }
}
