// // @file GenericCollectionFormatter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace MagicArchive.Formatters;

public sealed class GenericCollectionFormatter<TCollection, TElement> : ArchiveFormatter<TCollection?>
    where TCollection : ICollection<TElement?>, new()
{
    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in TCollection? value)
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var formatter = writer.GetFormatter<TElement>();
        writer.WriteCollectionHeader(value.Count);

        foreach (var element in value)
        {
            formatter.Serialize(ref writer, element);
        }
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref TCollection? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = default;
            return;
        }

        var formatter = reader.GetFormatter<TElement>();

        var collection = new TCollection();
        for (var i = 0; i < length; i++)
        {
            TElement? v = default;
            formatter.Deserialize(ref reader, ref v);
            collection.Add(v);
        }

        value = collection;
    }
}

public abstract class GenericSetFormatterBase<TSet, TElement> : ArchiveFormatter<TSet?>
    where TSet : ISet<TElement?>
{
    protected abstract TSet CreateSet();

    public sealed override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in TSet? value)
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var formatter = writer.GetFormatter<TElement>();
        writer.WriteCollectionHeader(value.Count);
        foreach (var item in value)
        {
            formatter.Serialize(ref writer, item);
        }
    }

    public sealed override void Deserialize(ref ArchiveReader reader, scoped ref TSet? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = default;
            return;
        }

        var formatter = reader.GetFormatter<TElement>();
        var collection = CreateSet();

        for (var i = 0; i < length; i++)
        {
            TElement? v = default;
            formatter.Deserialize(ref reader, ref v);
            collection.Add(v);
        }

        value = collection;
    }
}

public sealed class GenericSetFormatter<TSet, TElement> : GenericSetFormatterBase<TSet, TElement>
    where TSet : ISet<TElement?>, new()
{
    protected override TSet CreateSet() => [];
}

public abstract class GenericDictionaryFormatterBase<TDictionary, TKey, TValue> : ArchiveFormatter<TDictionary?>
    where TKey : notnull
    where TDictionary : IDictionary<TKey, TValue?>
{
    protected abstract TDictionary CreateDictionary();

    public sealed override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in TDictionary? value
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

    public sealed override void Deserialize(ref ArchiveReader reader, scoped ref TDictionary? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = default;
            return;
        }

        var keyFormatter = reader.GetFormatter<TKey>();
        var valueFormatter = reader.GetFormatter<TValue>();

        var dict = CreateDictionary();
        for (var i = 0; i < length; i++)
        {
            KeyValuePairFormatter.Deserialize(keyFormatter, valueFormatter, ref reader, out var k, out var v);
            dict.Add(k!, v);
        }

        value = dict;
    }
}

public sealed class GenericDictionaryFormatter<TDictionary, TKey, TValue>
    : GenericDictionaryFormatterBase<TDictionary, TKey, TValue>
    where TKey : notnull
    where TDictionary : IDictionary<TKey, TValue?>, new()
{
    protected override TDictionary CreateDictionary() => new();
}
