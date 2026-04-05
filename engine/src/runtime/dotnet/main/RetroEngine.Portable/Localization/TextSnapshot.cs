// // @file TextSnapshot.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization;

internal readonly struct TextSnapshot(Text text)
{
    private readonly ITextData? _textData = text.TextData;
    private readonly string? _localizedString = text.TextData.History.DisplayString;
    private readonly TextRevisions _revisions = GetHistoryForText(text);
    private readonly TextFlag _flags = text.Flags;

    public bool IsIdenticalTo(Text text)
    {
        text.Rebuild();

        return _textData is not null
            && _textData == text.TextData
            && _localizedString == text.TextData.LocalizedString
            && _revisions == GetHistoryForText(text)
            && _flags == text.Flags;
    }

    public bool IsDisplayStringEqualTo(Text text)
    {
        text.Rebuild();

        return _textData is not null
            && _revisions == GetHistoryForText(text)
            && string.Equals(_localizedString, text.TextData.LocalizedString, StringComparison.Ordinal);
    }

    private static TextRevisions GetHistoryForText(Text text)
    {
        return text.IsEmpty || text.IsCultureInvariant ? new TextRevisions(0, 0) : text.TextData.Revisions;
    }
}
