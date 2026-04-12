// // @file CollectionsFormatters.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MagicArchive.Utilities;

namespace MagicArchive.Formatters;

internal static class CollectionFormatters
{
    public static readonly ImmutableDictionary<Type, Type> FormatterTypes = ImmutableDictionary.CreateRange([
        new KeyValuePair<Type, Type>(typeof(List<>), typeof(ListFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(Stack<>), typeof(StackFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(Queue<>), typeof(QueueFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(LinkedList<>), typeof(LinkedListFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(HashSet<>), typeof(HashSetFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(SortedSet<>), typeof(SortedSetFormatter<>)),
    ]);
}

public static class ListFormatter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteArchivable<TBufferWriter, T>(this ref ArchiveWriter<TBufferWriter> writer, List<T?>? value)
        where TBufferWriter : IBufferWriter<byte>
        where T : IArchivable<T>
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        writer.WriteArchivable(CollectionsMarshal.AsSpan(value));
    }

    extension(ref ArchiveReader reader)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<T?>? ReadArchivableList<T>()
        {
            List<T?>? value = null;
            reader.ReadArchivableList(ref value);
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReadArchivableList<T>(ref List<T?>? value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (value is null)
            {
                value = [];
            }
            else if (value.Count == length)
            {
                value.Clear();
            }

            var span = CollectionsMarshalEx.CreateSpan(value, length);
            reader.ReadInto(span);
        }
    }
}

public sealed class ListFormatter<T> : ArchiveFormatter<List<T?>>
{
    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in List<T?>? value)
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        writer.Write(CollectionsMarshal.AsSpan(value));
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref List<T?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (value == null)
        {
            value = new List<T?>(length);
        }
        else if (value.Count == length)
        {
            value.Clear();
        }

        var span = CollectionsMarshalEx.CreateSpan(value, length);
        reader.ReadInto(span);
    }
}

public sealed class StackFormatter<T> : ArchiveFormatter<Stack<T?>>
{
    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in Stack<T?>? value)
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        writer.Write(CollectionsMarshalEx.AsSpan(value));
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref Stack<T?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (value == null)
        {
            value = new Stack<T?>(length);
        }
        else if (value.Count == length)
        {
            value.Clear();
        }

        var span = CollectionsMarshalEx.CreateSpan(value, length);
        reader.ReadInto(span);
    }
}

public sealed class QueueFormatter<T> : ArchiveFormatter<Queue<T?>>
{
    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in Queue<T?>? value)
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var formatter = writer.GetFormatter<T>();
        writer.WriteCollectionHeader(value.Count);
        foreach (var item in value)
        {
            formatter.Serialize(ref writer, in item);
        }
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref Queue<T?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (value == null)
        {
            value = new Queue<T?>(length);
        }
        else if (value.Count == length)
        {
            value.Clear();
            value.EnsureCapacity(length);
        }

        var formatter = reader.GetFormatter<T>();
        for (var i = 0; i < length; i++)
        {
            T? v = default;
            formatter.Deserialize(ref reader, ref v);
            value.Enqueue(v);
        }
    }
}

public sealed class LinkedListFormatter<T> : ArchiveFormatter<LinkedList<T?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in LinkedList<T?>? value
    )
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var formatter = writer.GetFormatter<T>();
        writer.WriteCollectionHeader(value.Count);
        foreach (var item in value)
        {
            formatter.Serialize(ref writer, in item);
        }
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref LinkedList<T?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (value == null)
        {
            value = new LinkedList<T?>();
        }
        else if (value.Count == length)
        {
            value.Clear();
        }

        var formatter = reader.GetFormatter<T>();
        for (var i = 0; i < length; i++)
        {
            T? v = default;
            formatter.Deserialize(ref reader, ref v);
            value.AddLast(v);
        }
    }
}

public sealed class HashSetFormatter<T>(IEqualityComparer<T?>? comparer = null) : ArchiveFormatter<HashSet<T?>>
{
    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in HashSet<T?>? value)
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var formatter = writer.GetFormatter<T>();
        writer.WriteCollectionHeader(value.Count);
        foreach (var item in value)
        {
            formatter.Serialize(ref writer, in item);
        }
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref HashSet<T?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (value == null)
        {
            value = new HashSet<T?>(length, comparer);
        }
        else if (value.Count == length)
        {
            value.Clear();
            value.EnsureCapacity(length);
        }

        var formatter = reader.GetFormatter<T>();
        for (var i = 0; i < length; i++)
        {
            T? v = default;
            formatter.Deserialize(ref reader, ref v);
            value.Add(v);
        }
    }
}

public sealed class SortedSetFormatter<T>(IComparer<T?>? comparer = null) : ArchiveFormatter<SortedSet<T?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in SortedSet<T?>? value
    )
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var formatter = writer.GetFormatter<T>();
        writer.WriteCollectionHeader(value.Count);
        foreach (var item in value)
        {
            formatter.Serialize(ref writer, in item);
        }
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref SortedSet<T?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (value == null)
        {
            value = new SortedSet<T?>(comparer);
        }
        else if (value.Count == length)
        {
            value.Clear();
        }

        var formatter = reader.GetFormatter<T>();
        for (var i = 0; i < length; i++)
        {
            T? v = default;
            formatter.Deserialize(ref reader, ref v);
            value.Add(v);
        }
    }
}

public sealed class PriorityQueueFormatter<TElement, TPriority> : ArchiveFormatter<PriorityQueue<TElement?, TPriority?>>
{
    static PriorityQueueFormatter()
    {
        if (!ArchiveFormatterRegistry.IsRegistered<(TElement?, TPriority?)>())
        {
            ArchiveFormatterRegistry.Register(new ValueTupleFormatter<TElement?, TPriority?>());
        }
    }

    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in PriorityQueue<TElement?, TPriority?>? value
    )
    {
        if (value == null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var formatter = writer.GetFormatter<(TElement?, TPriority?)>();

        writer.WriteCollectionHeader(value.Count);
        foreach (var item in value.UnorderedItems)
        {
            formatter.Serialize(ref writer, in item);
        }
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref PriorityQueue<TElement?, TPriority?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (value == null)
        {
            value = new PriorityQueue<TElement?, TPriority?>(length);
        }
        else
        {
            value.Clear();
        }

        var formatter = reader.GetFormatter<(TElement?, TPriority?)>();
        for (var i = 0; i < length; i++)
        {
            (TElement?, TPriority?) v = default;
            formatter.Deserialize(ref reader, ref v);
            value.Enqueue(v.Item1, v.Item2);
        }
    }
}
