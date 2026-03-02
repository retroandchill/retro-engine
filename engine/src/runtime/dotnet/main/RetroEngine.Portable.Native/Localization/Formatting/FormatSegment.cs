// // @file FormatSegment.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Utils;

namespace RetroEngine.Portable.Localization.Formatting;

[Union]
public readonly partial struct FormatSegment
{
    [UnionCase]
    public static partial FormatSegment Literal(string text);

    [UnionCase]
    public static partial FormatSegment Placeholder(PlaceholderKey key, ITextFormatArgumentModifier? modifier);
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
