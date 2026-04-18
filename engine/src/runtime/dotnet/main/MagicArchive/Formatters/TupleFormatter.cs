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

    public static readonly ImmutableArray<Type> ValueTupleTypes =
    [
        typeof(ValueTuple<>),
        typeof(ValueTuple<,>),
        typeof(ValueTuple<,,>),
        typeof(ValueTuple<,,,>),
        typeof(ValueTuple<,,,,>),
        typeof(ValueTuple<,,,,,>),
        typeof(ValueTuple<,,,,,,>),
        typeof(ValueTuple<,,,,,,,>),
    ];
}

public sealed class TupleFormatter<T1> : ArchiveFormatter<Tuple<T1?>>
{
    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in Tuple<T1?>? value)
    {
        if (value is null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(1);
        writer.WriteValue(value.Item1);
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

        value = new Tuple<T1?>(reader.ReadValue<T1?>());
    }
}

public sealed class TupleFormatter<T1, T2> : ArchiveFormatter<Tuple<T1?, T2?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in Tuple<T1?, T2?>? value
    )
    {
        if (value is null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(2);
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
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

        value = new Tuple<T1?, T2?>(reader.ReadValue<T1?>(), reader.ReadValue<T2?>());
    }
}

public sealed class TupleFormatter<T1, T2, T3> : ArchiveFormatter<Tuple<T1?, T2?, T3?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in Tuple<T1?, T2?, T3?>? value
    )
    {
        if (value is null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(3);
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
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

        value = new Tuple<T1?, T2?, T3?>(reader.ReadValue<T1?>(), reader.ReadValue<T2?>(), reader.ReadValue<T3?>());
    }
}

public sealed class TupleFormatter<T1, T2, T3, T4> : ArchiveFormatter<Tuple<T1?, T2?, T3?, T4?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in Tuple<T1?, T2?, T3?, T4?>? value
    )
    {
        if (value is null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(4);
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
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
            reader.ReadValue<T1?>(),
            reader.ReadValue<T2?>(),
            reader.ReadValue<T3?>(),
            reader.ReadValue<T4?>()
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
        if (value is null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(5);
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
        writer.WriteValue(value.Item5);
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
            reader.ReadValue<T1?>(),
            reader.ReadValue<T2?>(),
            reader.ReadValue<T3?>(),
            reader.ReadValue<T4?>(),
            reader.ReadValue<T5?>()
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
        if (value is null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(6);
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
        writer.WriteValue(value.Item5);
        writer.WriteValue(value.Item6);
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
            reader.ReadValue<T1?>(),
            reader.ReadValue<T2?>(),
            reader.ReadValue<T3?>(),
            reader.ReadValue<T4?>(),
            reader.ReadValue<T5?>(),
            reader.ReadValue<T6?>()
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
        if (value is null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(7);
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
        writer.WriteValue(value.Item5);
        writer.WriteValue(value.Item6);
        writer.WriteValue(value.Item7);
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
            reader.ReadValue<T1?>(),
            reader.ReadValue<T2?>(),
            reader.ReadValue<T3?>(),
            reader.ReadValue<T4?>(),
            reader.ReadValue<T5?>(),
            reader.ReadValue<T6?>(),
            reader.ReadValue<T7?>()
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
        if (value is null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(8);
        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
        writer.WriteValue(value.Item5);
        writer.WriteValue(value.Item6);
        writer.WriteValue(value.Item7);
        writer.WriteValue(value.Rest);
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
            reader.ReadValue<T1?>(),
            reader.ReadValue<T2?>(),
            reader.ReadValue<T3?>(),
            reader.ReadValue<T4?>(),
            reader.ReadValue<T5?>(),
            reader.ReadValue<T6?>(),
            reader.ReadValue<T7?>(),
            reader.ReadValue<TRest>()!
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
        if (BlittableMarshalling.IsBlittable<ValueTuple<T1?>>())
        {
            writer.UnsafeWriteBlittable(in value);
            return;
        }

        writer.WriteValue(value.Item1);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref ValueTuple<T1?> value)
    {
        if (BlittableMarshalling.IsBlittable<ValueTuple<T1?>>())
        {
            reader.UnsafeReadBlittable(out value);
            return;
        }

        value = new ValueTuple<T1?>(reader.ReadValue<T1?>());
    }
}

public sealed class ValueTupleFormatter<T1, T2> : ArchiveFormatter<ValueTuple<T1?, T2?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ValueTuple<T1?, T2?> value
    )
    {
        if (BlittableMarshalling.IsBlittable<ValueTuple<T1?, T2?>>())
        {
            writer.UnsafeWriteBlittable(in value);
            return;
        }

        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref ValueTuple<T1?, T2?> value)
    {
        if (BlittableMarshalling.IsBlittable<ValueTuple<T1?, T2?>>())
        {
            reader.UnsafeReadBlittable(out value);
            return;
        }

        value = new ValueTuple<T1?, T2?>(reader.ReadValue<T1?>(), reader.ReadValue<T2?>());
    }
}

public sealed class ValueTupleFormatter<T1, T2, T3> : ArchiveFormatter<ValueTuple<T1?, T2?, T3?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ValueTuple<T1?, T2?, T3?> value
    )
    {
        if (BlittableMarshalling.IsBlittable<ValueTuple<T1?, T2?, T3?>>())
        {
            writer.UnsafeWriteBlittable(in value);
            return;
        }

        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref ValueTuple<T1?, T2?, T3?> value)
    {
        if (BlittableMarshalling.IsBlittable<ValueTuple<T1?, T2?, T3?>>())
        {
            reader.UnsafeReadBlittable(out value);
            return;
        }

        value = new ValueTuple<T1?, T2?, T3?>(
            reader.ReadValue<T1?>(),
            reader.ReadValue<T2?>(),
            reader.ReadValue<T3?>()
        );
    }
}

public sealed class ValueTupleFormatter<T1, T2, T3, T4> : ArchiveFormatter<ValueTuple<T1?, T2?, T3?, T4?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ValueTuple<T1?, T2?, T3?, T4?> value
    )
    {
        if (BlittableMarshalling.IsBlittable<ValueTuple<T1?, T2?, T3?, T4?>>())
        {
            writer.UnsafeWriteBlittable(in value);
            return;
        }

        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref ValueTuple<T1?, T2?, T3?, T4?> value)
    {
        if (BlittableMarshalling.IsBlittable<ValueTuple<T1?, T2?, T3?, T4?>>())
        {
            reader.UnsafeReadBlittable(out value);
            return;
        }

        value = new ValueTuple<T1?, T2?, T3?, T4?>(
            reader.ReadValue<T1?>(),
            reader.ReadValue<T2?>(),
            reader.ReadValue<T3?>(),
            reader.ReadValue<T4?>()
        );
    }
}

public sealed class ValueTupleFormatter<T1, T2, T3, T4, T5> : ArchiveFormatter<ValueTuple<T1?, T2?, T3?, T4?, T5?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in ValueTuple<T1?, T2?, T3?, T4?, T5?> value
    )
    {
        if (BlittableMarshalling.IsBlittable<ValueTuple<T1?, T2?, T3?, T4?, T5?>>())
        {
            writer.UnsafeWriteBlittable(in value);
            return;
        }

        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
        writer.WriteValue(value.Item5);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref ValueTuple<T1?, T2?, T3?, T4?, T5?> value)
    {
        if (BlittableMarshalling.IsBlittable<ValueTuple<T1?, T2?, T3?, T4?, T5?>>())
        {
            reader.UnsafeReadBlittable(out value);
            return;
        }

        value = new ValueTuple<T1?, T2?, T3?, T4?, T5?>(
            reader.ReadValue<T1?>(),
            reader.ReadValue<T2?>(),
            reader.ReadValue<T3?>(),
            reader.ReadValue<T4?>(),
            reader.ReadValue<T5?>()
        );
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
        if (BlittableMarshalling.IsBlittable<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>>())
        {
            writer.UnsafeWriteBlittable(in value);
            return;
        }

        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
        writer.WriteValue(value.Item5);
        writer.WriteValue(value.Item6);
    }

    public override void Deserialize(
        ref ArchiveReader reader,
        scoped ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?> value
    )
    {
        if (BlittableMarshalling.IsBlittable<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>>())
        {
            reader.UnsafeReadBlittable(out value);
            return;
        }

        value = new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>(
            reader.ReadValue<T1?>(),
            reader.ReadValue<T2?>(),
            reader.ReadValue<T3?>(),
            reader.ReadValue<T4?>(),
            reader.ReadValue<T5?>(),
            reader.ReadValue<T6?>()
        );
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
        if (BlittableMarshalling.IsBlittable<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>())
        {
            writer.UnsafeWriteBlittable(in value);
            return;
        }

        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
        writer.WriteValue(value.Item5);
        writer.WriteValue(value.Item6);
        writer.WriteValue(value.Item7);
    }

    public override void Deserialize(
        ref ArchiveReader reader,
        scoped ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?> value
    )
    {
        if (BlittableMarshalling.IsBlittable<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>())
        {
            reader.UnsafeReadBlittable(out value);
            return;
        }

        value = new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>(
            reader.ReadValue<T1?>(),
            reader.ReadValue<T2?>(),
            reader.ReadValue<T3?>(),
            reader.ReadValue<T4?>(),
            reader.ReadValue<T5?>(),
            reader.ReadValue<T6?>(),
            reader.ReadValue<T7?>()
        );
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
        if (BlittableMarshalling.IsBlittable<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>>())
        {
            writer.UnsafeWriteBlittable(in value);
            return;
        }

        writer.WriteValue(value.Item1);
        writer.WriteValue(value.Item2);
        writer.WriteValue(value.Item3);
        writer.WriteValue(value.Item4);
        writer.WriteValue(value.Item5);
        writer.WriteValue(value.Item6);
        writer.WriteValue(value.Item7);
        writer.WriteValue(value.Rest);
    }

    public override void Deserialize(
        ref ArchiveReader reader,
        scoped ref ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest> value
    )
    {
        if (BlittableMarshalling.IsBlittable<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>>())
        {
            reader.UnsafeReadBlittable(out value);
            return;
        }

        value = new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>(
            reader.ReadValue<T1?>(),
            reader.ReadValue<T2?>(),
            reader.ReadValue<T3?>(),
            reader.ReadValue<T4?>(),
            reader.ReadValue<T5?>(),
            reader.ReadValue<T6?>(),
            reader.ReadValue<T7?>(),
            reader.ReadValue<TRest>()!
        );
    }
}
