// // @file ReusableLinkedArrayBufferWriter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ZLinq;

namespace MagicArchive.Utilities;

internal struct BufferSegment
{
    private byte[] _buffer;

    public bool IsNull => _buffer is null;

    public int WrittenCount { get; private set; }

    public Span<byte> WrittenBuffer => _buffer.AsSpan(0, WrittenCount);
    public Memory<byte> WrittenMemory => _buffer.AsMemory(0, WrittenCount);
    public Span<byte> FreeBuffer => _buffer.AsSpan(WrittenCount);

    public BufferSegment(int size)
    {
        _buffer = ArrayPool<byte>.Shared.Rent(size);
        WrittenCount = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        WrittenCount += count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        if (_buffer is not null)
        {
            ArrayPool<byte>.Shared.Return(_buffer);
        }
        _buffer = null!;
        WrittenCount = 0;
    }
}

public static class ReusableLinkedArrayBufferWriterPool
{
    private static readonly ConcurrentQueue<ReusableLinkedArrayBufferWriter> Queue = new();

    public static ReusableLinkedArrayBufferWriter Rent()
    {
        return Queue.TryDequeue(out var writer) ? writer : new ReusableLinkedArrayBufferWriter(false, false); // does not cache firstBuffer
    }

    public static void Return(ReusableLinkedArrayBufferWriter writer)
    {
        writer.Reset();
        Queue.Enqueue(writer);
    }
}

public sealed class ReusableLinkedArrayBufferWriter
    : IBufferWriter<byte>,
        IValueEnumerable<ReusableLinkedArrayBufferWriter.Enumerator, Memory<byte>>
{
    private const int InitialBufferSize = 262144; // 256K(32768, 65536, 131072, 262144)
    private static readonly byte[] NoUseFirstBufferSentinel = [];

    private List<BufferSegment> _buffers;
    private byte[] _firstBuffer;
    private int _firstBufferWritten;

    private BufferSegment _current;
    private int _nextBufferSize;

    private int _totalWritten;

    public int TotalWritten => _totalWritten;
    private bool UseFirstBuffer => _firstBuffer != NoUseFirstBufferSentinel;

    internal byte[] FirstBuffer => _firstBuffer;

    public ReusableLinkedArrayBufferWriter(bool useFirstBuffer, bool pinned)
    {
        _buffers = [];
        _firstBuffer = useFirstBuffer
            ? GC.AllocateUninitializedArray<byte>(InitialBufferSize, pinned)
            : NoUseFirstBufferSentinel;
        _firstBufferWritten = 0;
        _current = default;
        _nextBufferSize = InitialBufferSize;
        _totalWritten = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        if (_current.IsNull)
        {
            _firstBufferWritten += count;
        }
        else
        {
            _current.Advance(count);
        }
        _totalWritten += count;
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        throw new NotSupportedException("MagicArchive doesn't use GetMemory");
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        if (_current.IsNull)
        {
            var free = _firstBuffer.Length - _firstBufferWritten;
            if (free != 0 && sizeHint <= free)
            {
                return _firstBuffer.AsSpan(_firstBufferWritten);
            }
        }
        else
        {
            var buffer = _current.FreeBuffer;
            if (buffer.Length > sizeHint)
            {
                return buffer;
            }
        }

        BufferSegment next;
        if (sizeHint <= _nextBufferSize)
        {
            next = new BufferSegment(_nextBufferSize);
            _nextBufferSize = MathEx.NewArrayCapacity(_nextBufferSize);
        }
        else
        {
            next = new BufferSegment(sizeHint);
        }

        if (_current.WrittenCount != 0)
        {
            _buffers.Add(_current);
        }

        _current = next;
        return next.FreeBuffer;
    }

    public byte[] ToArrayAndReset()
    {
        if (_totalWritten == 0)
            return [];

        var result = GC.AllocateUninitializedArray<byte>(_totalWritten);
        var dest = result.AsSpan();
        if (UseFirstBuffer)
        {
            _firstBuffer.AsSpan(0, _firstBufferWritten).CopyTo(dest);
            dest = dest[_firstBufferWritten..];
        }

        if (_buffers.Count > 0)
        {
            foreach (ref var item in CollectionsMarshal.AsSpan(_buffers))
            {
                item.WrittenBuffer.CopyTo(dest);
                dest = dest[item.WrittenCount..];
                item.Clear();
            }
        }

        if (!_current.IsNull)
        {
            _current.WrittenBuffer.CopyTo(dest);
            _current.Clear();
        }

        ResetCore();
        return result;
    }

    public void WriteToAndReset<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer)
        where TBufferWriter : IBufferWriter<byte>
    {
        if (_totalWritten == 0)
            return;

        if (UseFirstBuffer)
        {
            ref var spanRef = ref writer.GetSpanReference(_firstBufferWritten);
            _firstBuffer
                .AsSpan(0, _firstBufferWritten)
                .CopyTo(MemoryMarshal.CreateSpan(ref spanRef, _firstBufferWritten));
            writer.Advance(_firstBufferWritten);
        }

        if (_buffers.Count > 0)
        {
            foreach (ref var item in CollectionsMarshal.AsSpan(_buffers))
            {
                ref var spanRef = ref writer.GetSpanReference(item.WrittenCount);
                item.WrittenBuffer.CopyTo(MemoryMarshal.CreateSpan(ref spanRef, item.WrittenCount));
                writer.Advance(item.WrittenCount);
                item.Clear(); // reset
            }
        }

        if (!_current.IsNull)
        {
            ref var spanRef = ref writer.GetSpanReference(_current.WrittenCount);
            _current.WrittenBuffer.CopyTo(MemoryMarshal.CreateSpan(ref spanRef, _current.WrittenCount));
            writer.Advance(_current.WrittenCount);
            _current.Clear();
        }

        ResetCore();
    }

    public async ValueTask WriteToAndResetAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        if (_totalWritten == 0)
            return;

        if (UseFirstBuffer)
        {
            await stream
                .WriteAsync(_firstBuffer.AsMemory(0, _firstBufferWritten), cancellationToken)
                .ConfigureAwait(false);
        }

        if (_buffers.Count > 0)
        {
            foreach (var item in _buffers)
            {
                await stream.WriteAsync(item.WrittenMemory, cancellationToken).ConfigureAwait(false);
                item.Clear(); // reset
            }
        }

        if (!_current.IsNull)
        {
            await stream.WriteAsync(_current.WrittenMemory, cancellationToken).ConfigureAwait(false);
            _current.Clear();
        }

        ResetCore();
    }

    public Enumerator GetEnumerator() => new(this);

    public ValueEnumerable<Enumerator, Memory<byte>> AsValueEnumerable()
    {
        return new ValueEnumerable<Enumerator, Memory<byte>>(GetEnumerator());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ResetCore()
    {
        _firstBufferWritten = 0;
        _buffers.Clear();
        _totalWritten = 0;
        _current = default;
        _nextBufferSize = InitialBufferSize;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset()
    {
        if (_totalWritten == 0)
            return;
        foreach (ref var item in CollectionsMarshal.AsSpan(_buffers))
        {
            item.Clear();
        }
        _current.Clear();
        ResetCore();
    }

    public struct Enumerator(ReusableLinkedArrayBufferWriter parent)
        : IEnumerator<Memory<byte>>,
            IValueEnumerator<Memory<byte>>
    {
        private enum State
        {
            FirstBuffer,
            BuffersInit,
            BuffersIterate,
            Current,
            End,
        }

        private State _state;
        private List<BufferSegment>.Enumerator _buffersEnumerator;

        public Memory<byte> Current { get; private set; }

        object IEnumerator.Current => throw new NotSupportedException();

        public bool MoveNext()
        {
            if (_state == State.FirstBuffer)
            {
                _state = State.BuffersInit;

                if (parent.UseFirstBuffer)
                {
                    Current = parent._firstBuffer.AsMemory(0, parent._firstBufferWritten);
                    return true;
                }
            }

            if (_state == State.BuffersInit)
            {
                _state = State.BuffersIterate;

                _buffersEnumerator = parent._buffers.GetEnumerator();
            }

            if (_state == State.BuffersIterate)
            {
                if (_buffersEnumerator.MoveNext())
                {
                    Current = _buffersEnumerator.Current.WrittenMemory;
                    return true;
                }

                _buffersEnumerator.Dispose();
                _state = State.Current;
            }

            if (_state == State.Current)
            {
                _state = State.End;

                Current = parent._current.WrittenMemory;
                return true;
            }

            return false;
        }

        public bool TryGetNext(out Memory<byte> current)
        {
            if (MoveNext())
            {
                current = Current;
                return true;
            }

            current = default;
            return false;
        }

        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = 0;
            return false;
        }

        public bool TryGetSpan(out ReadOnlySpan<Memory<byte>> span)
        {
            span = default;
            return false;
        }

        public bool TryCopyTo(scoped Span<Memory<byte>> destination, Index offset)
        {
            return false;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public void Dispose() { }
    }
}
