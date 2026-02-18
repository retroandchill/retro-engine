// // @file TextSnapshot.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Portable.Localization;

internal readonly struct TextSnapshot
{
    private readonly ITextData? _textData;
    private readonly string? _localizedString;
    private readonly TextRevision _revision;
    private readonly TextFlag _flags;

    public TextSnapshot() { }

    public TextSnapshot(Text text)
    {
        _textData = text.TextData;
        _localizedString = text.TextData.History.DisplayString;
        _revision = GetHistoryForText(text);
        _flags = text.Flags;
    }

    public bool IsIdenticalTo(Text text)
    {
        text.Rebuild();

        return _textData is not null
            && _textData == text.TextData
            && _localizedString == text.TextData.LocalizedString
            && _revision == GetHistoryForText(text)
            && _flags == text.Flags;
    }

    public bool IsDisplayStringEqualTo(Text text)
    {
        text.Rebuild();

        return _textData is not null
            && _revision == GetHistoryForText(text)
            && string.Equals(_localizedString, text.TextData.LocalizedString, StringComparison.Ordinal);
    }

    private static TextRevision GetHistoryForText(Text text)
    {
        return text.IsEmpty || text.IsCultureInvariant ? new TextRevision(0, 0) : text.TextData.Revision;
    }
}
