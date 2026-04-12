// // @file InterfaceCollectionFormatters.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using MagicArchive.Utilities;

namespace MagicArchive.Formatters;

using static InterfaceCollectionFormatterUtils;

internal static class InterfaceCollectionFormatters
{
    public static readonly ImmutableDictionary<Type, Type> FormatterTypes = ImmutableDictionary.CreateRange([
        new KeyValuePair<Type, Type>(typeof(IEnumerable<>), typeof(InterfaceEnumerableFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(ICollection<>), typeof(InterfaceCollectionFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(IReadOnlyCollection<>), typeof(InterfaceReadOnlyCollectionFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(IList<>), typeof(InterfaceListFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(IReadOnlyList<>), typeof(InterfaceReadOnlyListFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(IDictionary<,>), typeof(InterfaceDictionaryFormatter<,>)),
        new KeyValuePair<Type, Type>(typeof(IReadOnlyDictionary<,>), typeof(InterfaceReadOnlyDictionaryFormatter<,>)),
        new KeyValuePair<Type, Type>(typeof(ILookup<,>), typeof(InterfaceLookupFormatter<,>)),
        new KeyValuePair<Type, Type>(typeof(IGrouping<,>), typeof(InterfaceGroupingFormatter<,>)),
        new KeyValuePair<Type, Type>(typeof(ISet<>), typeof(InterfaceSetFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(IReadOnlySet<>), typeof(InterfaceReadOnlySetFormatter<>)),
    ]);
}

file static class InterfaceCollectionFormatterUtils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TrySerializeOptimized<TBufferWriter, TCollection, TElement>(
        ref ArchiveWriter<TBufferWriter> writer,
        [NotNullWhen(false)] TCollection? value
    )
        where TCollection : IEnumerable<TElement>
        where TBufferWriter : IBufferWriter<byte>
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return true;
        }

        // optimize for list or array

        switch (value)
        {
            case TElement?[] array:
                writer.Write(array);
                return true;
            case List<TElement?> list:
                writer.Write(CollectionsMarshal.AsSpan(list));
                return true;
            default:

                return false;
        }
    }

    public static void SerializeCollection<TBufferWriter, TCollection, TElement>(
        ref ArchiveWriter<TBufferWriter> writer,
        TCollection? value
    )
        where TCollection : ICollection<TElement>
        where TBufferWriter : IBufferWriter<byte>
    {
        if (TrySerializeOptimized<TBufferWriter, TCollection, TElement>(ref writer, value))
            return;

        var formatter = writer.GetFormatter<TElement>();
        writer.WriteCollectionHeader(value.Count);
        foreach (var item in value)
        {
            formatter.Serialize(ref writer, in item!);
        }
    }

    public static void SerializeReadOnlyCollection<TBufferWriter, TCollection, TElement>(
        ref ArchiveWriter<TBufferWriter> writer,
        TCollection? value
    )
        where TCollection : IReadOnlyCollection<TElement>
        where TBufferWriter : IBufferWriter<byte>
    {
        if (TrySerializeOptimized<TBufferWriter, TCollection, TElement>(ref writer, value))
            return;

        var formatter = writer.GetFormatter<TElement>();
        writer.WriteCollectionHeader(value.Count);
        foreach (var item in value)
        {
            formatter.Serialize(ref writer, in item!);
        }
    }

    public static List<T?>? ReadList<T>(ref ArchiveReader reader)
    {
        var formatter = reader.GetFormatter<List<T?>>();
        List<T?>? v = null;
        formatter.Deserialize(ref reader, ref v);
        return v;
    }
}

public sealed class InterfaceEnumerableFormatter<T> : ArchiveFormatter<IEnumerable<T?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in IEnumerable<T?>? value
    )
    {
        if (TrySerializeOptimized<TBufferWriter, IEnumerable<T?>, T?>(ref writer, value))
            return;

        if (value.TryGetNonEnumeratedCount(out var count))
        {
            var formatter = writer.GetFormatter<T>();
            writer.WriteCollectionHeader(count);
            foreach (var item in value)
            {
                formatter.Serialize(ref writer, in item);
            }
        }
        else
        {
            var tempBuffer = ReusableLinkedArrayBufferWriterPool.Rent();
            try
            {
                var tempWriter = ArchiveWriter.Create(ref tempBuffer, writer.State);

                count = 0;
                var formatter = writer.GetFormatter<T?>();
                foreach (var item in value)
                {
                    count++;
                    formatter.Serialize(ref tempWriter, in item);
                }

                tempWriter.Flush();

                // write to parameter writer.
                writer.WriteCollectionHeader(count);
                tempBuffer.WriteToAndReset(ref writer);
            }
            finally
            {
                ReusableLinkedArrayBufferWriterPool.Return(tempBuffer);
            }
        }
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref IEnumerable<T?>? value)
    {
        value = reader.ReadArray<T?>();
    }
}

public sealed class InterfaceCollectionFormatter<T> : ArchiveFormatter<ICollection<T?>>
{
    static InterfaceCollectionFormatter()
    {
        if (!ArchiveFormatterRegistry.IsRegistered<List<T?>>())
        {
            ArchiveFormatterRegistry.Register(new ListFormatter<T>());
        }
    }

    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ICollection<T?>? value
    )
    {
        SerializeCollection<TBufferWriter, ICollection<T?>, T?>(ref writer, value);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref ICollection<T?>? value)
    {
        value = ReadList<T?>(ref reader);
    }
}

public sealed class InterfaceReadOnlyCollectionFormatter<T> : ArchiveFormatter<IReadOnlyCollection<T?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in IReadOnlyCollection<T?>? value
    )
    {
        SerializeReadOnlyCollection<TBufferWriter, IReadOnlyCollection<T?>, T?>(ref writer, value);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref IReadOnlyCollection<T?>? value)
    {
        value = reader.ReadArray<T>();
    }
}

public sealed class InterfaceListFormatter<T> : ArchiveFormatter<IList<T?>>
{
    static InterfaceListFormatter()
    {
        if (!ArchiveFormatterRegistry.IsRegistered<List<T?>>())
        {
            ArchiveFormatterRegistry.Register(new ListFormatter<T>());
        }
    }

    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in IList<T?>? value)
    {
        SerializeCollection<TBufferWriter, IList<T?>, T?>(ref writer, value);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref IList<T?>? value)
    {
        value = ReadList<T?>(ref reader);
    }
}

public sealed class InterfaceReadOnlyListFormatter<T> : ArchiveFormatter<IReadOnlyList<T?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in IReadOnlyList<T?>? value
    )
    {
        SerializeReadOnlyCollection<TBufferWriter, IReadOnlyList<T?>, T?>(ref writer, value);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref IReadOnlyList<T?>? value)
    {
        value = reader.ReadArray<T>();
    }
}

public sealed class InterfaceDictionaryFormatter<TKey, TValue>(IEqualityComparer<TKey>? comparer)
    : ArchiveFormatter<IDictionary<TKey, TValue?>>
    where TKey : notnull
{
    [UsedImplicitly]
    public InterfaceDictionaryFormatter()
        : this(null) { }

    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in IDictionary<TKey, TValue?>? value
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

    public override void Deserialize(ref ArchiveReader reader, scoped ref IDictionary<TKey, TValue?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        var dict = new Dictionary<TKey, TValue?>(comparer);

        var keyFormatter = reader.GetFormatter<TKey>();
        var valueFormatter = reader.GetFormatter<TValue>();
        for (var i = 0; i < length; i++)
        {
            KeyValuePairFormatter.Deserialize(keyFormatter, valueFormatter, ref reader, out var k, out var v);
            dict.Add(k!, v);
        }

        value = dict;
    }
}

public sealed class InterfaceReadOnlyDictionaryFormatter<TKey, TValue>(IEqualityComparer<TKey>? comparer)
    : ArchiveFormatter<IReadOnlyDictionary<TKey, TValue?>>
    where TKey : notnull
{
    [UsedImplicitly]
    public InterfaceReadOnlyDictionaryFormatter()
        : this(null) { }

    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in IReadOnlyDictionary<TKey, TValue?>? value
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

    public override void Deserialize(ref ArchiveReader reader, scoped ref IReadOnlyDictionary<TKey, TValue?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        var dict = new Dictionary<TKey, TValue?>(length, comparer);

        var keyFormatter = reader.GetFormatter<TKey>();
        var valueFormatter = reader.GetFormatter<TValue>();
        for (var i = 0; i < length; i++)
        {
            KeyValuePairFormatter.Deserialize(keyFormatter, valueFormatter, ref reader, out var k, out var v);
            dict.Add(k!, v);
        }

        value = dict;
    }
}

public sealed class InterfaceLookupFormatter<TKey, TElement>(IEqualityComparer<TKey>? equalityComparer)
    : ArchiveFormatter<ILookup<TKey, TElement>>
    where TKey : notnull
{
    static InterfaceLookupFormatter()
    {
        if (!ArchiveFormatterRegistry.IsRegistered<IGrouping<TKey, TElement>>())
        {
            ArchiveFormatterRegistry.Register(new InterfaceGroupingFormatter<TKey, TElement>());
        }
    }

    public InterfaceLookupFormatter()
        : this(null) { }

    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ILookup<TKey, TElement>? value
    )
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var formatter = writer.GetFormatter<IGrouping<TKey, TElement>>();
        writer.WriteCollectionHeader(value.Count);
        foreach (var item in value)
        {
            formatter.Serialize(ref writer, in item);
        }
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref ILookup<TKey, TElement>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        var dict = new Dictionary<TKey, IGrouping<TKey, TElement>>(equalityComparer);

        var formatter = reader.GetFormatter<IGrouping<TKey, TElement>>();
        for (var i = 0; i < length; i++)
        {
            IGrouping<TKey, TElement>? item = null;
            formatter.Deserialize(ref reader, ref item);
            if (item != null)
            {
                dict.Add(item.Key, item);
            }
        }

        value = new Lookup<TKey, TElement>(dict);
    }
}

public sealed class InterfaceGroupingFormatter<TKey, TElement> : ArchiveFormatter<IGrouping<TKey, TElement>>
    where TKey : notnull
{
    static InterfaceGroupingFormatter()
    {
        if (!ArchiveFormatterRegistry.IsRegistered<IEnumerable<TElement>>())
        {
            ArchiveFormatterRegistry.Register(new InterfaceEnumerableFormatter<TElement>());
        }
    }

    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in IGrouping<TKey, TElement>? value
    )
    {
        if (value is null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(2);
        writer.Write(value.Key);
        writer.Write<IEnumerable<TElement>>(value);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref IGrouping<TKey, TElement>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 2)
            ArchiveSerializationException.ThrowInvalidPropertyCount(2, count);

        var key = reader.Read<TKey>();
        var values = reader.ReadArray<TElement>();

        if (key is null)
            ArchiveSerializationException.ThrowDeserializeObjectIsNull(nameof(key));
        if (values is null)
            ArchiveSerializationException.ThrowDeserializeObjectIsNull(nameof(values));

        value = new Grouping<TKey, TElement>(key, values!);
    }
}

public sealed class InterfaceSetFormatter<T>(IEqualityComparer<T?>? equalityComparer) : ArchiveFormatter<ISet<T?>>
{
    static InterfaceSetFormatter()
    {
        if (!ArchiveFormatterRegistry.IsRegistered<HashSet<T>>())
        {
            ArchiveFormatterRegistry.Register(new HashSetFormatter<T>());
        }
    }

    public InterfaceSetFormatter()
        : this(null) { }

    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in ISet<T?>? value)
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

    public override void Deserialize(ref ArchiveReader reader, scoped ref ISet<T?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        var set = new HashSet<T?>(length, equalityComparer);

        var formatter = reader.GetFormatter<T>();
        for (var i = 0; i < length; i++)
        {
            T? item = default;
            formatter.Deserialize(ref reader, ref item);
            set.Add(item);
        }

        value = set;
    }
}

public sealed class InterfaceReadOnlySetFormatter<T>(IEqualityComparer<T?>? equalityComparer)
    : ArchiveFormatter<IReadOnlySet<T?>>
{
    static InterfaceReadOnlySetFormatter()
    {
        if (!ArchiveFormatterRegistry.IsRegistered<HashSet<T>>())
        {
            ArchiveFormatterRegistry.Register(new HashSetFormatter<T>());
        }
    }

    public InterfaceReadOnlySetFormatter()
        : this(null) { }

    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in IReadOnlySet<T?>? value
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

    public override void Deserialize(ref ArchiveReader reader, scoped ref IReadOnlySet<T?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        var set = new HashSet<T?>(length, equalityComparer);

        var formatter = reader.GetFormatter<T>();
        for (var i = 0; i < length; i++)
        {
            T? item = default;
            formatter.Deserialize(ref reader, ref item);
            set.Add(item);
        }

        value = set;
    }
}

internal sealed class Grouping<TKey, TElement>(TKey key, IEnumerable<TElement> elements) : IGrouping<TKey, TElement>
{
    public TKey Key { get; } = key;

    public IEnumerator<TElement> GetEnumerator()
    {
        return elements.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return elements.GetEnumerator();
    }
}

internal sealed class Lookup<TKey, TElement>(Dictionary<TKey, IGrouping<TKey, TElement>> groupings)
    : ILookup<TKey, TElement>
    where TKey : notnull
{
    public int Count => groupings.Count;

    public IEnumerable<TElement> this[TKey key] => groupings.TryGetValue(key, out var grouping) ? grouping : [];

    public bool Contains(TKey key)
    {
        return groupings.ContainsKey(key);
    }

    public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
    {
        return groupings.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
