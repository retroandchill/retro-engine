// // @file ImmutableOrderedDictionary.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace RetroEngine.Portable.Collections.Immutable;

public class ImmutableOrderedDictionary<TKey, TValue>
    : IImmutableDictionary<TKey, TValue>,
        IImmutableList<KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    private readonly ImmutableDictionary<TKey, TValue> _dictionary;

    public static readonly ImmutableOrderedDictionary<TKey, TValue> Empty = new(
        ImmutableDictionary<TKey, TValue>.Empty,
        ImmutableArray<TKey>.Empty
    );

    public ImmutableArray<TKey> Keys { get; }
    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
    public IEnumerable<TValue> Values => Keys.Select(key => _dictionary[key]);

    public int Count => _dictionary.Count;
    public bool IsEmpty => _dictionary.IsEmpty;

    public TValue this[TKey key] => _dictionary[key];
    KeyValuePair<TKey, TValue> IReadOnlyList<KeyValuePair<TKey, TValue>>.this[int index] => GetAt(index);

    public IEqualityComparer<TKey> KeyComparer => _dictionary.KeyComparer;
    public IEqualityComparer<TValue> ValueComparer => _dictionary.ValueComparer;

    private ImmutableOrderedDictionary(ImmutableDictionary<TKey, TValue> dictionary, ImmutableArray<TKey> keys)
    {
        _dictionary = dictionary;
        Keys = keys;
    }

    internal ImmutableOrderedDictionary(
        IEnumerable<KeyValuePair<TKey, TValue>> items,
        IEqualityComparer<TKey>? keyComparer = null,
        IEqualityComparer<TValue>? valueComparer = null
    )
    {
        var dictionaryBuilder = ImmutableDictionary.CreateBuilder(keyComparer, valueComparer);
        var arrayBuilder = ImmutableArray.CreateBuilder<TKey>(
            items is IReadOnlyCollection<KeyValuePair<TKey, TValue>> collection ? collection.Count : 0
        );
        foreach (var item in items)
        {
            dictionaryBuilder.Add(item.Key, item.Value);
            arrayBuilder.Add(item.Key);
        }
        _dictionary = dictionaryBuilder.ToImmutable();
        Keys =
            arrayBuilder.Count == arrayBuilder.Capacity ? arrayBuilder.MoveToImmutable() : arrayBuilder.ToImmutable();
    }

    internal ImmutableOrderedDictionary(
        ReadOnlySpan<KeyValuePair<TKey, TValue>> items,
        IEqualityComparer<TKey>? keyComparer = null,
        IEqualityComparer<TValue>? valueComparer = null
    )
    {
        var dictionaryBuilder = ImmutableDictionary.CreateBuilder(keyComparer, valueComparer);
        var arrayBuilder = ImmutableArray.CreateBuilder<TKey>(items.Length);
        foreach (var item in items)
        {
            dictionaryBuilder.Add(item.Key, item.Value);
            arrayBuilder.Add(item.Key);
        }
        _dictionary = dictionaryBuilder.ToImmutable();
        Keys =
            arrayBuilder.Count == arrayBuilder.Capacity ? arrayBuilder.MoveToImmutable() : arrayBuilder.ToImmutable();
    }

    internal ImmutableOrderedDictionary(
        IEqualityComparer<TKey>? keyComparer = null,
        IEqualityComparer<TValue>? valueComparer = null
    )
    {
        _dictionary = ImmutableDictionary.Create(keyComparer, valueComparer);
        Keys = ImmutableArray<TKey>.Empty;
    }

    public bool Contains(KeyValuePair<TKey, TValue> pair) => _dictionary.Contains(pair);

    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

    public bool ContainsValue(TValue value) => _dictionary.ContainsValue(value);

    public KeyValuePair<TKey, TValue> GetAt(int index)
    {
        var key = Keys[index];
        return new KeyValuePair<TKey, TValue>(key, _dictionary[key]);
    }

    public bool TryGetKey(TKey equalKey, out TKey actualKey) => _dictionary.TryGetKey(equalKey, out actualKey);

    public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value)
    {
        return _dictionary.TryGetValue(key, out value);
    }

    public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value, out int index)
    {
        var keyIndex = Keys.IndexOf(key);
        if (keyIndex == -1)
        {
            value = default;
            index = -1;
            return false;
        }

        value = _dictionary[key]!;
        index = keyIndex;
        return true;
    }

    public int IndexOf(TKey key) => Keys.IndexOf(key);

    public int IndexOf(
        KeyValuePair<TKey, TValue> item,
        int index,
        int count,
        IEqualityComparer<KeyValuePair<TKey, TValue>>? equalityComparer
    )
    {
        for (var i = index; i < index + count; i++)
        {
            if (i >= Count)
                return -1;

            if (CompareEqual(item, GetAt(i), equalityComparer))
                return i;
        }

        return -1;
    }

    public int LastIndexOf(TKey key) => Keys.LastIndexOf(key);

    public int LastIndexOf(
        KeyValuePair<TKey, TValue> item,
        int index,
        int count,
        IEqualityComparer<KeyValuePair<TKey, TValue>>? equalityComparer
    )
    {
        for (var i = index + count - 1; i >= index; i--)
        {
            if (i < 0)
                return -1;

            if (CompareEqual(item, GetAt(i), equalityComparer))
                return i;
        }

        return -1;
    }

    private bool CompareEqual(
        KeyValuePair<TKey, TValue> item1,
        KeyValuePair<TKey, TValue> item2,
        IEqualityComparer<KeyValuePair<TKey, TValue>>? equalityComparer
    )
    {
        if (equalityComparer is not null)
        {
            return equalityComparer.Equals(item1, item2);
        }

        return _dictionary.KeyComparer.Equals(item1.Key, item2.Key)
            && _dictionary.ValueComparer.Equals(item1.Value, item2.Value);
    }

    public Enumerator GetEnumerator() => new(this);

    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public ImmutableOrderedDictionary<TKey, TValue> SetItem(TKey key, TValue value)
    {
        var newDictionary = _dictionary.SetItem(key, value);
        if (ReferenceEquals(_dictionary, newDictionary))
        {
            return this;
        }

        return _dictionary.ContainsKey(key)
            ? new ImmutableOrderedDictionary<TKey, TValue>(newDictionary, Keys)
            : new ImmutableOrderedDictionary<TKey, TValue>(newDictionary, Keys.Add(key));
    }

    IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.SetItem(TKey key, TValue value)
    {
        return SetItem(key, value);
    }

    IImmutableList<KeyValuePair<TKey, TValue>> IImmutableList<KeyValuePair<TKey, TValue>>.SetItem(
        int index,
        KeyValuePair<TKey, TValue> item
    )
    {
        return SetAt(index, item.Key, item.Value);
    }

    public ImmutableOrderedDictionary<TKey, TValue> SetItems(IEnumerable<KeyValuePair<TKey, TValue>> values)
    {
        var collection = values as IReadOnlyCollection<KeyValuePair<TKey, TValue>> ?? values.ToArray();
        var newDictionary = _dictionary.SetItems(collection);
        if (ReferenceEquals(_dictionary, newDictionary))
        {
            return this;
        }

        return new ImmutableOrderedDictionary<TKey, TValue>(
            newDictionary,
            Keys.AddRange(collection.Select(x => x.Key).Where(x => !_dictionary.ContainsKey(x)))
        );
    }

    IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.SetItems(
        IEnumerable<KeyValuePair<TKey, TValue>> items
    )
    {
        return SetItems(items);
    }

    public ImmutableOrderedDictionary<TKey, TValue> SetAt(int index, TValue value)
    {
        var key = Keys[index];
        return new ImmutableOrderedDictionary<TKey, TValue>(_dictionary.SetItem(key, value), Keys);
    }

    public ImmutableOrderedDictionary<TKey, TValue> SetAt(int index, TKey key, TValue value)
    {
        var existingKey = Keys[index];
        if (KeyComparer.Equals(existingKey, key))
        {
            return SetAt(index, value);
        }

        var builder = _dictionary.ToBuilder();
        builder.Remove(existingKey);
        builder.Add(key, value);

        return new ImmutableOrderedDictionary<TKey, TValue>(builder.ToImmutable(), Keys.SetItem(index, key));
    }

    public IImmutableList<KeyValuePair<TKey, TValue>> Replace(
        KeyValuePair<TKey, TValue> oldValue,
        KeyValuePair<TKey, TValue> newValue,
        IEqualityComparer<KeyValuePair<TKey, TValue>>? equalityComparer
    )
    {
        var indexOfOld = IndexOf(oldValue, 0, Count, equalityComparer);
        return indexOfOld == -1 ? this : SetAt(indexOfOld, newValue.Key, newValue.Value);
    }

    public ImmutableOrderedDictionary<TKey, TValue> Add(TKey key, TValue value)
    {
        var newDictionary = _dictionary.Add(key, value);
        return ReferenceEquals(_dictionary, newDictionary)
            ? this
            : new ImmutableOrderedDictionary<TKey, TValue>(newDictionary, Keys.Add(key));
    }

    IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Add(TKey key, TValue value)
    {
        return Add(key, value);
    }

    IImmutableList<KeyValuePair<TKey, TValue>> IImmutableList<KeyValuePair<TKey, TValue>>.Add(
        KeyValuePair<TKey, TValue> item
    )
    {
        return Add(item.Key, item.Value);
    }

    public ImmutableOrderedDictionary<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
    {
        var itemsCollection = items as IReadOnlyCollection<KeyValuePair<TKey, TValue>> ?? items.ToArray();
        var newDictionary = _dictionary.AddRange(itemsCollection);
        if (ReferenceEquals(_dictionary, newDictionary))
        {
            return this;
        }

        return new ImmutableOrderedDictionary<TKey, TValue>(
            newDictionary,
            Keys.AddRange(itemsCollection.Select(x => x.Key).Where(x => !_dictionary.ContainsKey(x)))
        );
    }

    IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.AddRange(
        IEnumerable<KeyValuePair<TKey, TValue>> items
    )
    {
        return AddRange(items);
    }

    IImmutableList<KeyValuePair<TKey, TValue>> IImmutableList<KeyValuePair<TKey, TValue>>.AddRange(
        IEnumerable<KeyValuePair<TKey, TValue>> items
    )
    {
        return AddRange(items);
    }

    public ImmutableOrderedDictionary<TKey, TValue> Insert(int index, TKey key, TValue value)
    {
        var newDictionary = _dictionary.Add(key, value);
        return ReferenceEquals(_dictionary, newDictionary)
            ? this
            : new ImmutableOrderedDictionary<TKey, TValue>(newDictionary, Keys.Insert(index, key));
    }

    IImmutableList<KeyValuePair<TKey, TValue>> IImmutableList<KeyValuePair<TKey, TValue>>.Insert(
        int index,
        KeyValuePair<TKey, TValue> item
    )
    {
        return Insert(index, item.Key, item.Value);
    }

    public ImmutableOrderedDictionary<TKey, TValue> InsertRange(
        int index,
        IEnumerable<KeyValuePair<TKey, TValue>> items
    )
    {
        var collection = items as IReadOnlyCollection<KeyValuePair<TKey, TValue>> ?? items.ToArray();
        var newDictionary = _dictionary.AddRange(collection);
        return ReferenceEquals(_dictionary, newDictionary)
            ? this
            : new ImmutableOrderedDictionary<TKey, TValue>(
                newDictionary,
                Keys.InsertRange(index, collection.Select(x => x.Key).Where(x => !_dictionary.ContainsKey(x)))
            );
    }

    IImmutableList<KeyValuePair<TKey, TValue>> IImmutableList<KeyValuePair<TKey, TValue>>.InsertRange(
        int index,
        IEnumerable<KeyValuePair<TKey, TValue>> items
    )
    {
        return InsertRange(index, items);
    }

    public ImmutableOrderedDictionary<TKey, TValue> Swap(int index1, int index2)
    {
        var newList = Keys.Swap(index1, index2);
        return newList == Keys ? this : new ImmutableOrderedDictionary<TKey, TValue>(_dictionary, newList);
    }

    public ImmutableOrderedDictionary<TKey, TValue> Remove(TKey key)
    {
        var newDictionary = _dictionary.Remove(key);
        return ReferenceEquals(_dictionary, newDictionary)
            ? this
            : new ImmutableOrderedDictionary<TKey, TValue>(newDictionary, Keys.Remove(key));
    }

    IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Remove(TKey key)
    {
        return Remove(key);
    }

    IImmutableList<KeyValuePair<TKey, TValue>> IImmutableList<KeyValuePair<TKey, TValue>>.Remove(
        KeyValuePair<TKey, TValue> value,
        IEqualityComparer<KeyValuePair<TKey, TValue>>? equalityComparer
    )
    {
        for (var i = 0; i < Count; i++)
        {
            if (CompareEqual(value, GetAt(i), equalityComparer))
                return RemoveAt(i);
        }

        return this;
    }

    public IImmutableList<KeyValuePair<TKey, TValue>> RemoveAll(Predicate<KeyValuePair<TKey, TValue>> match)
    {
        return RemoveRange(Keys.Where(key => match(new KeyValuePair<TKey, TValue>(key, _dictionary[key]))));
    }

    public ImmutableOrderedDictionary<TKey, TValue> RemoveAt(int index)
    {
        var key = Keys[index];
        return new ImmutableOrderedDictionary<TKey, TValue>(_dictionary.Remove(key), Keys.RemoveAt(index));
    }

    IImmutableList<KeyValuePair<TKey, TValue>> IImmutableList<KeyValuePair<TKey, TValue>>.RemoveAt(int index)
    {
        return RemoveAt(index);
    }

    public ImmutableOrderedDictionary<TKey, TValue> RemoveRange(IEnumerable<TKey> keys)
    {
        var newDictionary = _dictionary.RemoveRange(keys);
        if (ReferenceEquals(_dictionary, newDictionary))
            return this;

        return new ImmutableOrderedDictionary<TKey, TValue>(
            newDictionary,
            Keys.RemoveAll(x => !newDictionary.ContainsKey(x))
        );
    }

    IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.RemoveRange(IEnumerable<TKey> keys)
    {
        return RemoveRange(keys);
    }

    IImmutableList<KeyValuePair<TKey, TValue>> IImmutableList<KeyValuePair<TKey, TValue>>.RemoveRange(
        IEnumerable<KeyValuePair<TKey, TValue>> items,
        IEqualityComparer<KeyValuePair<TKey, TValue>>? equalityComparer
    )
    {
        return RemoveRange(
            items
                .Where(x =>
                {
                    var keyIndex = IndexOf(x.Key);
                    return keyIndex != -1 && CompareEqual(x, GetAt(keyIndex), equalityComparer);
                })
                .Select(x => x.Key)
        );
    }

    public IImmutableList<KeyValuePair<TKey, TValue>> RemoveRange(int index, int count)
    {
        return RemoveRange(Keys.Skip(index).Take(count));
    }

    public ImmutableOrderedDictionary<TKey, TValue> Clear()
    {
        return IsEmpty
            ? this
            : new ImmutableOrderedDictionary<TKey, TValue>(_dictionary.Clear(), ImmutableArray<TKey>.Empty);
    }

    IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Clear() => Clear();

    IImmutableList<KeyValuePair<TKey, TValue>> IImmutableList<KeyValuePair<TKey, TValue>>.Clear() => Clear();

    public ImmutableOrderedDictionary<TKey, TValue> WithComparers(
        IEqualityComparer<TKey>? keyComparer,
        IEqualityComparer<TValue>? valueComparer
    )
    {
        var newDictionary = _dictionary.WithComparers(keyComparer, valueComparer);
        if (ReferenceEquals(newDictionary, _dictionary))
            return this;

        if (ReferenceEquals(newDictionary.KeyComparer, _dictionary.KeyComparer))
        {
            return new ImmutableOrderedDictionary<TKey, TValue>(newDictionary, Keys);
        }

        var keyHashSet = new HashSet<TKey>(Keys.Length, newDictionary.KeyComparer);
        var keyBuilder = ImmutableArray.CreateBuilder<TKey>(Keys.Length);
        foreach (var key in Keys.Where(key => keyHashSet.Add(key)))
        {
            keyBuilder.Add(key);
        }

        return new ImmutableOrderedDictionary<TKey, TValue>(
            newDictionary,
            keyBuilder.Count == keyBuilder.Capacity ? keyBuilder.MoveToImmutable() : keyBuilder.ToImmutable()
        );
    }

    public ImmutableOrderedDictionary<TKey, TValue> WithComparers(IEqualityComparer<TKey>? keyComparer)
    {
        return WithComparers(keyComparer, _dictionary.ValueComparer);
    }

    public Builder ToBuilder() => new(this, _dictionary.KeyComparer, _dictionary.ValueComparer);

    public class Builder
        : IDictionary<TKey, TValue>,
            IReadOnlyDictionary<TKey, TValue>,
            IDictionary,
            IList<KeyValuePair<TKey, TValue>>,
            IReadOnlyList<KeyValuePair<TKey, TValue>>,
            IList
    {
        private readonly OrderedDictionary<TKey, TValue> _dictionary;

        internal Builder(IEqualityComparer<TKey>? keyComparer, IEqualityComparer<TValue>? valueComparer)
        {
            _dictionary = new OrderedDictionary<TKey, TValue>(keyComparer);
            ValueComparer = valueComparer;
        }

        internal Builder(
            int initialCapacity,
            IEqualityComparer<TKey>? keyComparer,
            IEqualityComparer<TValue>? valueComparer
        )
        {
            _dictionary = new OrderedDictionary<TKey, TValue>(initialCapacity, keyComparer);
            ValueComparer = valueComparer;
        }

        internal Builder(
            IEnumerable<KeyValuePair<TKey, TValue>> items,
            IEqualityComparer<TKey>? keyComparer,
            IEqualityComparer<TValue>? valueComparer
        )
        {
            _dictionary = new OrderedDictionary<TKey, TValue>(keyComparer);
            foreach (var (key, value) in items)
            {
                _dictionary.Add(key, value);
            }
            ValueComparer = valueComparer;
        }

        public int Capacity => _dictionary.Capacity;
        public IEqualityComparer<TKey> Comparer => _dictionary.Comparer;
        public int Count => _dictionary.Count;
        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set => _dictionary[key] = value;
        }

        public IEqualityComparer<TValue>? ValueComparer { get; set; }

        KeyValuePair<TKey, TValue> IList<KeyValuePair<TKey, TValue>>.this[int index]
        {
            get => ((IList<KeyValuePair<TKey, TValue>>)_dictionary)[index];
            set => ((IList<KeyValuePair<TKey, TValue>>)_dictionary)[index] = value;
        }
        KeyValuePair<TKey, TValue> IReadOnlyList<KeyValuePair<TKey, TValue>>.this[int index] =>
            ((IReadOnlyList<KeyValuePair<TKey, TValue>>)_dictionary)[index];

        object? IDictionary.this[object key]
        {
            get => ((IDictionary)_dictionary)[key];
            set => ((IDictionary)_dictionary)[key] = value;
        }
        object? IList.this[int index]
        {
            get => ((IList)_dictionary)[index];
            set => ((IList)_dictionary)[index] = value;
        }

        public ICollection<TKey> Keys => _dictionary.Keys;
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _dictionary.Keys;
        ICollection IDictionary.Keys => _dictionary.Keys;
        public ICollection<TValue> Values => _dictionary.Values;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _dictionary.Values;
        ICollection IDictionary.Values => _dictionary.Values;
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly =>
            ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).IsReadOnly;
        bool IDictionary.IsReadOnly => ((IDictionary)_dictionary).IsReadOnly;
        bool IList.IsReadOnly => ((IList)_dictionary).IsReadOnly;
        bool IDictionary.IsFixedSize => ((IDictionary)_dictionary).IsFixedSize;
        bool IList.IsFixedSize => ((IList)_dictionary).IsFixedSize;
        bool ICollection.IsSynchronized => ((ICollection)_dictionary).IsSynchronized;
        object ICollection.SyncRoot => ((ICollection)_dictionary).SyncRoot;

        public KeyValuePair<TKey, TValue> GetAt(int index) => _dictionary.GetAt(index);

        public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

#if NET10_0_OR_GREATER
        public bool TryGetValue(TKey key, [NotNullWhen(true)] out TValue? value, out int index)
        {
            return _dictionary.TryGetValue(key, out value, out index);
        }
#endif

        public int IndexOf(TKey key) => _dictionary.IndexOf(key);

        int IList<KeyValuePair<TKey, TValue>>.IndexOf(KeyValuePair<TKey, TValue> item)
        {
            // ReSharper disable once UsageOfDefaultStructEquality
            return ((IList<KeyValuePair<TKey, TValue>>)_dictionary).IndexOf(item);
        }

        int IList.IndexOf(object? value)
        {
            return ((IList)_dictionary).IndexOf(value);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool ContainsValue(TValue value)
        {
            return _dictionary.ContainsValue(value);
        }

        bool IDictionary.Contains(object key)
        {
            return ((IDictionary)_dictionary).Contains(key);
        }

        bool IList.Contains(object? value)
        {
            return ((IList)_dictionary).Contains(value);
        }

        public int EnsureCapacity(int capacity)
        {
            return _dictionary.EnsureCapacity(capacity);
        }

        public void SetAt(int index, TKey key, TValue value)
        {
            _dictionary.SetAt(index, key, value);
        }

        public void SetAt(int index, TValue value)
        {
            _dictionary.SetAt(index, value);
        }

        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Add(item);
        }

        int IList.Add(object? value)
        {
            return ((IList)_dictionary).Add(value);
        }

        void IDictionary.Add(object key, object? value)
        {
            ((IDictionary)_dictionary).Add(key, value);
        }

        public bool TryAdd(TKey key, TValue value)
        {
            return _dictionary.TryAdd(key, value);
        }

#if NET10_0_OR_GREATER
        public bool TryAdd(TKey key, TValue value, out int index)
        {
            return _dictionary.TryAdd(key, value, out index);
        }
#endif

        public void Insert(int index, TKey key, TValue value)
        {
            _dictionary.Insert(index, key, value);
        }

        void IList<KeyValuePair<TKey, TValue>>.Insert(int index, KeyValuePair<TKey, TValue> item)
        {
            ((IList<KeyValuePair<TKey, TValue>>)_dictionary).Insert(index, item);
        }

        void IList.Insert(int index, object? value)
        {
            ((IList)_dictionary).Insert(index, value);
        }

        public bool Remove(TKey key)
        {
            return _dictionary.Remove(key);
        }

        public bool Remove(TKey key, [NotNullWhen(true)] out TValue? value)
        {
            return _dictionary.Remove(key, out value);
        }

        void IList.Remove(object? value)
        {
            ((IList)_dictionary).Remove(value);
        }

        void IDictionary.Remove(object key)
        {
            ((IDictionary)_dictionary).Remove(key);
        }

        public void RemoveAt(int index)
        {
            _dictionary.RemoveAt(index);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Remove(item);
        }

        public void Clear()
        {
            _dictionary.Clear();
        }

        public void TrimExcess()
        {
            _dictionary.TrimExcess();
        }

        public void TrimExcess(int capacity)
        {
            _dictionary.TrimExcess(capacity);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);
        }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            ((ICollection)_dictionary).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IDictionaryEnumerator IDictionary.GetEnumerator() => _dictionary.GetEnumerator();

        public ImmutableOrderedDictionary<TKey, TValue> ToImmutable()
        {
            return new ImmutableOrderedDictionary<TKey, TValue>(this, _dictionary.Comparer, ValueComparer);
        }
    }

    public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        private readonly ImmutableOrderedDictionary<TKey, TValue> _dictionary;
        private int _index = -1;

        internal Enumerator(ImmutableOrderedDictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary;
        }

        public KeyValuePair<TKey, TValue> Current => _dictionary.GetAt(_index);
        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (++_index >= _dictionary.Count)
                return false;

            return true;
        }

        public void Reset()
        {
            _index = -1;
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}

public static class ImmutableOrderedDictionary
{
    public static ImmutableOrderedDictionary<TKey, TValue> Create<TKey, TValue>()
        where TKey : notnull
    {
        return ImmutableOrderedDictionary<TKey, TValue>.Empty;
    }

    public static ImmutableOrderedDictionary<TKey, TValue> Create<TKey, TValue>(
        IEqualityComparer<TKey>? keyComparer,
        IEqualityComparer<TValue>? valueComparer = null
    )
        where TKey : notnull
    {
        return new ImmutableOrderedDictionary<TKey, TValue>(keyComparer, valueComparer);
    }

    public static ImmutableOrderedDictionary<TKey, TValue> CreateRange<TKey, TValue>(
        IEnumerable<KeyValuePair<TKey, TValue>> items
    )
        where TKey : notnull
    {
        return new ImmutableOrderedDictionary<TKey, TValue>(items);
    }

    public static ImmutableOrderedDictionary<TKey, TValue> CreateRange<TKey, TValue>(
        IEqualityComparer<TKey>? keyComparer,
        IEnumerable<KeyValuePair<TKey, TValue>> items
    )
        where TKey : notnull
    {
        return new ImmutableOrderedDictionary<TKey, TValue>(items, keyComparer);
    }

    public static ImmutableOrderedDictionary<TKey, TValue> CreateRange<TKey, TValue>(
        IEqualityComparer<TKey>? keyComparer,
        IEqualityComparer<TValue>? valueComparer,
        IEnumerable<KeyValuePair<TKey, TValue>> items
    )
        where TKey : notnull
    {
        return new ImmutableOrderedDictionary<TKey, TValue>(items, keyComparer, valueComparer);
    }

    public static ImmutableOrderedDictionary<TKey, TValue> CreateRange<TKey, TValue>(
        params ReadOnlySpan<KeyValuePair<TKey, TValue>> items
    )
        where TKey : notnull
    {
        return new ImmutableOrderedDictionary<TKey, TValue>(items);
    }

    public static ImmutableOrderedDictionary<TKey, TValue> CreateRange<TKey, TValue>(
        IEqualityComparer<TKey>? keyComparer,
        params ReadOnlySpan<KeyValuePair<TKey, TValue>> items
    )
        where TKey : notnull
    {
        return new ImmutableOrderedDictionary<TKey, TValue>(items, keyComparer);
    }

    public static ImmutableOrderedDictionary<TKey, TValue> CreateRange<TKey, TValue>(
        IEqualityComparer<TKey>? keyComparer,
        IEqualityComparer<TValue>? valueComparer,
        params ReadOnlySpan<KeyValuePair<TKey, TValue>> items
    )
        where TKey : notnull
    {
        return new ImmutableOrderedDictionary<TKey, TValue>(items, keyComparer, valueComparer);
    }

    public static ImmutableOrderedDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>(
        IEqualityComparer<TKey>? keyComparer = null,
        IEqualityComparer<TValue>? valueComparer = null
    )
        where TKey : notnull
    {
        return new ImmutableOrderedDictionary<TKey, TValue>.Builder(keyComparer, valueComparer);
    }

    public static ImmutableOrderedDictionary<TKey, TValue>.Builder CreateBuilder<TKey, TValue>(
        int initialCapacity,
        IEqualityComparer<TKey>? keyComparer = null,
        IEqualityComparer<TValue>? valueComparer = null
    )
        where TKey : notnull
    {
        return new ImmutableOrderedDictionary<TKey, TValue>.Builder(initialCapacity, keyComparer, valueComparer);
    }

    public static ImmutableOrderedDictionary<TKey, TValue> ToImmutableOrderedDictionary<TKey, TValue>(
        this IEnumerable<KeyValuePair<TKey, TValue>> items,
        IEqualityComparer<TKey>? keyComparer = null,
        IEqualityComparer<TValue>? valueComparer = null
    )
        where TKey : notnull
    {
        return new ImmutableOrderedDictionary<TKey, TValue>(items, keyComparer, valueComparer);
    }

    extension<TSource>(IEnumerable<TSource> items)
    {
        public ImmutableOrderedDictionary<TKey, TSource> ToImmutableOrderedDictionary<TKey>(
            Func<TSource, TKey> keyMapper,
            IEqualityComparer<TKey>? keyComparer = null,
            IEqualityComparer<TSource>? valueComparer = null
        )
            where TKey : notnull
        {
            return new ImmutableOrderedDictionary<TKey, TSource>(
                items.Select(x => new KeyValuePair<TKey, TSource>(keyMapper(x), x)),
                keyComparer,
                valueComparer
            );
        }

        public ImmutableOrderedDictionary<TKey, TValue> ToImmutableOrderedDictionary<TKey, TValue>(
            Func<TSource, TKey> keyMapper,
            Func<TSource, TValue> valueMapper,
            IEqualityComparer<TKey>? keyComparer = null,
            IEqualityComparer<TValue>? valueComparer = null
        )
            where TKey : notnull
        {
            return new ImmutableOrderedDictionary<TKey, TValue>(
                items.Select(x => new KeyValuePair<TKey, TValue>(keyMapper(x), valueMapper(x))),
                keyComparer,
                valueComparer
            );
        }
    }
}
