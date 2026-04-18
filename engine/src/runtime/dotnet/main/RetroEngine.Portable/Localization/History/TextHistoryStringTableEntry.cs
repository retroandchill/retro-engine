// // @file TextHistoryStringTableEntry.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Text;
using MagicArchive;
using RetroEngine.Portable.Localization.Stringification;
using RetroEngine.Portable.Localization.StringTables;
using RetroEngine.Portable.Strings;
using RetroEngine.Utilities;
using ZParse;
using ZParse.Parsers;

namespace RetroEngine.Portable.Localization.History;

[Archivable]
internal sealed partial class TextHistoryStringTableEntry : TextHistory, ITextHistory
{
    private enum StringTableLoadingPhase : byte
    {
        PendingLoad,
        Loading,
        Loaded,
    }

    private sealed class StringTableReferenceData
    {
        public Name TableId { get; private set; }
        public TextKey Key { get; }
        private StringTableLoadingPhase _loadingPhase;

        [ArchiveIgnore]
        public TextId TextId => ResolveStringTableEntry()?.DisplayStringId ?? TextId.Empty;

        private WeakReference<StringTableEntry>? _entry;
        private string? _displayString;

        private readonly Lock _lock = new();

        public StringTableReferenceData(Name tableId, TextKey key, StringTableLoadingPolicy loadingPolicy)
        {
            TableId = tableId;
            Key = key;

            switch (loadingPolicy)
            {
                case StringTableLoadingPolicy.Find:
                    _loadingPhase = StringTableLoadingPhase.Loaded;
                    ResolveDisplayString();
                    break;
                case StringTableLoadingPolicy.FindOrFullyLoad when StringTableLoader.CanFindOrLoadStringTableAsset:
                    _loadingPhase = StringTableLoadingPhase.Loaded;
                    StringTableLoader.LoadStringTableAsset(TableId);
                    ResolveDisplayString();
                    break;
                case StringTableLoadingPolicy.FindOrFullyLoad:
                case StringTableLoadingPolicy.FindOrLoad:
                    _loadingPhase = StringTableLoadingPhase.PendingLoad;
                    ConditionalBeginAssetLoad();
                    break;
                default:
                    throw new ArgumentException("Unknown loading policy");
            }
        }

        public bool IsIdentical(StringTableReferenceData other)
        {
            using var scope = _lock.EnterScope();
            using var otherScope = other._lock.EnterScope();
            return TableId == other.TableId && Key == other.Key;
        }

        public StringTableEntry? ResolveStringTableEntry()
        {
            var entry = _entry?.Target;
            if (entry is null)
            {
                ConditionalBeginAssetLoad();
            }

            if (entry is null || !entry.IsOwned)
            {
                using var scope = _lock.EnterScope();
                _entry = null;
                _displayString = null;

                if (_loadingPhase != StringTableLoadingPhase.Loaded)
                {
                    return null;
                }

                var stringTable = StringTableRegistry.Instance.FindStringTable(TableId);
                if (stringTable is not null)
                {
                    if (!stringTable.IsLoaded)
                    {
                        return null;
                    }

                    entry = stringTable.FindEntry(Key);
                }

                _entry = entry is not null ? new WeakReference<StringTableEntry>(entry) : null;
            }

            if (entry is null)
            {
                StringTableRegistry.Instance.LogMissingStringTable(TableId, Key);
            }

            return entry;
        }

        public string? ResolveDisplayString(bool forceRefresh = false)
        {
            var entry = ResolveStringTableEntry();
            if (entry is not null && (forceRefresh || _displayString is null))
            {
                _displayString = entry.DisplayString;
            }

            return _displayString;
        }

        private void ConditionalBeginAssetLoad()
        {
            Name tableIdToLoad;
            using (_lock.EnterScope())
            {
                if (_loadingPhase != StringTableLoadingPhase.PendingLoad)
                    return;

                tableIdToLoad = TableId;
                _loadingPhase = StringTableLoadingPhase.Loading;
            }

            _ = LoadStringTableAsset();

            return;

            async Task LoadStringTableAsset()
            {
                var loadedTableId = await StringTableLoader.LoadStringTableAssetAsync(tableIdToLoad);

                using var scope = _lock.EnterScope();
                Debug.Assert(TableId == loadedTableId);

                if (!loadedTableId.IsNone)
                {
                    TableId = loadedTableId;
                }
                _loadingPhase = StringTableLoadingPhase.Loaded;
                ResolveDisplayString();
            }
        }
    }

    private readonly StringTableReferenceData _stringTableReferenceData;

    [ArchiveInclude]
    private Name TableId => _stringTableReferenceData.TableId;

    [ArchiveInclude]
    private TextKey Key => _stringTableReferenceData.Key;

    public override TextId TextId => _stringTableReferenceData.TextId;

    public override string? LocalizedString => _stringTableReferenceData.ResolveDisplayString();

    public override string SourceString =>
        _stringTableReferenceData.ResolveStringTableEntry()?.SourceString ?? StringTableEntry.PlaceholderSourceString;

    public override string DisplayString => LocalizedString ?? SourceString;

    [ArchivableConstructor]
    private TextHistoryStringTableEntry(Name tableId, TextKey key)
    {
        _stringTableReferenceData = new StringTableReferenceData(tableId, key, StringTableLoadingPolicy.FindOrLoad);
        MarkDisplayStringUpToDate();
    }

    public TextHistoryStringTableEntry(Name tableId, TextKey key, StringTableLoadingPolicy loadingPolicy)
    {
        _stringTableReferenceData = new StringTableReferenceData(tableId, key, loadingPolicy);
        MarkDisplayStringUpToDate();
    }

    public override string BuildInvariantDisplayString()
    {
        return SourceString;
    }

    public override bool IdenticalTo(TextHistory other, TextIdenticalModeFlags flags)
    {
        return other is TextHistoryStringTableEntry otherEntry
            && otherEntry._stringTableReferenceData.IsIdentical(_stringTableReferenceData);
    }

    internal override void UpdateDisplayString()
    {
        _stringTableReferenceData.ResolveDisplayString(true);
    }

    private static readonly TextParser<ITextData> Parser = TextStringReader.Marked(
        Markers.LocTable,
        TextStringReader.TextLiteral.Then(
            TextStringReader.CommaSeparator.IgnoreThen(TextStringReader.TextLiteral),
            ITextData (t, k) => new TextHistoryStringTableEntry(t, k, StringTableLoadingPolicy.FindOrLoad)
        )
    );

    public static ParseResult<ITextData> ImportFromString(TextSegment input, string? textNamespace)
    {
        return Parser(input);
    }

    public override bool ExportToString(StringBuilder buffer)
    {
        buffer.Append($"{Markers.LocTable}(\"");
        buffer.Append(_stringTableReferenceData.TableId.ToString().ReplaceCharWithEscapedChar());
        buffer.Append("\", \"");
        buffer.Append(_stringTableReferenceData.Key.ToString().ReplaceCharWithEscapedChar());
        buffer.Append("\")");
        return true;
    }
}
