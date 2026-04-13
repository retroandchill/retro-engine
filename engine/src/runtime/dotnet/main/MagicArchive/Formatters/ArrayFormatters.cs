// // @file ArrayFormatter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Immutable;

namespace MagicArchive.Formatters;

internal static class ArrayLikeFormatters
{
    public static readonly ImmutableDictionary<Type, Type> FormatterTypes = ImmutableDictionary.CreateRange([
        new KeyValuePair<Type, Type>(typeof(ArraySegment<>), typeof(ArraySegmentFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(Memory<>), typeof(MemoryFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(ReadOnlyMemory<>), typeof(ReadOnlyMemoryFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(ReadOnlySequence<>), typeof(ReadOnlySequenceFormatter<>)),
    ]);
}

public sealed class ArrayFormatter<T> : ArchiveFormatter<T?[]>
{
    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in T?[]? value)
    {
        writer.WriteArray(value);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref T?[]? value)
    {
        reader.ReadArray(ref value);
    }
}

public sealed class ArraySegmentFormatter<T> : ArchiveFormatter<ArraySegment<T?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ArraySegment<T?> value
    )
    {
        writer.WriteSpan(value.AsMemory().Span);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref ArraySegment<T?> value)
    {
        var array = reader.ReadArray<T>();
        value = array is not null ? new ArraySegment<T?>(array) : default;
    }
}

public sealed class MemoryFormatter<T> : ArchiveFormatter<Memory<T?>>
{
    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in Memory<T?> value)
    {
        writer.WriteSpan(value.Span);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref Memory<T?> value)
    {
        value = reader.ReadArray<T>();
    }
}

public sealed class ReadOnlyMemoryFormatter<T> : ArchiveFormatter<ReadOnlyMemory<T?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ReadOnlyMemory<T?> value
    )
    {
        writer.WriteSpan(value.Span);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref ReadOnlyMemory<T?> value)
    {
        value = reader.ReadArray<T>();
    }
}

public sealed class ReadOnlySequenceFormatter<T> : ArchiveFormatter<ReadOnlySequence<T?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ReadOnlySequence<T?> value
    )
    {
        if (value.IsSingleSegment)
        {
            writer.WriteSpan(value.First.Span);
            return;
        }

        writer.WriteCollectionHeader(checked((int)value.Length));
        foreach (var memory in value)
        {
            writer.WriteSpanWithoutHeader(memory.Span);
        }
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref ReadOnlySequence<T?> value)
    {
        var array = reader.ReadArray<T>();
        value = array is not null ? new ReadOnlySequence<T?>(array) : default;
    }
}
