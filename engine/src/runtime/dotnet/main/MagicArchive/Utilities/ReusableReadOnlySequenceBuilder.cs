// // @file ReusableReadOnlySequenceBuilder.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace MagicArchive.Utilities;

public static class ReusableReadOnlySequenceBuilderPool
{
    private static readonly ConcurrentQueue<ReusableReadOnlySequenceBuilder> Queue = new();

    public static ReusableReadOnlySequenceBuilder Rent()
    {
        return Queue.TryDequeue(out var builder) ? builder : new ReusableReadOnlySequenceBuilder();
    }

    public static void Return(ReusableReadOnlySequenceBuilder builder)
    {
        builder.Reset();
        Queue.Enqueue(builder);
    }
}

public sealed class ReusableReadOnlySequenceBuilder
{
    private readonly Stack<Segment> _segmentPool = new();
    private readonly List<Segment> _list = [];

    public void Add(ReadOnlyMemory<byte> buffer, bool returnToPool)
    {
        if (!_segmentPool.TryPop(out var segment))
        {
            segment = new Segment();
        }

        segment.SetBuffer(buffer, returnToPool);
        _list.Add(segment);
    }

    public bool TryGetSingleMemory(out ReadOnlyMemory<byte> memory)
    {
        if (_list.Count == 1)
        {
            memory = _list[0].Memory;
            return true;
        }
        memory = default;
        return false;
    }

    public void ReadFromStream(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(65536);
        var offset = 0;
        do
        {
            if (offset == buffer.Length)
            {
                Add(buffer, returnToPool: true);
                buffer = ArrayPool<byte>.Shared.Rent(MathEx.NewArrayCapacity(buffer.Length));
                offset = 0;
            }

            int read;
            try
            {
                read = stream.Read(buffer.AsSpan(offset, buffer.Length - offset));
            }
            catch
            {
                ArrayPool<byte>.Shared.Return(buffer);
                throw;
            }

            offset += read;

            if (read != 0)
                continue;
            Add(buffer.AsMemory(0, offset), returnToPool: true);
            break;
        } while (true);
    }

    public async ValueTask ReadFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(65536);
        var offset = 0;
        do
        {
            if (offset == buffer.Length)
            {
                Add(buffer, returnToPool: true);
                buffer = ArrayPool<byte>.Shared.Rent(MathEx.NewArrayCapacity(buffer.Length));
                offset = 0;
            }

            int read;
            try
            {
                read = await stream
                    .ReadAsync(buffer.AsMemory(offset, buffer.Length - offset), cancellationToken)
                    .ConfigureAwait(false);
            }
            catch
            {
                ArrayPool<byte>.Shared.Return(buffer);
                throw;
            }

            offset += read;

            if (read != 0)
                continue;
            Add(buffer.AsMemory(0, offset), returnToPool: true);
            break;
        } while (true);
    }

    public ReadOnlySequence<byte> Build()
    {
        switch (_list.Count)
        {
            case 0:
                return ReadOnlySequence<byte>.Empty;
            case 1:
                return new ReadOnlySequence<byte>(_list[0].Memory);
        }

        long running = 0;
        var span = CollectionsMarshal.AsSpan(_list);
        for (var i = 0; i < span.Length; i++)
        {
            var next = i < span.Length - 1 ? span[i + 1] : null;
            span[i].SetRunningIndexAndNext(running, next);
            running += span[i].Memory.Length;
        }
        var firstSegment = span[0];
        var lastSegment = span[^1];
        return new ReadOnlySequence<byte>(firstSegment, 0, lastSegment, lastSegment.Memory.Length);
    }

    public void Reset()
    {
        var span = CollectionsMarshal.AsSpan(_list);
        foreach (var item in span)
        {
            item.Reset();
            _segmentPool.Push(item);
        }
        _list.Clear();
    }

    private class Segment : ReadOnlySequenceSegment<byte>
    {
        private bool _returnToPool;

        public void SetBuffer(ReadOnlyMemory<byte> buffer, bool returnToPool)
        {
            Memory = buffer;
            this._returnToPool = returnToPool;
        }

        public void Reset()
        {
            if (_returnToPool)
            {
                if (MemoryMarshal.TryGetArray(Memory, out var segment) && segment.Array is not null)
                {
                    ArrayPool<byte>.Shared.Return(segment.Array, clearArray: false);
                }
            }
            Memory = default;
            RunningIndex = 0;
            Next = null;
        }

        public void SetRunningIndexAndNext(long runningIndex, Segment? nextSegment)
        {
            RunningIndex = runningIndex;
            Next = nextSegment;
        }
    }
}
