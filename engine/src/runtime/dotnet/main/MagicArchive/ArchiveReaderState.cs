// // @file ArchiveSerializerState.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace MagicArchive;

public static class ArchiveReaderStatePool
{
    private static readonly ConcurrentQueue<ArchiveReaderState> Queue = new();

    public static ArchiveReaderState Rent(ArchiveSerializerOptions? options)
    {
        if (!Queue.TryDequeue(out var state))
        {
            state = new ArchiveReaderState();
        }

        state.Init(options);
        return state;
    }

    internal static void Return(ArchiveReaderState state)
    {
        state.Reset();
        Queue.Enqueue(state);
    }
}

public sealed class ArchiveReaderState : IDisposable
{
    internal static ArchiveReaderState NullStateLittleEndian { get; } = new(ByteOrder.LittleEndian);
    internal static ArchiveReaderState NullStateBigEndian { get; } = new(ByteOrder.BigEndian);

    private readonly Dictionary<uint, object> _refToObject;

    public ArchiveSerializerOptions Options { get; private set; }

    internal ArchiveReaderState()
    {
        _refToObject = new Dictionary<uint, object>();
        Options = null!;
    }

    private ArchiveReaderState(ByteOrder byteOrder)
    {
        _refToObject = null!;
        Options = byteOrder switch
        {
            ByteOrder.LittleEndian => ArchiveSerializerOptions.LittleEndian,
            ByteOrder.BigEndian => ArchiveSerializerOptions.BigEndian,
            _ => throw new ArgumentOutOfRangeException(nameof(byteOrder), byteOrder, null),
        };
    }

    internal void Init(ArchiveSerializerOptions? options)
    {
        Options = options ?? ArchiveSerializerOptions.Default;
    }

    public object GetObjectReference(uint id)
    {
        if (_refToObject.TryGetValue(id, out var value))
        {
            return value;
        }
        ArchiveSerializationException.ThrowMessage("Object is not found in this reference id:" + id);
        return null!;
    }

    public void AddObjectReference(uint id, object value)
    {
        if (!_refToObject.TryAdd(id, value))
        {
            ArchiveSerializationException.ThrowMessage("Object is already added, id:" + id);
        }
    }

    public void Reset()
    {
        _refToObject.Clear();
        Options = null!;
    }

    void IDisposable.Dispose()
    {
        ArchiveReaderStatePool.Return(this);
    }
}
