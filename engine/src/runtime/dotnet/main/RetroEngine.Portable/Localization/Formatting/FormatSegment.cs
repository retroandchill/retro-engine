// // @file FormatSegment.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using RetroEngine.Utilities;

namespace RetroEngine.Portable.Localization.Formatting;

public readonly struct FormatSegment
{
    private readonly string? _literal;
    private readonly FormatPlaceholder _placeholder;

    public FormatSegment(string literal)
    {
        _literal = literal;
        _placeholder = default;
    }

    public FormatSegment(FormatPlaceholder placeholder)
    {
        _literal = null;
        _placeholder = placeholder;
    }

    public bool TryGetValue([NotNullWhen(true)] out string? value)
    {
        if (_literal is not null)
        {
            value = _literal;
            return true;
        }

        value = null;
        return false;
    }

    public bool TryGetValue(out FormatPlaceholder value)
    {
        if (_literal is null)
        {
            value = _placeholder;
            return true;
        }

        value = default;
        return false;
    }

    public static implicit operator FormatSegment(string literal) => new(literal);

    public static implicit operator FormatSegment(FormatPlaceholder placeholder) => new(placeholder);
}

public readonly record struct PlaceholderKey
{
    public string Name { get; }
    public int Index { get; }

    public PlaceholderKey(string name)
    {
        Name = name;
        Index = int.TryParse(name, out var index) ? index : -1;
    }

    public static implicit operator PlaceholderKey(string key) => new(key);
}

public readonly record struct FormatPlaceholder(
    PlaceholderKey Key,
    string? ModifierPattern = null,
    ITextFormatArgumentModifier? Modifier = null
);
