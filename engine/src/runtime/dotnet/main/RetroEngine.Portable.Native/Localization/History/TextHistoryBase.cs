// // @file TextHistoryBase.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using System.Text.Json;
using RetroEngine.Portable.Localization.Formatting;
using RetroEngine.Portable.Utils;

namespace RetroEngine.Portable.Localization.History;

internal class TextHistoryBase : TextHistory
{
    private TextId _textId = TextId.Empty;
    public sealed override TextId TextId => _textId;
    private string _source = "";
    private string? _localized;

    public TextHistoryBase() { }

    public TextHistoryBase(TextId id, string source, string? localized = null)
    {
        _textId = id;
        _source = source;
        _localized = localized;
    }

    public override string SourceString => _source;
    public override string DisplayString => _localized ?? _source;
    public override string? LocalizedString => _localized;

    public override bool ReadFromBuffer(
        ReadOnlySpan<char> buffer,
        string? textNamespace,
        string? textKey,
        out ReadOnlySpan<char> remaining
    )
    {
        if (buffer.PeekMarker(TextStringificationUtil.NsLocTextMarker))
        {
            buffer = buffer[TextStringificationUtil.NsLocTextMarker.Length..];

            if (!buffer.SkipWhitespaceAndCharacter('(', out buffer))
            {
                remaining = default;
                return false;
            }

            buffer = buffer.SkipWhitespace();
            if (!buffer.ReadQuotedString(out var ns, out buffer))
            {
                remaining = default;
                return false;
            }

            if (!buffer.SkipWhitespaceAndCharacter(',', out buffer))
            {
                remaining = default;
                return false;
            }

            buffer = buffer.SkipWhitespace();
            if (!buffer.ReadQuotedString(out var key, out buffer))
            {
                remaining = default;
                return false;
            }

            buffer = buffer.SkipWhitespace();
            if (!buffer.ReadQuotedString(out var source, out buffer))
            {
                remaining = default;
                return false;
            }

            _source = source;

            if (!buffer.SkipWhitespaceAndCharacter(')', out buffer))
            {
                remaining = default;
                return false;
            }

            if (string.IsNullOrEmpty(key))
            {
                key = Guid.NewGuid().ToString();
            }

            _textId = new TextId(ns, key);
            _localized = null;
            MarkDisplayStringOutOfDate();

            remaining = buffer;
            return true;
        }

        if (buffer.PeekMarker(TextStringificationUtil.LocTextMarker))
        {
            buffer = buffer[TextStringificationUtil.LocTextMarker.Length..];

            if (!buffer.SkipWhitespaceAndCharacter('(', out buffer))
            {
                remaining = default;
                return false;
            }

            buffer = buffer.SkipWhitespace();
            if (!buffer.ReadQuotedString(out var key, out buffer))
            {
                remaining = default;
                return false;
            }

            buffer = buffer.SkipWhitespace();
            if (!buffer.ReadQuotedString(out var source, out buffer))
            {
                remaining = default;
                return false;
            }

            _source = source;

            if (!buffer.SkipWhitespaceAndCharacter(')', out buffer))
            {
                remaining = default;
                return false;
            }

            if (string.IsNullOrEmpty(key))
            {
                key = Guid.NewGuid().ToString();
            }

            var ns = textNamespace ?? "";

            _textId = new TextId(ns, key);
            _localized = null;
            MarkDisplayStringOutOfDate();

            remaining = buffer;
            return true;
        }

        remaining = default;
        return false;
    }

    public override bool WriteToBuffer(StringBuilder buffer)
    {
        if (TextId.IsEmpty)
            return false;

        var ns = TextId.Namespace.ToString();
        var key = TextId.Key.ToString();

        buffer.Append("NSLOCTEXT(\"");
        buffer.Append(ns.ReplaceQuotesWithEscapedQuotes());
        buffer.Append("\", \"");
        buffer.Append(key.ReplaceQuotesWithEscapedQuotes());
        buffer.Append(_source.ReplaceQuotesWithEscapedQuotes());
        buffer.Append("\")");

        return true;
    }

    public override string BuildInvariantDisplayString()
    {
        return _source;
    }

    public override bool IdenticalTo(TextHistory other, TextIdenticalModeFlags flags)
    {
        return false;
    }

    protected override bool CanUpdateDisplayString => !TextId.IsEmpty;

    internal override void UpdateDisplayString()
    {
        _localized = LocalizationManager.Instance.GetDisplayString(TextId.Namespace, TextId.Key, _source);
    }
}
