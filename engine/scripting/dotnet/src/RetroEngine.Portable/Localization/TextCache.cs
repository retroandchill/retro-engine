// // @file TextCache.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using RetroEngine.Portable.Collections;

namespace RetroEngine.Portable.Localization;

public sealed class TextCache
{
    private readonly StripedDictionary<TextId, Text> _cachedText = new(32);

    public static TextCache Instance { get; } = new();

    public Text FindOrCache(ReadOnlySpan<char> textLiteral, TextId textId)
    {
        var existingText = FindExisting(textLiteral, textId);
        return existingText ?? CacheText(textLiteral.ToString(), textId);
    }

    public Text FindOrCache(string textLiteral, TextId textId)
    {
        var existingText = FindExisting(textLiteral, textId);
        return existingText ?? CacheText(textLiteral, textId);
    }

    private Text? FindExisting(ReadOnlySpan<char> textLiteral, TextId textId)
    {
        Text? text = null;
        _cachedText.GetAndApply(
            textLiteral,
            textId,
            (span, t) =>
            {
                var foundTextValue = t.TextData.SourceString;
                if (string.IsNullOrEmpty(foundTextValue) || !span.Equals(foundTextValue, StringComparison.Ordinal))
                    return;

                text = t;
            }
        );

        return text;
    }

    private Text CacheText(string textLiteral, TextId textId)
    {
        var newText = new Text(textLiteral, textId.Namespace, textId.Key, TextFlag.Immutable);
        _cachedText.Add(textId, newText);
        return newText;
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
