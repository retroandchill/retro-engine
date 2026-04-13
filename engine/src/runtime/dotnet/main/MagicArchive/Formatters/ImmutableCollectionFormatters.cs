// // @file ImmutableCollectionFormatters.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MagicArchive.Utilities;

namespace MagicArchive.Formatters;

internal sealed class ImmutableCollectionFormatters
{
    public static readonly ImmutableDictionary<Type, Type> FormatterTypes = ImmutableDictionary.CreateRange([
        new KeyValuePair<Type, Type>(typeof(ImmutableArray<>), typeof(ImmutableArrayFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(ImmutableList<>), typeof(ImmutableListFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(ImmutableQueue<>), typeof(ImmutableQueueFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(ImmutableStack<>), typeof(ImmutableStackFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(ImmutableDictionary<,>), typeof(ImmutableDictionaryFormatter<,>)),
        new KeyValuePair<Type, Type>(
            typeof(ImmutableSortedDictionary<,>),
            typeof(ImmutableSortedDictionaryFormatter<,>)
        ),
        new KeyValuePair<Type, Type>(typeof(ImmutableSortedSet<>), typeof(ImmutableSortedSetFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(ImmutableHashSet<>), typeof(ImmutableHashSetFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(IImmutableList<>), typeof(InterfaceImmutableListFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(IImmutableQueue<>), typeof(InterfaceImmutableQueueFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(IImmutableStack<>), typeof(InterfaceImmutableStackFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(IImmutableDictionary<,>), typeof(InterfaceImmutableDictionaryFormatter<,>)),
        new KeyValuePair<Type, Type>(typeof(IImmutableSet<>), typeof(InterfaceImmutableSetFormatter<>)),
    ]);
}

public sealed class ImmutableArrayFormatter<T> : ArchiveFormatter<ImmutableArray<T?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ImmutableArray<T?> value
    )
    {
        if (value.IsDefault)
        {
            writer.WriteNullCollectionHeader();
        }
        else
        {
            writer.WriteSpan(value.AsSpan());
        }
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref ImmutableArray<T?> value)
    {
        var array = reader.ReadArray<T?>();
        value = array is not null ? ImmutableCollectionsMarshal.AsImmutableArray(array) : default;
    }
}

public sealed class ImmutableListFormatter<T> : ArchiveFormatter<ImmutableList<T?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ImmutableList<T?>? value
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

    public override void Deserialize(ref ArchiveReader reader, scoped ref ImmutableList<T?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        switch (length)
        {
            case 0:
                value = ImmutableList<T?>.Empty;
                break;
            case 1:
            {
                var item = reader.ReadValue<T>();
                value = ImmutableList.Create(item);
                break;
            }
            default:
            {
                var formatter = reader.GetFormatter<T>();
                var builder = ImmutableList.CreateBuilder<T?>();
                for (var i = 0; i < length; i++)
                {
                    T? item = default;
                    formatter.Deserialize(ref reader, ref item);
                    builder.Add(item);
                }

                value = builder.ToImmutable();
                break;
            }
        }
    }
}

public sealed class ImmutableQueueFormatter<T> : ArchiveFormatter<ImmutableQueue<T?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ImmutableQueue<T?>? value
    )
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var tempBuffer = ReusableLinkedArrayBufferWriterPool.Rent();
        try
        {
            var tempWriter = ArchiveWriter.Create(ref tempBuffer, writer.State);

            var count = 0;
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

    public override void Deserialize(ref ArchiveReader reader, scoped ref ImmutableQueue<T?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        switch (length)
        {
            case 0:
                value = ImmutableQueue<T?>.Empty;
                break;
            case 1:
            {
                var item = reader.ReadValue<T>();
                value = ImmutableQueue.Create(item);
                break;
            }
            default:
            {
                var rentArray = ArrayPool<T?>.Shared.Rent(length);
                try
                {
                    var formatter = reader.GetFormatter<T?>();
                    for (var i = 0; i < length; i++)
                    {
                        formatter.Deserialize(ref reader, ref rentArray[i]);
                    }

                    // we can use T[] ctor
                    value =
                        rentArray.Length == length
                            ? ImmutableQueue.Create(rentArray)
                            :
                            // IEnumerable<T> method
                            ImmutableQueue.CreateRange((new ArraySegment<T?>(rentArray, 0, length)).AsEnumerable());
                }
                finally
                {
                    ArrayPool<T?>.Shared.Return(
                        rentArray,
                        clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>()
                    );
                }

                break;
            }
        }
    }
}

public sealed class ImmutableStackFormatter<T> : ArchiveFormatter<ImmutableStack<T?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ImmutableStack<T?>? value
    )
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var tempBuffer = ReusableLinkedArrayBufferWriterPool.Rent();
        try
        {
            var tempWriter = ArchiveWriter.Create(ref tempBuffer, writer.State);

            var count = 0;
            var formatter = writer.GetFormatter<T?>();
            foreach (var item in value.Reverse())
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

    public override void Deserialize(ref ArchiveReader reader, scoped ref ImmutableStack<T?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        switch (length)
        {
            case 0:
                value = ImmutableStack<T?>.Empty;
                break;
            case 1:
            {
                var item = reader.ReadValue<T>();
                value = ImmutableStack.Create(item);
                break;
            }
            default:
            {
                var rentArray = ArrayPool<T?>.Shared.Rent(length);
                try
                {
                    var formatter = reader.GetFormatter<T?>();
                    for (var i = 0; i < length; i++)
                    {
                        formatter.Deserialize(ref reader, ref rentArray[i]);
                    }

                    value =
                        rentArray.Length == length
                            ? ImmutableStack.Create(rentArray)
                            : ImmutableStack.CreateRange((new ArraySegment<T?>(rentArray, 0, length)).AsEnumerable());
                }
                finally
                {
                    ArrayPool<T?>.Shared.Return(
                        rentArray,
                        clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>()
                    );
                }

                break;
            }
        }
    }
}

public sealed class ImmutableDictionaryFormatter<TKey, TValue>(
    IEqualityComparer<TKey>? keyEqualityComparer,
    IEqualityComparer<TValue?>? valueEqualityComparer
) : ArchiveFormatter<ImmutableDictionary<TKey, TValue?>?>
    where TKey : notnull
{
    public ImmutableDictionaryFormatter()
        : this(null, null) { }

    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ImmutableDictionary<TKey, TValue?>? value
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

    public override void Deserialize(ref ArchiveReader reader, scoped ref ImmutableDictionary<TKey, TValue?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (length == 0)
        {
            value = ImmutableDictionary<TKey, TValue?>.Empty;
            if (keyEqualityComparer is not null || valueEqualityComparer is not null)
            {
                value = value.WithComparers(keyEqualityComparer, valueEqualityComparer);
            }
        }

        var keyFormatter = reader.GetFormatter<TKey>();
        var valueFormatter = reader.GetFormatter<TValue>();
        var builder = ImmutableDictionary.CreateBuilder(keyEqualityComparer, valueEqualityComparer);
        for (var i = 0; i < length; i++)
        {
            KeyValuePairFormatter.Deserialize(keyFormatter, valueFormatter, ref reader, out var k, out var v);
            builder.Add(k!, v);
        }

        value = builder.ToImmutable();
    }
}

public sealed class ImmutableHashSetFormatter<T>(IEqualityComparer<T?>? equalityComparer)
    : ArchiveFormatter<ImmutableHashSet<T?>>
{
    public ImmutableHashSetFormatter()
        : this(null) { }

    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ImmutableHashSet<T?>? value
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

    public override void Deserialize(ref ArchiveReader reader, scoped ref ImmutableHashSet<T?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (length == 0)
        {
            value = ImmutableHashSet<T?>.Empty;
            if (equalityComparer is not null)
            {
                value = value.WithComparer(equalityComparer);
            }
        }

        var formatter = reader.GetFormatter<T>();
        var builder = ImmutableHashSet.CreateBuilder(equalityComparer);
        for (var i = 0; i < length; i++)
        {
            T? item = default;
            formatter.Deserialize(ref reader, ref item);
            builder.Add(item);
        }

        value = builder.ToImmutable();
    }
}

public sealed class ImmutableSortedDictionaryFormatter<TKey, TValue>(
    IComparer<TKey>? keyComparer,
    IEqualityComparer<TValue?>? valueEqualityComparer
) : ArchiveFormatter<ImmutableSortedDictionary<TKey, TValue?>?>
    where TKey : notnull
{
    public ImmutableSortedDictionaryFormatter()
        : this(null, null) { }

    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ImmutableSortedDictionary<TKey, TValue?>? value
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

    public override void Deserialize(
        ref ArchiveReader reader,
        scoped ref ImmutableSortedDictionary<TKey, TValue?>? value
    )
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (length == 0)
        {
            value = ImmutableSortedDictionary<TKey, TValue?>.Empty;
            if (keyComparer is not null || valueEqualityComparer is not null)
            {
                value = value.WithComparers(keyComparer, valueEqualityComparer);
            }
        }

        var keyFormatter = reader.GetFormatter<TKey>();
        var valueFormatter = reader.GetFormatter<TValue>();
        var builder = ImmutableSortedDictionary.CreateBuilder(keyComparer, valueEqualityComparer);
        for (var i = 0; i < length; i++)
        {
            KeyValuePairFormatter.Deserialize(keyFormatter, valueFormatter, ref reader, out var k, out var v);
            builder.Add(k!, v);
        }

        value = builder.ToImmutable();
    }
}

public sealed class ImmutableSortedSetFormatter<T>(IComparer<T?>? comparer) : ArchiveFormatter<ImmutableSortedSet<T?>>
{
    public ImmutableSortedSetFormatter()
        : this(null) { }

    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ImmutableSortedSet<T?>? value
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

    public override void Deserialize(ref ArchiveReader reader, scoped ref ImmutableSortedSet<T?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (length == 0)
        {
            value = ImmutableSortedSet<T?>.Empty;
            if (comparer is not null)
            {
                value = value.WithComparer(comparer);
            }
        }

        var formatter = reader.GetFormatter<T>();
        var builder = ImmutableSortedSet.CreateBuilder(comparer);
        for (var i = 0; i < length; i++)
        {
            T? item = default;
            formatter.Deserialize(ref reader, ref item);
            builder.Add(item);
        }

        value = builder.ToImmutable();
    }
}

public sealed class InterfaceImmutableListFormatter<T> : ArchiveFormatter<IImmutableList<T?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in IImmutableList<T?>? value
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

    public override void Deserialize(ref ArchiveReader reader, scoped ref IImmutableList<T?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        switch (length)
        {
            case 0:
                value = ImmutableList<T?>.Empty;
                break;
            case 1:
            {
                var item = reader.ReadValue<T>();
                value = ImmutableList.Create(item);
                break;
            }
            default:
            {
                var formatter = reader.GetFormatter<T>();
                var builder = ImmutableList.CreateBuilder<T?>();
                for (var i = 0; i < length; i++)
                {
                    T? item = default;
                    formatter.Deserialize(ref reader, ref item);
                    builder.Add(item);
                }

                value = builder.ToImmutable();
                break;
            }
        }
    }
}

public sealed class InterfaceImmutableQueueFormatter<T> : ArchiveFormatter<IImmutableQueue<T?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in IImmutableQueue<T?>? value
    )
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var tempBuffer = ReusableLinkedArrayBufferWriterPool.Rent();
        try
        {
            var tempWriter = ArchiveWriter.Create(ref tempBuffer, writer.State);

            var count = 0;
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

    public override void Deserialize(ref ArchiveReader reader, scoped ref IImmutableQueue<T?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        switch (length)
        {
            case 0:
                value = ImmutableQueue<T?>.Empty;
                break;
            case 1:
            {
                var item = reader.ReadValue<T>();
                value = ImmutableQueue.Create(item);
                break;
            }
            default:
            {
                var rentArray = ArrayPool<T?>.Shared.Rent(length);
                try
                {
                    var formatter = reader.GetFormatter<T?>();
                    for (var i = 0; i < length; i++)
                    {
                        formatter.Deserialize(ref reader, ref rentArray[i]);
                    }

                    value =
                        rentArray.Length == length
                            ? ImmutableQueue.Create(rentArray)
                            : ImmutableQueue.CreateRange((new ArraySegment<T?>(rentArray, 0, length)).AsEnumerable());
                }
                finally
                {
                    ArrayPool<T?>.Shared.Return(
                        rentArray,
                        clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>()
                    );
                }

                break;
            }
        }
    }
}

public sealed class InterfaceImmutableStackFormatter<T> : ArchiveFormatter<IImmutableStack<T?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in IImmutableStack<T?>? value
    )
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var tempBuffer = ReusableLinkedArrayBufferWriterPool.Rent();
        try
        {
            var tempWriter = ArchiveWriter.Create(ref tempBuffer, writer.State);

            var count = 0;
            var formatter = writer.GetFormatter<T?>();
            foreach (var item in value.Reverse())
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

    public override void Deserialize(ref ArchiveReader reader, scoped ref IImmutableStack<T?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        switch (length)
        {
            case 0:
                value = ImmutableStack<T?>.Empty;
                break;
            case 1:
            {
                var item = reader.ReadValue<T>();
                value = ImmutableStack.Create(item);
                break;
            }
            default:
            {
                var rentArray = ArrayPool<T?>.Shared.Rent(length);
                try
                {
                    var formatter = reader.GetFormatter<T?>();
                    for (var i = 0; i < length; i++)
                    {
                        formatter.Deserialize(ref reader, ref rentArray[i]);
                    }

                    value =
                        rentArray.Length == length
                            ? ImmutableStack.Create(rentArray)
                            : ImmutableStack.CreateRange((new ArraySegment<T?>(rentArray, 0, length)).AsEnumerable());
                }
                finally
                {
                    ArrayPool<T?>.Shared.Return(
                        rentArray,
                        clearArray: RuntimeHelpers.IsReferenceOrContainsReferences<T>()
                    );
                }

                break;
            }
        }
    }
}

public sealed class InterfaceImmutableDictionaryFormatter<TKey, TValue>(
    IEqualityComparer<TKey>? keyEqualityComparer,
    IEqualityComparer<TValue?>? valueEqualityComparer
) : ArchiveFormatter<IImmutableDictionary<TKey, TValue?>?>
    where TKey : notnull
{
    public InterfaceImmutableDictionaryFormatter()
        : this(null, null) { }

    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in IImmutableDictionary<TKey, TValue?>? value
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

    public override void Deserialize(ref ArchiveReader reader, scoped ref IImmutableDictionary<TKey, TValue?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (length == 0)
        {
            if (keyEqualityComparer is not null || valueEqualityComparer is not null)
            {
                value = ImmutableDictionary<TKey, TValue?>.Empty.WithComparers(
                    keyEqualityComparer,
                    valueEqualityComparer
                );
            }
            else
            {
                value = ImmutableDictionary<TKey, TValue?>.Empty;
            }
        }

        var keyFormatter = reader.GetFormatter<TKey>();
        var valueFormatter = reader.GetFormatter<TValue>();
        var builder = ImmutableDictionary.CreateBuilder(keyEqualityComparer, valueEqualityComparer);
        for (var i = 0; i < length; i++)
        {
            KeyValuePairFormatter.Deserialize(keyFormatter, valueFormatter, ref reader, out var k, out var v);
            builder.Add(k!, v);
        }

        value = builder.ToImmutable();
    }
}

public sealed class InterfaceImmutableSetFormatter<T>(IEqualityComparer<T?>? equalityComparer)
    : ArchiveFormatter<IImmutableSet<T?>>
{
    public InterfaceImmutableSetFormatter()
        : this(null) { }

    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in IImmutableSet<T?>? value
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

    public override void Deserialize(ref ArchiveReader reader, scoped ref IImmutableSet<T?>? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (length == 0)
        {
            value = equalityComparer is not null
                ? ImmutableHashSet<T?>.Empty.WithComparer(equalityComparer)
                : ImmutableHashSet<T?>.Empty;
        }

        var formatter = reader.GetFormatter<T>();
        var builder = ImmutableHashSet.CreateBuilder(equalityComparer);
        for (var i = 0; i < length; i++)
        {
            T? item = default;
            formatter.Deserialize(ref reader, ref item);
            builder.Add(item);
        }

        value = builder.ToImmutable();
    }
}
