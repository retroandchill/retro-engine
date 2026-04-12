// // @file FrozenCollectionFormatters.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Frozen;
using System.Collections.Immutable;

namespace MagicArchive.Formatters;

internal static class FrozenCollectionFormatters
{
    public static readonly ImmutableDictionary<Type, Type> FormatterTypes = ImmutableDictionary.CreateRange([
        new KeyValuePair<Type, Type>(typeof(FrozenDictionary<,>), typeof(FrozenDictionaryFormatter<,>)),
        new KeyValuePair<Type, Type>(typeof(FrozenSet<>), typeof(FrozenSetFormatter<>)),
    ]);
}

public sealed class FrozenDictionaryFormatter<TKey, TValue>(IEqualityComparer<TKey>? equalityComparer)
    : ArchiveFormatter<FrozenDictionary<TKey, TValue?>>
    where TKey : notnull
{
    public FrozenDictionaryFormatter()
        : this(null) { }

    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in FrozenDictionary<TKey, TValue?>? value
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

    public override void Deserialize(ref ArchiveReader reader, scoped ref FrozenDictionary<TKey, TValue?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        var dict = new Dictionary<TKey, TValue?>(length, equalityComparer);

        var keyFormatter = reader.GetFormatter<TKey>();
        var valueFormatter = reader.GetFormatter<TValue>();
        for (var i = 0; i < length; i++)
        {
            KeyValuePairFormatter.Deserialize(keyFormatter, valueFormatter, ref reader, out var k, out var v);
            dict.Add(k!, v);
        }
        value = dict.ToFrozenDictionary(equalityComparer);
    }
}

public sealed class FrozenSetFormatter<T>(IEqualityComparer<T?>? equalityComparer) : ArchiveFormatter<FrozenSet<T?>>
{
    public FrozenSetFormatter()
        : this(null) { }

    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in FrozenSet<T?>? value
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

    public override void Deserialize(ref ArchiveReader reader, scoped ref FrozenSet<T?>? value)
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
        value = set.ToFrozenSet(equalityComparer);
    }
}
