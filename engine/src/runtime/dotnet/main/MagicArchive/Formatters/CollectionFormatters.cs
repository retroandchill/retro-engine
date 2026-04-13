// // @file CollectionsFormatters.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
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
        new KeyValuePair<Type, Type>(typeof(PriorityQueue<,>), typeof(PriorityQueueFormatter<,>)),
        new KeyValuePair<Type, Type>(typeof(ObservableCollection<>), typeof(ObservableCollectionFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(Collection<>), typeof(CollectionFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(ConcurrentQueue<>), typeof(ConcurrentQueueFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(ConcurrentStack<>), typeof(ConcurrentStackFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(ConcurrentBag<>), typeof(ConcurrentBagFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(Dictionary<,>), typeof(DictionaryFormatter<,>)),
        new KeyValuePair<Type, Type>(typeof(SortedDictionary<,>), typeof(SortedDictionaryFormatter<,>)),
        new KeyValuePair<Type, Type>(typeof(SortedList<,>), typeof(SortedListFormatter<,>)),
        new KeyValuePair<Type, Type>(typeof(ConcurrentDictionary<,>), typeof(ConcurrentDictionaryFormatter<,>)),
        new KeyValuePair<Type, Type>(typeof(ReadOnlyCollection<>), typeof(ReadOnlyCollectionFormatter<>)),
        new KeyValuePair<Type, Type>(
            typeof(ReadOnlyObservableCollection<>),
            typeof(ReadOnlyObservableCollectionFormatter<>)
        ),
        new KeyValuePair<Type, Type>(typeof(BlockingCollection<>), typeof(BlockingCollectionFormatter<>)),
    ]);
}

public static class ListFormatter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize<TBufferWriter, T>(ref ArchiveWriter<TBufferWriter> writer, List<T?>? value)
        where TBufferWriter : IBufferWriter<byte>
        where T : IArchivable<T>
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        writer.WriteArchivableSpan(CollectionsMarshal.AsSpan(value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<T?>? Deserialize<T>(ref ArchiveReader reader)
    {
        List<T?>? value = null;
        Deserialize(ref reader, ref value);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Deserialize<T>(ref ArchiveReader reader, ref List<T?>? value)
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

public sealed class ListFormatter<T> : ArchiveFormatter<List<T?>>
{
    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in List<T?>? value)
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        writer.WriteSpan(CollectionsMarshal.AsSpan(value));
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref List<T?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (value is null)
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

        writer.WriteSpan(CollectionsMarshalEx.AsSpan(value));
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref Stack<T?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (value is null)
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

        if (value is null)
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

        if (value is null)
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

public sealed class HashSetFormatter<T>(IEqualityComparer<T?>? comparer) : ArchiveFormatter<HashSet<T?>>
{
    [UsedImplicitly]
    public HashSetFormatter()
        : this(null) { }

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

        if (value is null)
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

public sealed class SortedSetFormatter<T>(IComparer<T?>? comparer) : ArchiveFormatter<SortedSet<T?>>
{
    [UsedImplicitly]
    public SortedSetFormatter()
        : this(null) { }

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

        if (value is null)
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
        if (value is null)
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

        if (value is null)
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

public sealed class CollectionFormatter<T> : ArchiveFormatter<Collection<T?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in Collection<T?>? value
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

    public override void Deserialize(ref ArchiveReader reader, scoped ref Collection<T?>? value)
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
        else
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

public sealed class ObservableCollectionFormatter<T> : ArchiveFormatter<ObservableCollection<T?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ObservableCollection<T?>? value
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

    public override void Deserialize(ref ArchiveReader reader, scoped ref ObservableCollection<T?>? value)
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
        else
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

public sealed class ConcurrentQueueFormatter<T> : ArchiveFormatter<ConcurrentQueue<T?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ConcurrentQueue<T?>? value
    )
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var formatter = writer.GetFormatter<T>();
        var count = value.Count;
        writer.WriteCollectionHeader(value.Count);
        var i = 0;
        foreach (var item in value)
        {
            i++;
            formatter.Serialize(ref writer, in item);
        }

        if (i != count)
            ArchiveSerializationException.ThrowInvalidConcurrrentCollectionOperation();
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref ConcurrentQueue<T?>? value)
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
        else
        {
            value.Clear();
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

public sealed class ConcurrentStackFormatter<T> : ArchiveFormatter<ConcurrentStack<T?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ConcurrentStack<T?>? value
    )
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var count = value.Count;
        var rentArray = ArrayPool<T?>.Shared.Rent(count);
        try
        {
            var i = 0;
            foreach (var item in value)
            {
                rentArray[i++] = item;
            }
            if (i != count)
                ArchiveSerializationException.ThrowInvalidConcurrrentCollectionOperation();

            var formatter = writer.GetFormatter<T>();
            writer.WriteCollectionHeader(value.Count);
            for (i -= 1; i >= 0; i--)
            {
                formatter.Serialize(ref writer, in rentArray[i]);
            }
        }
        finally
        {
            ArrayPool<T?>.Shared.Return(rentArray, clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        }
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref ConcurrentStack<T?>? value)
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
        else
        {
            value.Clear();
        }

        var formatter = reader.GetFormatter<T>();
        for (var i = 0; i < length; i++)
        {
            T? v = default;
            formatter.Deserialize(ref reader, ref v);
            value.Push(v);
        }
    }
}

public sealed class ConcurrentBagFormatter<T> : ArchiveFormatter<ConcurrentBag<T?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ConcurrentBag<T?>? value
    )
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var formatter = writer.GetFormatter<T>();
        var count = value.Count;
        writer.WriteCollectionHeader(value.Count);
        var i = 0;
        foreach (var item in value)
        {
            i++;
            formatter.Serialize(ref writer, in item);
        }

        if (i != count)
            ArchiveSerializationException.ThrowInvalidConcurrrentCollectionOperation();
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref ConcurrentBag<T?>? value)
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
        else
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

public sealed class DictionaryFormatter<TKey, TValue>(IEqualityComparer<TKey>? comparer)
    : ArchiveFormatter<Dictionary<TKey, TValue?>>
    where TKey : notnull
{
    [UsedImplicitly]
    public DictionaryFormatter()
        : this(null) { }

    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in Dictionary<TKey, TValue?>? value
    )
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var keyFormatter = writer.GetFormatter<TKey>();
        var valueFormatter = writer.GetFormatter<TValue>();

        writer.WriteCollectionHeader(value.Count);
        foreach (var item in value)
        {
            KeyValuePairFormatter.Serialize(keyFormatter, valueFormatter, ref writer, item!);
        }
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref Dictionary<TKey, TValue?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (value is null)
        {
            value = new Dictionary<TKey, TValue?>(length, comparer);
        }
        else
        {
            value.Clear();
        }

        var keyFormatter = reader.GetFormatter<TKey>();
        var valueFormatter = reader.GetFormatter<TValue>();
        for (var i = 0; i < length; i++)
        {
            KeyValuePairFormatter.Deserialize(keyFormatter, valueFormatter, ref reader, out var k, out var v);
            value.Add(k!, v);
        }
    }
}

public sealed class SortedDictionaryFormatter<TKey, TValue>(IComparer<TKey>? comparer)
    : ArchiveFormatter<SortedDictionary<TKey, TValue?>>
    where TKey : notnull
{
    [UsedImplicitly]
    public SortedDictionaryFormatter()
        : this(null) { }

    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in SortedDictionary<TKey, TValue?>? value
    )
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var keyFormatter = writer.GetFormatter<TKey>();
        var valueFormatter = writer.GetFormatter<TValue>();

        writer.WriteCollectionHeader(value.Count);
        foreach (var item in value)
        {
            KeyValuePairFormatter.Serialize(keyFormatter, valueFormatter, ref writer, item!);
        }
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref SortedDictionary<TKey, TValue?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (value is null)
        {
            value = new SortedDictionary<TKey, TValue?>(comparer);
        }
        else
        {
            value.Clear();
        }

        var keyFormatter = reader.GetFormatter<TKey>();
        var valueFormatter = reader.GetFormatter<TValue>();
        for (var i = 0; i < length; i++)
        {
            KeyValuePairFormatter.Deserialize(keyFormatter, valueFormatter, ref reader, out var k, out var v);
            value.Add(k!, v);
        }
    }
}

public sealed class SortedListFormatter<TKey, TValue>(IComparer<TKey>? comparer)
    : ArchiveFormatter<SortedList<TKey, TValue?>>
    where TKey : notnull
{
    [UsedImplicitly]
    public SortedListFormatter()
        : this(null) { }

    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in SortedList<TKey, TValue?>? value
    )
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var keyFormatter = writer.GetFormatter<TKey>();
        var valueFormatter = writer.GetFormatter<TValue>();

        writer.WriteCollectionHeader(value.Count);
        foreach (var item in value)
        {
            KeyValuePairFormatter.Serialize(keyFormatter, valueFormatter, ref writer, item!);
        }
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref SortedList<TKey, TValue?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (value is null)
        {
            value = new SortedList<TKey, TValue?>(length, comparer);
        }
        else
        {
            value.Clear();
        }

        var keyFormatter = reader.GetFormatter<TKey>();
        var valueFormatter = reader.GetFormatter<TValue>();
        for (var i = 0; i < length; i++)
        {
            KeyValuePairFormatter.Deserialize(keyFormatter, valueFormatter, ref reader, out var k, out var v);
            value.Add(k!, v);
        }
    }
}

public sealed class ConcurrentDictionaryFormatter<TKey, TValue>(IEqualityComparer<TKey>? comparer)
    : ArchiveFormatter<ConcurrentDictionary<TKey, TValue?>>
    where TKey : notnull
{
    [UsedImplicitly]
    public ConcurrentDictionaryFormatter()
        : this(null) { }

    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ConcurrentDictionary<TKey, TValue?>? value
    )
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var keyFormatter = writer.GetFormatter<TKey>();
        var valueFormatter = writer.GetFormatter<TValue>();

        var count = value.Count;
        writer.WriteCollectionHeader(value.Count);
        var i = 0;
        foreach (var item in value)
        {
            i++;
            KeyValuePairFormatter.Serialize(keyFormatter, valueFormatter, ref writer, item!);
        }

        if (i != count)
            ArchiveSerializationException.ThrowInvalidConcurrrentCollectionOperation();
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref ConcurrentDictionary<TKey, TValue?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (value is null)
        {
            value = new ConcurrentDictionary<TKey, TValue?>(comparer);
        }
        else
        {
            value.Clear();
        }

        var keyFormatter = reader.GetFormatter<TKey>();
        var valueFormatter = reader.GetFormatter<TValue>();
        for (var i = 0; i < length; i++)
        {
            KeyValuePairFormatter.Deserialize(keyFormatter, valueFormatter, ref reader, out var k, out var v);
            value.TryAdd(k!, v);
        }
    }
}

public sealed class ReadOnlyCollectionFormatter<T> : ArchiveFormatter<ReadOnlyCollection<T?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ReadOnlyCollection<T?>? value
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

    public override void Deserialize(ref ArchiveReader reader, scoped ref ReadOnlyCollection<T?>? value)
    {
        var array = reader.ReadArray<T?>();
        value = array is not null ? new ReadOnlyCollection<T?>(array) : null;
    }
}

public sealed class ReadOnlyObservableCollectionFormatter<T> : ArchiveFormatter<ReadOnlyObservableCollection<T?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ReadOnlyObservableCollection<T?>? value
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

    public override void Deserialize(ref ArchiveReader reader, scoped ref ReadOnlyObservableCollection<T?>? value)
    {
        var array = reader.ReadArray<T?>();
        value = array is not null ? new ReadOnlyObservableCollection<T?>(new ObservableCollection<T?>(array)) : null;
    }
}

public sealed class BlockingCollectionFormatter<T> : ArchiveFormatter<BlockingCollection<T?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in BlockingCollection<T?>? value
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

    public override void Deserialize(ref ArchiveReader reader, scoped ref BlockingCollection<T?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        value = new BlockingCollection<T?>();

        var formatter = reader.GetFormatter<T>();
        for (var i = 0; i < length; i++)
        {
            T? v = default;
            formatter.Deserialize(ref reader, ref v);
            value.Add(v);
        }
    }
}
