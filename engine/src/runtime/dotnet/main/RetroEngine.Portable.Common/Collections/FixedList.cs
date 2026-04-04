// // @file FixedList.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections;
using ZLinq;

namespace RetroEngine.Portable.Collections;

public ref struct FixedList<T> : IList<T>, IReadOnlyList<T>, IDisposable
{
    private readonly Span<T> _buffer;
    private readonly T[]? _array;

    public int Count { get; private set; }

    public bool IsReadOnly => false;

    public FixedList(Span<T> buffer)
    {
        _buffer = buffer;
        _array = null;
    }

    public FixedList(int minimumCapacity)
    {
        _array = ArrayPool<T>.Shared.Rent(minimumCapacity);
        _buffer = _array;
    }

    public T this[int index]
    {
        get => _buffer[index];
        set => _buffer[index] = value;
    }

    private void EnsureCanAdd()
    {
        if (Count == _buffer.Length)
            throw new InvalidOperationException("FixedList is full");
    }

    public void Add(T item)
    {
        EnsureCanAdd();
        _buffer[Count++] = item;
    }

    public void Clear()
    {
        for (var i = 0; i < Count; i++)
            _buffer[i] = default!;
        Count = 0;
    }

    public bool Contains(T item)
    {
        return _buffer[..Count].Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        CopyTo(array.AsSpan(arrayIndex));
    }

    public void CopyTo(scoped Span<T> span)
    {
        _buffer[..Count].CopyTo(span);
    }

    public void CopyTo(scoped Span<T> span, Index offset)
    {
        _buffer[offset..Count].CopyTo(span);
    }

    public bool Remove(T item)
    {
        var itemIndex = IndexOf(item);
        if (itemIndex == -1)
            return false;

        RemoveAt(itemIndex);
        return true;
    }

    public int IndexOf(T item)
    {
        return _buffer[..Count].IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        EnsureCanAdd();

        for (var i = Count; i > index; i--)
            _buffer[i] = _buffer[i - 1];
        _buffer[index] = item;
        Count++;
    }

    public void RemoveAt(int index)
    {
        if (index >= Count || index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        for (var i = index; i < Count - 1; i++)
            _buffer[i] = _buffer[i + 1];
        Count--;

        _buffer[Count] = default!;
    }

    public Span<T> AsSpan() => _buffer[..Count];

    public void Dispose()
    {
        if (_array is not null)
        {
            ArrayPool<T>.Shared.Return(_array);
        }
    }

    public Enumerator GetEnumerator() => new(_buffer[..Count]);

    public ValueEnumerable<Enumerator, T> AsValueEnumerable() => new(GetEnumerator());

    IEnumerator<T> IEnumerable<T>.GetEnumerator() =>
        throw new NotSupportedException("FixedList enumerator cannot be boxed");

    IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException("FixedList enumerator cannot be boxed");

    public ref struct Enumerator : IEnumerator<T>, IValueEnumerator<T>
    {
        private readonly Span<T> _buffer;
        private int _index;

        internal Enumerator(Span<T> buffer)
        {
            _buffer = buffer;
            _index = -1;
        }

        object? IEnumerator.Current => Current;
        public T Current
        {
            get
            {
                return _index >= 0
                    ? _buffer[_index]
                    : throw new InvalidOperationException("Enumerator has not been started.");
            }
        }

        public bool MoveNext()
        {
            if (_index >= _buffer.Length - 1)
                return false;

            _index++;
            return true;
        }

        public bool TryGetNext(out T current)
        {
            if (!MoveNext())
            {
                current = default!;
                return false;
            }

            current = Current;
            return true;
        }

        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = _buffer.Length;
            return true;
        }

        public bool TryGetSpan(out ReadOnlySpan<T> span)
        {
            span = _buffer;
            return true;
        }

        public bool TryCopyTo(scoped Span<T> destination, Index offset)
        {
            var destinationSpan = destination[offset..];
            if (destinationSpan.Length < _buffer.Length)
                return false;

            _buffer.CopyTo(destinationSpan);
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
