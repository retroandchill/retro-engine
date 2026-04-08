// // @file ArchiveSerializerState.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RetroEngine.Portable.Serialization.Binary;

public static class ArchiveSerializerStatePool
{
    private static readonly ConcurrentQueue<ArchiveSerializerState> Queue = new();

    public static ArchiveSerializerState Rent(ArchiveSerializerOptions? options)
    {
        if (!Queue.TryDequeue(out var state))
        {
            state = new ArchiveSerializerState();
        }

        state.Init(options);
        return state;
    }

    internal static void Return(ArchiveSerializerState state)
    {
        state.Reset();
        Queue.Enqueue(state);
    }
}

public sealed class ArchiveSerializerState : IDisposable
{
    internal static ArchiveSerializerState NullState { get; } = new(true);

    private uint _nextId;
    private readonly Dictionary<object, uint> _objectToRef;

    public ArchiveSerializerOptions Options { get; private set; }

    internal ArchiveSerializerState()
    {
        _objectToRef = new Dictionary<object, uint>(ReferenceEqualityComparer.Instance);
        Options = null!;
        _nextId = 0;
    }

    // ReSharper disable once UnusedParameter.Local
    private ArchiveSerializerState(bool _)
    {
        _objectToRef = null!;
        Options = ArchiveSerializerOptions.Default;
        _nextId = 0;
    }

    internal void Init(ArchiveSerializerOptions? options)
    {
        Options = options ?? ArchiveSerializerOptions.Default;
    }

    public void Reset()
    {
        _objectToRef.Clear();
        Options = null!;
        _nextId = 0;
    }

    public (bool Exists, uint Id) GetOrAddReference(object value)
    {
        ref var id = ref CollectionsMarshal.GetValueRefOrAddDefault(_objectToRef, value, out var exists);
        if (exists)
        {
            return (true, id);
        }

        id = _nextId++;
        return (false, id);
    }

    void IDisposable.Dispose()
    {
        ArchiveSerializerStatePool.Return(this);
    }

    private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static ReferenceEqualityComparer Instance { get; } = new();

        private ReferenceEqualityComparer() { }

        // ReSharper disable once MemberHidesStaticFromOuterClass
        public new bool Equals(object? x, object? y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(object obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
