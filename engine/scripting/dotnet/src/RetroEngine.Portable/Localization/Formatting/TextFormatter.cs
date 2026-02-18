// // @file TextFormatter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Superpower;

namespace RetroEngine.Portable.Localization.Formatting;

internal readonly struct ArgumentTokenSpecifier
{
    public string ArgumentName { get; }

    public int? ArgumentIndex { get; }

    public ArgumentTokenSpecifier(string argumentName)
    {
        ArgumentName = argumentName;
        if (int.TryParse(argumentName, out var index) && index >= 0)
        {
            ArgumentIndex = index;
        }
    }

    public void Deconstruct(out string argumentName, out int? argumentIndex)
    {
        argumentName = ArgumentName;
        argumentIndex = ArgumentIndex;
    }
}

public sealed class TextFormatter
{
    private readonly ConcurrentDictionary<string, TextParser<ITextFormatArgumentModifier>> _argumentModifiers = new();

    private TextFormatter() { }

    public static TextFormatter Instance { get; } = new();

    public void RegisterTextArgumentModifier(string keyword, TextParser<ITextFormatArgumentModifier> compileDelegate)
    {
        _argumentModifiers.TryAdd(keyword, compileDelegate);
    }

    public void UnregisterTextArgumentModifier(string keyword)
    {
        _argumentModifiers.Remove(keyword, out _);
    }

    public bool TryGetArgumentModifier(
        string keyword,
        [NotNullWhen(true)] out TextParser<ITextFormatArgumentModifier>? modifier
    )
    {
        return _argumentModifiers.TryGetValue(keyword, out modifier);
    }
}
