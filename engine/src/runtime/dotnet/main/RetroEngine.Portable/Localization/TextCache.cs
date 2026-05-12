// // @file TextCache.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Utilities.Collections;

namespace RetroEngine.Portable.Localization;

public sealed class TextCache
{
    private readonly StripedDictionary<TextId, Text> _cachedText = new(32);

    public static TextCache Instance { get; } = new();

    public Text FindOrCache(ReadOnlySpan<char> textLiteral, TextId textId)
    {
        return FindOrCache(textLiteral.ToString(), textId);
    }

    public Text FindOrCache(string textLiteral, TextId textId)
    {
        return _cachedText.GetOrAdd(
            textId,
            textLiteral,
            (id, literal) => new Text(literal, id.Namespace, id.Key, TextFlag.Immutable)
        );
    }

    public void RemoveCache(TextId textId)
    {
        RemoveCache([textId]);
    }

    public void RemoveCache(ReadOnlySpan<TextId> textIds)
    {
        foreach (var textId in textIds)
        {
            _cachedText.Remove(textId);
        }
    }

    public void RemoveCache(IEnumerable<TextId> textIds)
    {
        foreach (var textId in textIds)
        {
            _cachedText.Remove(textId);
        }
    }
}
