// // @file StringTable.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Data;
using System.Globalization;
using CsvHelper;
using LinkDotNet.StringBuilder;
using RetroEngine.Portable.Collections;
using RetroEngine.Portable.Strings;
using RetroEngine.Portable.Utils;
using ZLinq;
using Zomp.SyncMethodGenerator;

namespace RetroEngine.Portable.Localization.StringTables;

public sealed partial class StringTable
{
    public bool IsLoaded { get; set; } = true;

    public bool IsInternal { get; set; }

    public TextKey Namespace
    {
        get;
        set
        {
            using var scope = _entriesLock.EnterScope();
            if (field == value)
                return;
            field = value;

            foreach (var (key, oldEntry) in _entries)
            {
                oldEntry.Disown();
                _entries[key] = new StringTableEntry(this, oldEntry.SourceString, new TextId(field, key));
            }
        }
    }

    private readonly Dictionary<TextKey, StringTableEntry> _entries = new();
    private readonly Lock _entriesLock = new();

    private readonly Dictionary<TextKey, Dictionary<Name, string>> _metadata = new();
    private readonly Lock _metadataLock = new();

    public string? GetSourceString(TextKey key)
    {
        using var scope = _entriesLock.EnterScope();
        return _entries.TryGetValue(key, out var entry) ? entry.SourceString : null;
    }

    public void SetSourceString(TextKey key, string value)
    {
        if (key.IsEmpty)
            throw new ArgumentException("Key cannot be empty", nameof(key));

        using var scope = _entriesLock.EnterScope();

        if (_entries.TryGetValue(key, out var entry))
        {
            entry.Disown();
        }

        entry = new StringTableEntry(this, value, new TextId(Namespace, key));
        _entries[key] = entry;
    }

    public void RemoveSourceString(TextKey key)
    {
        using var scope = _entriesLock.EnterScope();
        if (!_entries.TryGetValue(key, out var entry))
            return;

        entry.Disown();
        _entries.Remove(key);
        ClearMetadata(key);
    }

    public IEnumerable<(TextKey Key, string SourceString)> EnumerateSourceStrings(Action<TextKey, string> action)
    {
        return _entries.Lock(_entriesLock).Select(x => (x.Key, x.Value.SourceString));
    }

    public void ClearSourceStrings()
    {
        using var scope = _entriesLock.EnterScope();
        foreach (var (_, entry) in _entries)
        {
            entry.Disown();
        }

        _entries.Clear();
        ClearMetadata();
    }

    public StringTableEntry? FindEntry(TextKey key)
    {
        using var scope = _entriesLock.EnterScope();
        return _entries.GetValueOrDefault(key);
    }

    public TextKey? FindKey(StringTableEntry entry)
    {
        if (!entry.IsOwnedBy(this))
            return null;

        using var scope = _entriesLock.EnterScope();
        foreach (var (key, value) in _entries)
        {
            if (ReferenceEquals(value, entry))
            {
                return key;
            }
        }

        return null;
    }

    public string GetMetadata(TextKey key, Name metaDataId)
    {
        using var scope = _metadataLock.EnterScope();
        return _metadata.GetValueOrDefault(key)?.GetValueOrDefault(metaDataId, "") ?? "";
    }

    public void SetMetadata(TextKey key, Name metaDataId, string value)
    {
        using var scope = _metadataLock.EnterScope();
        if (!_metadata.TryGetValue(key, out var metadata))
        {
            metadata = new Dictionary<Name, string>();
            _metadata[key] = metadata;
        }

        metadata[metaDataId] = value;
    }

    public void RemoveMetadata(TextKey key, Name metaDataId)
    {
        using var scope = _metadataLock.EnterScope();
        if (!_metadata.TryGetValue(key, out var metadata))
            return;

        metadata.Remove(metaDataId);
        if (metadata.Count == 0)
            _metadata.Remove(key);
    }

    public IEnumerable<KeyValuePair<Name, string>> EnumerateMetadata(TextKey key)
    {
        return _metadata.TryGetValue(key, out var metadata) ? metadata.Lock(_metadataLock) : [];
    }

    public void ClearMetadata(TextKey key)
    {
        using var scope = _metadataLock.EnterScope();
        _metadata.Remove(key);
    }

    public void ClearMetadata()
    {
        using var scope = _metadataLock.EnterScope();
        _metadata.Clear();
    }

    [CreateSyncVersion]
    public async ValueTask ExportStringsAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var dataTable = ToDataTable();
        await using var writer = new StreamWriter(stream, leaveOpen: true);
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        foreach (DataColumn dc in dataTable.Columns)
        {
            csv.WriteField(dc.ColumnName);
        }
        await csv.NextRecordAsync();

        foreach (DataRow dr in dataTable.Rows)
        {
            foreach (DataColumn dc in dataTable.Columns)
            {
                csv.WriteField(dr[dc]);
            }
            await csv.NextRecordAsync();
        }
    }

    private DataTable ToDataTable()
    {
        var dataTable = new DataTable();
        using var scope = _entriesLock.EnterScope();
        using var metadataLock = _metadataLock.EnterScope();

        var metadataColumnNames = _metadata.Values.SelectMany(x => x.Keys).Distinct().ToImmutableArray();

        dataTable.Columns.Add("Key", typeof(string));
        dataTable.Columns.Add("SourceString", typeof(string));
        foreach (var name in metadataColumnNames)
        {
            dataTable.Columns.Add(name.ToString(), typeof(string));
        }

        foreach (var (key, sourceString) in _entries)
        {
            var row = dataTable.NewRow();

            var exportedKey = key.ToString().ReplaceCharWithEscapedChar();
            var exportedSourceString = sourceString.SourceString.ReplaceCharWithEscapedChar();
            row[0] = exportedKey;
            row[1] = exportedSourceString;

            foreach (
                var (i, metadata) in metadataColumnNames
                    .AsValueEnumerable()
                    .Select(columnName => _metadata.GetValueOrDefault(key)?.GetValueOrDefault(columnName, "") ?? "")
                    .Index()
            )
            {
                row[i + 2] = metadata.ReplaceCharWithEscapedChar();
            }
        }

        return dataTable;
    }

    [CreateSyncVersion]
    public async ValueTask ImportStringsAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var dataTable = new DataTable();
        using (var reader = new StreamReader(stream, leaveOpen: true))
        {
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            await csv.ReadAsync();
            cancellationToken.ThrowIfCancellationRequested();
            csv.ReadHeader();
            if (csv.HeaderRecord is null)
                throw new InvalidOperationException("Header record is null.");

            foreach (var header in csv.HeaderRecord)
            {
                dataTable.Columns.Add(header);
            }

            while (await csv.ReadAsync())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var row = dataTable.NewRow();
                for (var i = 0; i < dataTable.Columns.Count; i++)
                {
                    row[i] = csv.GetField<string>(i);
                }
            }
        }

        FromDataTable(dataTable);
    }

    private void FromDataTable(DataTable dataTable)
    {
        if (dataTable.Columns.Count < 2)
            throw new InvalidOperationException("Table must have at least 2 columns.");

        var keyColumn = -1;
        var sourceStringColumn = -1;
        var metaDataColumns = new Dictionary<Name, int>();

        foreach (var (i, column) in dataTable.Columns.AsValueEnumerable().OfType<DataColumn>().Index())
        {
            var columnName = column.ColumnName;
            if (columnName.Equals("Key", StringComparison.OrdinalIgnoreCase) && keyColumn == -1)
            {
                keyColumn = i;
            }
            else if (columnName.Equals("SourceString", StringComparison.OrdinalIgnoreCase) && sourceStringColumn == -1)
            {
                sourceStringColumn = i;
            }
            else
            {
                var name = new Name(columnName);
                if (!name.IsNone)
                    metaDataColumns[name] = i;
            }
        }

        using (var builder = new ValueStringBuilder("Missing required columns: "))
        {
            var isValidHeader = true;
            if (keyColumn == -1)
            {
                isValidHeader = false;
                builder.Append("'Key'");
            }

            if (sourceStringColumn == -1)
            {
                isValidHeader = false;
                builder.Append("'SourceString'");
            }

            if (!isValidHeader)
                throw new InvalidOperationException(builder.ToString());
        }

        using var entriesScope = _entriesLock.EnterScope();
        using var metadataScope = _metadataLock.EnterScope();
        ClearSourceStrings();
        _entries.EnsureCapacity(dataTable.Rows.Count);
        _metadata.EnsureCapacity(dataTable.Rows.Count);

        foreach (DataRow row in dataTable.Rows)
        {
            var key = row.Field<string>(keyColumn)?.ReplaceEscapedCharWithChar();
            if (key is null)
                continue;

            var sourceString = row.Field<string>(sourceStringColumn)?.ReplaceEscapedCharWithChar();
            if (sourceString is null)
                continue;

            var tableEntry = new StringTableEntry(this, sourceString, new TextId(Namespace, key));
            _entries[key] = tableEntry;

            foreach (var (column, index) in metaDataColumns)
            {
                var value = row.Field<string>(index)?.ReplaceEscapedCharWithChar();
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                if (!_metadata.TryGetValue(key, out var metadata))
                {
                    metadata = new Dictionary<Name, string>();
                    _metadata[key] = metadata;
                }

                metadata[column] = value;
            }
        }
    }

    public static void CreateNew(Name tableId, TextKey ns)
    {
        var stringTable = new StringTable { Namespace = ns };
        StringTableRegistry.Instance.RegisterStringTable(tableId, stringTable);
    }

    [CreateSyncVersion]
    public static async ValueTask LoadAsync(
        Stream stream,
        Name tableId,
        TextKey ns,
        CancellationToken cancellationToken = default
    )
    {
        var stringTable = new StringTable { Namespace = ns };
        await stringTable.ImportStringsAsync(stream, cancellationToken);
        StringTableRegistry.Instance.RegisterStringTable(tableId, stringTable);
    }

    public static void SetEntry(Name tableId, TextKey key, string value)
    {
        var stringTable = StringTableRegistry.Instance.FindStringTable(tableId);
        if (stringTable is null)
            throw new InvalidOperationException($"String table with ID {tableId} does not exist.");

        stringTable.SetSourceString(key, value);
    }

    public static void SetMetadata(Name tableId, TextKey key, Name metaDataId, string value)
    {
        var stringTable = StringTableRegistry.Instance.FindStringTable(tableId);
        if (stringTable is null)
            throw new InvalidOperationException($"String table with ID {tableId} does not exist.");

        stringTable.SetMetadata(key, metaDataId, value);
    }

    public static Text From(Name tableId, TextKey key)
    {
        return new Text(tableId, key, StringTableLoadingPolicy.FindOrLoad);
    }
}
