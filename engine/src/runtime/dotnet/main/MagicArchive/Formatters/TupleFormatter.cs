using System.Collections.Immutable;
using MagicArchive.Utilities;

namespace MagicArchive.Formatters;

internal static class TupleFormatters
{
    public static readonly ImmutableDictionary<Type, Type> FormatterTypes = ImmutableDictionary.CreateRange([
        new KeyValuePair<Type, Type>(typeof(Tuple<>), typeof(TupleFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(ValueTuple<>), typeof(ValueTupleFormatter<>)),
        new KeyValuePair<Type, Type>(typeof(Tuple<,>), typeof(TupleFormatter<,>)),
        new KeyValuePair<Type, Type>(typeof(ValueTuple<,>), typeof(ValueTupleFormatter<,>)),
        new KeyValuePair<Type, Type>(typeof(Tuple<,,>), typeof(TupleFormatter<,,>)),
        new KeyValuePair<Type, Type>(typeof(ValueTuple<,,>), typeof(ValueTupleFormatter<,,>)),
        new KeyValuePair<Type, Type>(typeof(Tuple<,,,>), typeof(TupleFormatter<,,,>)),
        new KeyValuePair<Type, Type>(typeof(ValueTuple<,,,>), typeof(ValueTupleFormatter<,,,>)),
        new KeyValuePair<Type, Type>(typeof(Tuple<,,,,>), typeof(TupleFormatter<,,,,>)),
        new KeyValuePair<Type, Type>(typeof(ValueTuple<,,,,>), typeof(ValueTupleFormatter<,,,,>)),
        new KeyValuePair<Type, Type>(typeof(Tuple<,,,,,>), typeof(TupleFormatter<,,,,,>)),
        new KeyValuePair<Type, Type>(typeof(ValueTuple<,,,,,>), typeof(ValueTupleFormatter<,,,,,>)),
        new KeyValuePair<Type, Type>(typeof(Tuple<,,,,,,>), typeof(TupleFormatter<,,,,,,>)),
        new KeyValuePair<Type, Type>(typeof(ValueTuple<,,,,,,>), typeof(ValueTupleFormatter<,,,,,,>)),
        new KeyValuePair<Type, Type>(typeof(Tuple<,,,,,,,>), typeof(TupleFormatter<,,,,,,,>)),
        new KeyValuePair<Type, Type>(typeof(ValueTuple<,,,,,,,>), typeof(ValueTupleFormatter<,,,,,,,>)),
    ]);
}

public sealed class TupleFormatter<T1> : ArchiveFormatter<Tuple<T1?>>
{
    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in Tuple<T1?>? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(1);
        writer.Write(value.Item1);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref Tuple<T1?>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 1)
            ArchiveSerializationException.ThrowInvalidPropertyCount(1, count);

        value = new Tuple<T1?>(reader.Read<T1?>());
    }
}

public sealed class TupleFormatter<T1, T2> : ArchiveFormatter<Tuple<T1?, T2?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in Tuple<T1?, T2?>? value
    )
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(2);
        writer.Write(value.Item1);
        writer.Write(value.Item2);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref Tuple<T1?, T2?>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 2)
            ArchiveSerializationException.ThrowInvalidPropertyCount(2, count);

        value = new Tuple<T1?, T2?>(reader.Read<T1?>(), reader.Read<T2?>());
    }
}

public sealed class TupleFormatter<T1, T2, T3> : ArchiveFormatter<Tuple<T1?, T2?, T3?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in Tuple<T1?, T2?, T3?>? value
    )
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(3);
        writer.Write(value.Item1);
        writer.Write(value.Item2);
        writer.Write(value.Item3);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref Tuple<T1?, T2?, T3?>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 3)
            ArchiveSerializationException.ThrowInvalidPropertyCount(3, count);

        value = new Tuple<T1?, T2?, T3?>(reader.Read<T1?>(), reader.Read<T2?>(), reader.Read<T3?>());
    }
}

public sealed class TupleFormatter<T1, T2, T3, T4> : ArchiveFormatter<Tuple<T1?, T2?, T3?, T4?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in Tuple<T1?, T2?, T3?, T4?>? value
    )
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(4);
        writer.Write(value.Item1);
        writer.Write(value.Item2);
        writer.Write(value.Item3);
        writer.Write(value.Item4);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref Tuple<T1?, T2?, T3?, T4?>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 4)
            ArchiveSerializationException.ThrowInvalidPropertyCount(4, count);

        value = new Tuple<T1?, T2?, T3?, T4?>(
            reader.Read<T1?>(),
            reader.Read<T2?>(),
            reader.Read<T3?>(),
            reader.Read<T4?>()
        );
    }
}

public sealed class TupleFormatter<T1, T2, T3, T4, T5> : ArchiveFormatter<Tuple<T1?, T2?, T3?, T4?, T5?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in Tuple<T1?, T2?, T3?, T4?, T5?>? value
    )
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(5);
        writer.Write(value.Item1);
        writer.Write(value.Item2);
        writer.Write(value.Item3);
        writer.Write(value.Item4);
        writer.Write(value.Item5);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref Tuple<T1?, T2?, T3?, T4?, T5?>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 5)
            ArchiveSerializationException.ThrowInvalidPropertyCount(5, count);

        value = new Tuple<T1?, T2?, T3?, T4?, T5?>(
            reader.Read<T1?>(),
            reader.Read<T2?>(),
            reader.Read<T3?>(),
            reader.Read<T4?>(),
            reader.Read<T5?>()
        );
    }
}

public sealed class TupleFormatter<T1, T2, T3, T4, T5, T6> : ArchiveFormatter<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in Tuple<T1?, T2?, T3?, T4?, T5?, T6?>? value
    )
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(6);
        writer.Write(value.Item1);
        writer.Write(value.Item2);
        writer.Write(value.Item3);
        writer.Write(value.Item4);
        writer.Write(value.Item5);
        writer.Write(value.Item6);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref Tuple<T1?, T2?, T3?, T4?, T5?, T6?>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 6)
            ArchiveSerializationException.ThrowInvalidPropertyCount(6, count);

        value = new Tuple<T1?, T2?, T3?, T4?, T5?, T6?>(
            reader.Read<T1?>(),
            reader.Read<T2?>(),
            reader.Read<T3?>(),
            reader.Read<T4?>(),
            reader.Read<T5?>(),
            reader.Read<T6?>()
        );
    }
}

public sealed class TupleFormatter<T1, T2, T3, T4, T5, T6, T7>
    : ArchiveFormatter<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>? value
    )
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(7);
        writer.Write(value.Item1);
        writer.Write(value.Item2);
        writer.Write(value.Item3);
        writer.Write(value.Item4);
        writer.Write(value.Item5);
        writer.Write(value.Item6);
        writer.Write(value.Item7);
    }

    public override void Deserialize(
        ref ArchiveReader reader,
        scoped ref Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>? value
    )
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 7)
            ArchiveSerializationException.ThrowInvalidPropertyCount(7, count);

        value = new Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>(
            reader.Read<T1?>(),
            reader.Read<T2?>(),
            reader.Read<T3?>(),
            reader.Read<T4?>(),
            reader.Read<T5?>(),
            reader.Read<T6?>(),
            reader.Read<T7?>()
        );
    }
}

public sealed class TupleFormatter<T1, T2, T3, T4, T5, T6, T7, TRest>
    : ArchiveFormatter<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>>
    where TRest : notnull
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>? value
    )
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(8);
        writer.Write(value.Item1);
        writer.Write(value.Item2);
        writer.Write(value.Item3);
        writer.Write(value.Item4);
        writer.Write(value.Item5);
        writer.Write(value.Item6);
        writer.Write(value.Item7);
        writer.Write(value.Rest);
    }

    public override void Deserialize(
        ref ArchiveReader reader,
        scoped ref Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>? value
    )
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 8)
            ArchiveSerializationException.ThrowInvalidPropertyCount(8, count);

        value = new Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>(
            reader.Read<T1?>(),
            reader.Read<T2?>(),
            reader.Read<T3?>(),
            reader.Read<T4?>(),
            reader.Read<T5?>(),
            reader.Read<T6?>(),
            reader.Read<T7?>(),
            reader.Read<TRest>()!
        );
    }
}

public sealed class ValueTupleFormatter<T1> : ArchiveFormatter<ValueTuple<T1?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ValueTuple<T1?> value
    )
    {
        writer.Write(value.Item1);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref ValueTuple<T1?> value)
    {
        reader.Read(out T1? item1);

        value = new ValueTuple<T1?>(item1);
    }
}

public sealed class ValueTupleFormatter<T1, T2> : ArchiveFormatter<ValueTuple<T1?, T2?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ValueTuple<T1?, T2?> value
    )
    {
        writer.Write(value.Item1, value.Item2);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref ValueTuple<T1?, T2?> value)
    {
        reader.Read(out T1? item1, out T2? item2);

        value = new ValueTuple<T1?, T2?>(item1, item2);
    }
}

public sealed class ValueTupleFormatter<T1, T2, T3> : ArchiveFormatter<ValueTuple<T1?, T2?, T3?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ValueTuple<T1?, T2?, T3?> value
    )
    {
        writer.Write(value.Item1, value.Item2, value.Item3);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref ValueTuple<T1?, T2?, T3?> value)
    {
        reader.Read(out T1? item1, out T2? item2, out T3? item3);

        value = new ValueTuple<T1?, T2?, T3?>(item1, item2, item3);
    }
}

public sealed class ValueTupleFormatter<T1, T2, T3, T4> : ArchiveFormatter<ValueTuple<T1?, T2?, T3?, T4?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ValueTuple<T1?, T2?, T3?, T4?> value
    )
    {
        writer.Write(value.Item1, value.Item2, value.Item3, value.Item4);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref ValueTuple<T1?, T2?, T3?, T4?> value)
    {
        reader.Read(out T1? item1, out T2? item2, out T3? item3, out T4? item4);

        value = new ValueTuple<T1?, T2?, T3?, T4?>(item1, item2, item3, item4);
    }
}

public sealed class ValueTupleFormatter<T1, T2, T3, T4, T5> : ArchiveFormatter<ValueTuple<T1?, T2?, T3?, T4?, T5?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ValueTuple<T1?, T2?, T3?, T4?, T5?> value
    )
    {
        writer.Write(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref ValueTuple<T1?, T2?, T3?, T4?, T5?> value)
    {
        reader.Read(out T1? item1, out T2? item2, out T3? item3, out T4? item4, out T5? item5);

        value = new ValueTuple<T1?, T2?, T3?, T4?, T5?>(item1, item2, item3, item4, item5);
    }
}

public sealed class ValueTupleFormatter<T1, T2, T3, T4, T5, T6>
    : ArchiveFormatter<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?> value
    )
    {
        writer.Write(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6);
    }

    public override void Deserialize(
        ref ArchiveReader reader,
        scoped ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?> value
    )
    {
        reader.Read(out T1? item1, out T2? item2, out T3? item3, out T4? item4, out T5? item5, out T6? item6);

        value = new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>(item1, item2, item3, item4, item5, item6);
    }
}

public sealed class ValueTupleFormatter<T1, T2, T3, T4, T5, T6, T7>
    : ArchiveFormatter<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?> value
    )
    {
        writer.Write(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7);
    }

    public override void Deserialize(
        ref ArchiveReader reader,
        scoped ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?> value
    )
    {
        reader.Read(
            out T1? item1,
            out T2? item2,
            out T3? item3,
            out T4? item4,
            out T5? item5,
            out T6? item6,
            out T7? item7
        );

        value = new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>(item1, item2, item3, item4, item5, item6, item7);
    }
}

public sealed class ValueTupleFormatter<T1, T2, T3, T4, T5, T6, T7, TRest>
    : ArchiveFormatter<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>>
    where TRest : struct
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest> value
    )
    {
        writer.Write(
            value.Item1,
            value.Item2,
            value.Item3,
            value.Item4,
            value.Item5,
            value.Item6,
            value.Item7,
            value.Rest
        );
    }

    public override void Deserialize(
        ref ArchiveReader reader,
        scoped ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest> value
    )
    {
        reader.Read(
            out T1? item1,
            out T2? item2,
            out T3? item3,
            out T4? item4,
            out T5? item5,
            out T6? item6,
            out T7? item7,
            out TRest item8
        );

        value = new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>(
            item1,
            item2,
            item3,
            item4,
            item5,
            item6,
            item7,
            item8!
        );
    }
}
