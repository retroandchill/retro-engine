// // @file TextHistorySimple.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using RetroEngine.Portable.Utils;

namespace RetroEngine.Portable.Localization.History;

internal sealed class TextHistorySimple : TextHistoryBase, ITextHistory
{
    public TextHistorySimple() { }

    public TextHistorySimple(TextId id, string source, string? localized = null)
        : base(id, source, localized) { }

    public static bool ShouldReadFromBuffer(ReadOnlySpan<char> buffer)
    {
        return buffer.PeekMarker(TextStringificationUtil.NsLocTextMarker)
            || buffer.PeekMarker(TextStringificationUtil.LocTextMarker);
    }

    public static ITextData? ReadFromBuffer(
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
                return null;
            }

            buffer = buffer.SkipWhitespace();
            if (!buffer.ReadQuotedString(out var ns, out buffer) || !buffer.SkipWhitespaceAndCharacter(',', out buffer))
            {
                remaining = default;
                return null;
            }

            buffer = buffer.SkipWhitespace();
            if (!buffer.ReadQuotedString(out var key, out buffer))
            {
                remaining = default;
                return null;
            }

            buffer = buffer.SkipWhitespace();
            if (!buffer.ReadQuotedString(out var source, out buffer))
            {
                remaining = default;
                return null;
            }

            if (!buffer.SkipWhitespaceAndCharacter(')', out buffer))
            {
                remaining = default;
                return null;
            }

            if (string.IsNullOrEmpty(key))
            {
                key = Guid.NewGuid().ToString();
            }

            remaining = buffer;
            return new TextHistorySimple(new TextId(ns, key), source);
        }

        if (buffer.PeekMarker(TextStringificationUtil.LocTextMarker))
        {
            buffer = buffer[TextStringificationUtil.LocTextMarker.Length..];

            if (!buffer.SkipWhitespaceAndCharacter('(', out buffer))
            {
                remaining = default;
                return null;
            }

            buffer = buffer.SkipWhitespace();
            if (!buffer.ReadQuotedString(out var key, out buffer))
            {
                remaining = default;
                return null;
            }

            buffer = buffer.SkipWhitespace();
            if (!buffer.ReadQuotedString(out var source, out buffer))
            {
                remaining = default;
                return null;
            }

            if (!buffer.SkipWhitespaceAndCharacter(')', out buffer))
            {
                remaining = default;
                return null;
            }

            if (string.IsNullOrEmpty(key))
            {
                key = Guid.NewGuid().ToString();
            }

            var ns = textNamespace ?? "";

            remaining = buffer;
            return new TextHistorySimple(new TextId(ns, key), source);
        }

        remaining = default;
        return null;
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
        buffer.Append(Source.ReplaceQuotesWithEscapedQuotes());
        buffer.Append("\")");

        return true;
    }
}
