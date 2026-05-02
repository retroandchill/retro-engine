// // @file AssetPackageEntryList.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections;
using System.Runtime.CompilerServices;
using ZLinq;

namespace RetroEngine.Assets;

[CollectionBuilder(typeof(AssetPackageEntryList), nameof(AssetPackageEntryList.Create))]
internal sealed class AssetPackageEntryList<TEntry> : IReadOnlyCollection<TEntry>
    where TEntry : class, IAssetPackageEntry
{
    private readonly TEntry[] _entries;

    public int Count => _entries.Length;

    // ReSharper disable once UseCollectionExpression
    public static AssetPackageEntryList<TEntry> Empty { get; } = new();

    public AssetPackageEntryList()
    {
        _entries = [];
    }

    internal AssetPackageEntryList(ReadOnlySpan<TEntry> entries)
    {
        _entries = entries.AsValueEnumerable().OrderBy(x => x, AssetPackageEntryComparer.Default).ToArray();
    }

    internal AssetPackageEntryList(IEnumerable<TEntry> entries)
    {
        _entries = entries.OrderBy(x => x, AssetPackageEntryComparer.Default).ToArray();
    }

    private AssetPackageEntryList(TEntry[] entries)
    {
        _entries = entries;
    }

    public TEntry? GetOrDefault(AssetPackageEntryKey key)
    {
        var index = IndexOf(key);
        return index >= 0 ? _entries[index] : null;
    }

    public AssetPackageEntryList<TEntry> Add(TEntry entry)
    {
        var index = IndexOf(entry.Key);
        if (index > 0)
            return this;

        index = ~index;
        var newEntries = new TEntry[_entries.Length + 1];
        Array.Copy(_entries, newEntries, index);
        newEntries[index] = entry;
        Array.Copy(_entries, index, newEntries, index + 1, _entries.Length - index);
        return new AssetPackageEntryList<TEntry>(newEntries);
    }

    public AssetPackageEntryList<TEntry> AddOrReplace(TEntry entry)
    {
        var index = IndexOf(entry.Key);
        if (index < 0)
        {
            index = ~index;
            var newEntries = new TEntry[_entries.Length + 1];
            Array.Copy(_entries, newEntries, index);
            newEntries[index] = entry;
            Array.Copy(_entries, index, newEntries, index + 1, _entries.Length - index);
            return new AssetPackageEntryList<TEntry>(newEntries);
        }
        else
        {
            var newEntries = new TEntry[_entries.Length];
            Array.Copy(_entries, newEntries, _entries.Length);
            newEntries[index] = entry;
            return new AssetPackageEntryList<TEntry>(newEntries);
        }
    }

    public AssetPackageEntryList<TEntry> Remove(TEntry entry)
    {
        var index = IndexOf(entry.Key);
        if (index < 0)
            return this;

        if (Count == 1)
            return Empty;

        var newEntries = new TEntry[_entries.Length - 1];
        Array.Copy(_entries, newEntries, index);
        Array.Copy(_entries, index + 1, newEntries, index, _entries.Length - index - 1);
        return new AssetPackageEntryList<TEntry>(newEntries);
    }

    public AssetPackageEntryList<TEntry> Replace(AssetPackageEntryKey oldKey, TEntry newEntry)
    {
        var oldIndex = IndexOf(oldKey);
        if (oldIndex < 0)
            return this;

        var newIndex = IndexOf(newEntry.Key);

        var newEntries = new TEntry[_entries.Length];
        if (oldIndex == newIndex)
        {
            Array.Copy(_entries, newEntries, _entries.Length);
            newEntries[oldIndex] = newEntry;
        }
        else if (newIndex >= 0)
        {
            throw new InvalidOperationException("Cannot replace an entry with an existing entry");
        }

        var insertIndex = ~newIndex;
        if (insertIndex > oldIndex)
            insertIndex--;

        if (oldIndex < insertIndex)
        {
            Array.Copy(_entries, newEntries, oldIndex);
            Array.Copy(_entries, oldIndex + 1, newEntries, oldIndex, insertIndex - oldIndex);
            newEntries[insertIndex] = newEntry;
            Array.Copy(_entries, insertIndex + 1, newEntries, insertIndex + 1, _entries.Length - insertIndex - 1);
        }
        else
        {
            Array.Copy(_entries, newEntries, insertIndex);
            newEntries[insertIndex] = newEntry;
            Array.Copy(_entries, insertIndex, newEntries, insertIndex + 1, oldIndex - insertIndex);
            Array.Copy(_entries, oldIndex + 1, newEntries, oldIndex + 1, _entries.Length - oldIndex - 1);
        }

        return new AssetPackageEntryList<TEntry>(newEntries);
    }

    public AssetPackageEntryList<TEntry> Intersect(IEnumerable<AssetPackageEntryKey> keys)
    {
        if (keys.TryGetNonEnumeratedCount(out var count) && count == 0)
        {
            return Empty;
        }

        var keyArray = keys.ToArray();
        if (keyArray.Length == 0)
        {
            return Empty;
        }

        Array.Sort(keyArray);
        var stagingBuffer = ArrayPool<TEntry>.Shared.Rent(count);
        try
        {
            _entries.AsSpan().CopyTo(stagingBuffer);
            var writeIndex = 0;
            var entryIndex = 0;
            var keyIndex = 0;

            while (entryIndex < Count && keyIndex < keyArray.Length)
            {
                var entry = stagingBuffer[entryIndex];
                var comparison = entry.Key.CompareTo(keyArray[keyIndex]);

                switch (comparison)
                {
                    case < 0:
                        entryIndex++;
                        break;
                    case > 0:
                        keyIndex++;
                        break;
                    default:
                    {
                        stagingBuffer[writeIndex++] = entry;
                        entryIndex++;

                        var matchedKey = keyArray[keyIndex];
                        do
                        {
                            keyIndex++;
                        } while (
                            keyIndex < keyArray.Length && stagingBuffer[entryIndex].Key.CompareTo(matchedKey) == 0
                        );

                        break;
                    }
                }
            }

            return writeIndex != Count ? new AssetPackageEntryList<TEntry>(stagingBuffer.AsSpan(0, writeIndex)) : this;
        }
        finally
        {
            ArrayPool<TEntry>.Shared.Return(stagingBuffer, true);
        }
    }

    private int IndexOf(AssetPackageEntryKey key)
    {
        return _entries.BinarySearch(key);
    }

    public IEnumerator<TEntry> GetEnumerator()
    {
        return ((IEnumerable<TEntry>)_entries).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public Builder ToBuilder(int additionalCapacity = 0)
    {
        var entriesCopy = new TEntry[_entries.Length + additionalCapacity];
        Array.Copy(_entries, entriesCopy, _entries.Length);
        return new Builder(entriesCopy, _entries.Length);
    }

    public sealed class Builder : ICollection<TEntry>
    {
        private TEntry[] _entries;
        private int _capacity;
        private uint _version;

        public int Count { get; private set; }
        public bool IsReadOnly => false;
        private Span<TEntry> ActiveEntries => _entries.AsSpan(0, Count);

        public Builder()
            : this(8) { }

        public Builder(int capacity)
        {
            _entries = new TEntry[capacity];
            _capacity = capacity;
        }

        internal Builder(TEntry[] entries, int length)
        {
            _entries = entries;
            _capacity = entries.Length;
            Count = length;
        }

        public Enumerator GetEnumerator() => new(this);

        IEnumerator<TEntry> IEnumerable<TEntry>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(TEntry item)
        {
            _version++;
            if (_capacity == 0)
                Grow(8);
            if (Count == _capacity)
                Grow(Count * 2);

            var index = IndexOf(item.Key);
            if (index >= 0)
            {
                _entries[index] = item;
                return;
            }

            index = ~index;
            if (index < Count - 1)
            {
                Array.Copy(_entries, index, _entries, index + 1, Count - index);
            }
            _entries[index] = item;
            Count++;
        }

        public void AddOrReplace(TEntry item)
        {
            _version++;
            if (_capacity == 0)
                Grow(8);
            if (Count == _capacity)
                Grow(Count * 2);

            var index = IndexOf(item.Key);
            if (index < 0)
            {
                index = ~index;
                if (index < Count)
                {
                    Array.Copy(_entries, index, _entries, index + 1, Count - index);
                }
                Count++;
            }

            _entries[index] = item;
        }

        public void Clear()
        {
            _version++;
            for (var i = 0; i < Count; i++)
            {
                _entries[i] = null!;
            }

            Count = 0;
        }

        public bool Contains(TEntry item)
        {
            return Contains(item.Key);
        }

        public bool Contains(AssetPackageEntryKey key)
        {
            return IndexOf(key) >= 0;
        }

        public void CopyTo(TEntry[] array, int arrayIndex)
        {
            ActiveEntries.CopyTo(array.AsSpan(arrayIndex));
        }

        public bool Remove(TEntry entry)
        {
            return Remove(entry.Key);
        }

        public bool Remove(AssetPackageEntryKey key)
        {
            var index = IndexOf(key);
            if (index < 0)
                return false;

            _version++;
            if (index == Count - 1)
            {
                _entries[index] = null!;
                Count--;
                return true;
            }

            for (var i = index; i < Count - 1; i++)
            {
                _entries[i] = _entries[i + 1];
            }

            _entries[Count - 1] = null!;
            Count--;
            return true;
        }

        public void Intersect(IEnumerable<AssetPackageEntryKey> keys)
        {
            if (keys.TryGetNonEnumeratedCount(out var count) && count == 0)
            {
                Clear();
                return;
            }

            var keyArray = keys.ToArray();
            if (keyArray.Length == 0)
            {
                Clear();
                return;
            }

            Array.Sort(keyArray);

            var writeIndex = 0;
            var entryIndex = 0;
            var keyIndex = 0;

            while (entryIndex < Count && keyIndex < keyArray.Length)
            {
                var entry = _entries[entryIndex];
                var comparison = entry.Key.CompareTo(keyArray[keyIndex]);

                switch (comparison)
                {
                    case < 0:
                        entryIndex++;
                        break;
                    case > 0:
                        keyIndex++;
                        break;
                    default:
                    {
                        _entries[writeIndex++] = entry;
                        entryIndex++;

                        var matchedKey = keyArray[keyIndex];
                        do
                        {
                            keyIndex++;
                        } while (keyIndex < keyArray.Length && _entries[entryIndex].Key.CompareTo(matchedKey) == 0);

                        break;
                    }
                }
            }

            if (writeIndex == Count)
                return;

            _version++;

            for (var i = writeIndex; i < Count; i++)
            {
                _entries[i] = null!;
            }
            Count = writeIndex;
        }

        public void EnsureCapacity(int capacity)
        {
            if (capacity <= _capacity)
                return;

            _version++;
            Grow(capacity);
        }

        private void Grow(int newCapacity)
        {
            _capacity = newCapacity;
            Array.Resize(ref _entries, newCapacity);
        }

        private int IndexOf(AssetPackageEntryKey item)
        {
            return ActiveEntries.BinarySearch(item);
        }

        public AssetPackageEntryList<TEntry> ToImmutable()
        {
            if (Count == 0)
                return Empty;

            var targetArray = new TEntry[Count];
            Array.Copy(_entries, targetArray, Count);
            return new AssetPackageEntryList<TEntry>(targetArray);
        }

        public AssetPackageEntryList<TEntry> DrainToImmutable()
        {
            if (Count == 0)
                return Empty;

            return Count == _entries.Length
                ? new AssetPackageEntryList<TEntry>(Interlocked.Exchange(ref _entries, []))
                : ToImmutable();
        }

        public sealed class Enumerator(Builder builder) : IEnumerator<TEntry>
        {
            private readonly uint _version = builder._version;
            private int _index;

            object IEnumerator.Current => Current;
            public TEntry Current => builder._entries[_index];

            public bool MoveNext()
            {
                if (_version != builder._version)
                {
                    throw new InvalidOperationException(
                        "Collection was modified; enumeration operation may not execute"
                    );
                }
                if (_index >= builder.Count - 1)
                    return false;

                _index++;
                return true;
            }

            public void Reset()
            {
                _index = -1;
            }

            public void Dispose()
            {
                // Nothing to dispose.
            }
        }
    }
}

internal static class AssetPackageEntryList
{
    public static AssetPackageEntryList<TEntry>.Builder CreateBuilder<TEntry>()
        where TEntry : class, IAssetPackageEntry
    {
        return [];
    }

    public static AssetPackageEntryList<TEntry> Create<TEntry>(ReadOnlySpan<TEntry> entries)
        where TEntry : class, IAssetPackageEntry
    {
        return !entries.IsEmpty ? new AssetPackageEntryList<TEntry>(entries) : AssetPackageEntryList<TEntry>.Empty;
    }

    public static AssetPackageEntryList<TEntry> Create<TEntry>(IEnumerable<TEntry> entries)
        where TEntry : class, IAssetPackageEntry
    {
        if (!entries.TryGetNonEnumeratedCount(out var count))
            return new AssetPackageEntryList<TEntry>(entries);

        if (count == 0)
            return AssetPackageEntryList<TEntry>.Empty;

        var output = ArrayPool<TEntry>.Shared.Rent(count);
        try
        {
            var i = 0;
            foreach (var entry in entries)
            {
                output[i++] = entry;
            }
            return new AssetPackageEntryList<TEntry>(output.AsSpan(0, i));
        }
        finally
        {
            ArrayPool<TEntry>.Shared.Return(output, true);
        }
    }

    public static AssetPackageEntryList<TEntry> ToAssetPackageEntryList<TEntry>(this IEnumerable<TEntry> entries)
        where TEntry : class, IAssetPackageEntry
    {
        return Create(entries);
    }
}
