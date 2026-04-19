// // @file TextHistorySimple.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using MagicArchive;
using RetroEngine.Portable.Localization.Stringification;
using RetroEngine.Utilities;
using ZParse;

namespace RetroEngine.Portable.Localization.History;

[Archivable]
internal sealed partial class TextHistorySimple : TextHistoryBase, ITextHistory
{
    [ArchiveInclude]
    private TextKey Namespace => TextId.Namespace;

    [ArchiveInclude]
    private TextKey Key => TextId.Key;

    [ArchiveInclude]
    private string Source => SourceString;

    public TextHistorySimple() { }

    public TextHistorySimple(TextId id, string source, string? localized = null)
        : base(id, source, localized) { }

    [ArchivableConstructor]
    public TextHistorySimple(TextKey @namespace, TextKey key, string source)
        : base(new TextId(@namespace, key), source) { }

    private static readonly TextParser<ITextData> NsLocTextParser = TextStringReader.Marked(
        Markers.NsLocText,
        TextStringReader
            .TextLiteral.Then(
                TextStringReader.CommaSeparator.IgnoreThen(TextStringReader.TextLiteral),
                (n, k) => (Namespace: n, Key: !string.IsNullOrEmpty(k) ? k : Guid.NewGuid().ToString())
            )
            .Then(
                TextStringReader.CommaSeparator.IgnoreThen(TextStringReader.TextLiteral),
                ITextData (p, s) => new TextHistorySimple(new TextId(p.Namespace, p.Key), s)
            )
    );

    private static readonly TextParser<(string Key, string Source)> LocTextParser = TextStringReader.Marked(
        Markers.LocText,
        TextStringReader.TextLiteral.Then(
            TextStringReader.CommaSeparator.IgnoreThen(TextStringReader.TextLiteral),
            (k, s) => (!string.IsNullOrEmpty(k) ? k : Guid.NewGuid().ToString(), s)
        )
    );

    public static ParseResult<ITextData> ImportFromString(TextSegment input, string? textNamespace)
    {
        var nsloctextResult = NsLocTextParser(input);
        if (nsloctextResult.HasValue)
            return nsloctextResult;

        var loctextResult = LocTextParser(input);
        if (!loctextResult.HasValue)
            return ParseResult.CombineEmpty(
                nsloctextResult,
                ParseResult.CastEmpty<(string Key, string Source), ITextData>(loctextResult)
            );

        var (key, sourceString) = loctextResult.Value;
        return ParseResult.Success<ITextData>(
            new TextHistorySimple(new TextId(textNamespace ?? "", key), sourceString),
            input,
            loctextResult.Remainder
        );
    }

    public override bool ExportToString(StringBuilder buffer)
    {
        if (TextId.IsEmpty)
            return false;

        var ns = TextId.Namespace.ToString();
        var key = TextId.Key.ToString();

        buffer.Append("NSLOCTEXT(\"");
        buffer.Append(ns.ReplaceCharWithEscapedChar());
        buffer.Append("\", \"");
        buffer.Append(key.ReplaceCharWithEscapedChar());
        buffer.Append("\", \"");
        buffer.Append(Source.ReplaceCharWithEscapedChar());
        buffer.Append("\")");

        return true;
    }
}
