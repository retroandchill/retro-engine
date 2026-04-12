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
