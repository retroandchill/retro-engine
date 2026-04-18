using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MagicArchive.Utilities;

namespace MagicArchive;

public ref partial struct ArchiveWriter<TBufferWriter>
    where TBufferWriter : IBufferWriter<byte>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittable<T1>(in T1 value1)
        where T1 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size = Unsafe.SizeOf<T1>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Advance(size);
        }
        else if (BlittableMarshalling.IsSimpleBlittable<T1>())
        {
            var size = Unsafe.SizeOf<T1>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Advance(size);
        }
        else
        {
            GetFormatter<T1>().Serialize(ref this, in value1);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittableWithObjectHeader<T1>(byte propertyCount, in T1 value1)
        where T1 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size = Unsafe.SizeOf<T1>() + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Advance(size);
        }
        else if (BlittableMarshalling.IsSimpleBlittable<T1>())
        {
            var size = Unsafe.SizeOf<T1>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T1>().Serialize(ref this, in value1);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittable<T1, T2>(in T1 value1, in T2 value2)
        where T1 : unmanaged
        where T2 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Advance(size);
        }
        else if (BlittableMarshalling.IsSimpleBlittable<T1>() && BlittableMarshalling.IsSimpleBlittable<T2>())
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T2>().Serialize(ref this, in value2);
            GetFormatter<T2>().Serialize(ref this, in value2);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittableWithObjectHeader<T1, T2>(byte propertyCount, in T1 value1, in T2 value2)
        where T1 : unmanaged
        where T2 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Advance(size);
        }
        else if (BlittableMarshalling.IsSimpleBlittable<T1>() && BlittableMarshalling.IsSimpleBlittable<T2>())
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T2>().Serialize(ref this, in value2);
            GetFormatter<T2>().Serialize(ref this, in value2);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittable<T1, T2, T3>(in T1 value1, in T2 value2, in T3 value3)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
        )
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T3>().Serialize(ref this, in value3);
            GetFormatter<T3>().Serialize(ref this, in value3);
            GetFormatter<T3>().Serialize(ref this, in value3);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittableWithObjectHeader<T1, T2, T3>(byte propertyCount, in T1 value1, in T2 value2, in T3 value3)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                value3!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
        )
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T3>().Serialize(ref this, in value3);
            GetFormatter<T3>().Serialize(ref this, in value3);
            GetFormatter<T3>().Serialize(ref this, in value3);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittable<T1, T2, T3, T4>(in T1 value1, in T2 value2, in T3 value3, in T4 value4)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                value4!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
        )
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T4>().Serialize(ref this, in value4);
            GetFormatter<T4>().Serialize(ref this, in value4);
            GetFormatter<T4>().Serialize(ref this, in value4);
            GetFormatter<T4>().Serialize(ref this, in value4);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittableWithObjectHeader<T1, T2, T3, T4>(
        byte propertyCount,
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4
    )
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                value3!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                value4!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
        )
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T4>().Serialize(ref this, in value4);
            GetFormatter<T4>().Serialize(ref this, in value4);
            GetFormatter<T4>().Serialize(ref this, in value4);
            GetFormatter<T4>().Serialize(ref this, in value4);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittable<T1, T2, T3, T4, T5>(in T1 value1, in T2 value2, in T3 value3, in T4 value4, in T5 value5)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                value5!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T5>().Serialize(ref this, in value5);
            GetFormatter<T5>().Serialize(ref this, in value5);
            GetFormatter<T5>().Serialize(ref this, in value5);
            GetFormatter<T5>().Serialize(ref this, in value5);
            GetFormatter<T5>().Serialize(ref this, in value5);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittableWithObjectHeader<T1, T2, T3, T4, T5>(
        byte propertyCount,
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5
    )
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                value3!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                value5!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T5>().Serialize(ref this, in value5);
            GetFormatter<T5>().Serialize(ref this, in value5);
            GetFormatter<T5>().Serialize(ref this, in value5);
            GetFormatter<T5>().Serialize(ref this, in value5);
            GetFormatter<T5>().Serialize(ref this, in value5);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittable<T1, T2, T3, T4, T5, T6>(
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6
    )
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                value6!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T6>().Serialize(ref this, in value6);
            GetFormatter<T6>().Serialize(ref this, in value6);
            GetFormatter<T6>().Serialize(ref this, in value6);
            GetFormatter<T6>().Serialize(ref this, in value6);
            GetFormatter<T6>().Serialize(ref this, in value6);
            GetFormatter<T6>().Serialize(ref this, in value6);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittableWithObjectHeader<T1, T2, T3, T4, T5, T6>(
        byte propertyCount,
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6
    )
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                value3!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                value6!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T6>().Serialize(ref this, in value6);
            GetFormatter<T6>().Serialize(ref this, in value6);
            GetFormatter<T6>().Serialize(ref this, in value6);
            GetFormatter<T6>().Serialize(ref this, in value6);
            GetFormatter<T6>().Serialize(ref this, in value6);
            GetFormatter<T6>().Serialize(ref this, in value6);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittable<T1, T2, T3, T4, T5, T6, T7>(
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7
    )
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                value7!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T7>().Serialize(ref this, in value7);
            GetFormatter<T7>().Serialize(ref this, in value7);
            GetFormatter<T7>().Serialize(ref this, in value7);
            GetFormatter<T7>().Serialize(ref this, in value7);
            GetFormatter<T7>().Serialize(ref this, in value7);
            GetFormatter<T7>().Serialize(ref this, in value7);
            GetFormatter<T7>().Serialize(ref this, in value7);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittableWithObjectHeader<T1, T2, T3, T4, T5, T6, T7>(
        byte propertyCount,
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7
    )
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                value3!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                value7!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T7>().Serialize(ref this, in value7);
            GetFormatter<T7>().Serialize(ref this, in value7);
            GetFormatter<T7>().Serialize(ref this, in value7);
            GetFormatter<T7>().Serialize(ref this, in value7);
            GetFormatter<T7>().Serialize(ref this, in value7);
            GetFormatter<T7>().Serialize(ref this, in value7);
            GetFormatter<T7>().Serialize(ref this, in value7);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8>(
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8
    )
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                value8!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittableWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8>(
        byte propertyCount,
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8
    )
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                value3!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                value8!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9
    )
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                value9!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittableWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        byte propertyCount,
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9
    )
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                value3!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                value9!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9,
        in T10 value10
    )
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
        where T10 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                value9!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                ),
                value10!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
            && BlittableMarshalling.IsSimpleBlittable<T10>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                ),
                BlittableMarshalling.ReverseEndianness(value10)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittableWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        byte propertyCount,
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9,
        in T10 value10
    )
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
        where T10 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                value3!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                value9!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + sizeof(byte)
                ),
                value10!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
            && BlittableMarshalling.IsSimpleBlittable<T10>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value10)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9,
        in T10 value10,
        in T11 value11
    )
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
        where T10 : unmanaged
        where T11 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                value9!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                ),
                value10!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                ),
                value11!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
            && BlittableMarshalling.IsSimpleBlittable<T10>()
            && BlittableMarshalling.IsSimpleBlittable<T11>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                ),
                BlittableMarshalling.ReverseEndianness(value10)!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                ),
                BlittableMarshalling.ReverseEndianness(value11)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittableWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        byte propertyCount,
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9,
        in T10 value10,
        in T11 value11
    )
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
        where T10 : unmanaged
        where T11 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                value3!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                value9!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + sizeof(byte)
                ),
                value10!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + sizeof(byte)
                ),
                value11!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
            && BlittableMarshalling.IsSimpleBlittable<T10>()
            && BlittableMarshalling.IsSimpleBlittable<T11>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value10)!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value11)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9,
        in T10 value10,
        in T11 value11,
        in T12 value12
    )
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
        where T10 : unmanaged
        where T11 : unmanaged
        where T12 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                value9!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                ),
                value10!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                ),
                value11!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                ),
                value12!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
            && BlittableMarshalling.IsSimpleBlittable<T10>()
            && BlittableMarshalling.IsSimpleBlittable<T11>()
            && BlittableMarshalling.IsSimpleBlittable<T12>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                ),
                BlittableMarshalling.ReverseEndianness(value10)!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                ),
                BlittableMarshalling.ReverseEndianness(value11)!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                ),
                BlittableMarshalling.ReverseEndianness(value12)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittableWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        byte propertyCount,
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9,
        in T10 value10,
        in T11 value11,
        in T12 value12
    )
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
        where T10 : unmanaged
        where T11 : unmanaged
        where T12 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>()
                + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                value3!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                value9!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + sizeof(byte)
                ),
                value10!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + sizeof(byte)
                ),
                value11!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + sizeof(byte)
                ),
                value12!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
            && BlittableMarshalling.IsSimpleBlittable<T10>()
            && BlittableMarshalling.IsSimpleBlittable<T11>()
            && BlittableMarshalling.IsSimpleBlittable<T12>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value10)!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value11)!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value12)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9,
        in T10 value10,
        in T11 value11,
        in T12 value12,
        in T13 value13
    )
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
        where T10 : unmanaged
        where T11 : unmanaged
        where T12 : unmanaged
        where T13 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>()
                + Unsafe.SizeOf<T13>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                value9!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                ),
                value10!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                ),
                value11!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                ),
                value12!
            );
            Unsafe.WriteUnaligned<T13>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                ),
                value13!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
            && BlittableMarshalling.IsSimpleBlittable<T10>()
            && BlittableMarshalling.IsSimpleBlittable<T11>()
            && BlittableMarshalling.IsSimpleBlittable<T12>()
            && BlittableMarshalling.IsSimpleBlittable<T13>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>()
                + Unsafe.SizeOf<T13>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                ),
                BlittableMarshalling.ReverseEndianness(value10)!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                ),
                BlittableMarshalling.ReverseEndianness(value11)!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                ),
                BlittableMarshalling.ReverseEndianness(value12)!
            );
            Unsafe.WriteUnaligned<T13>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                ),
                BlittableMarshalling.ReverseEndianness(value13)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittableWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        byte propertyCount,
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9,
        in T10 value10,
        in T11 value11,
        in T12 value12,
        in T13 value13
    )
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
        where T10 : unmanaged
        where T11 : unmanaged
        where T12 : unmanaged
        where T13 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>()
                + Unsafe.SizeOf<T13>()
                + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                value3!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                value9!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + sizeof(byte)
                ),
                value10!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + sizeof(byte)
                ),
                value11!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + sizeof(byte)
                ),
                value12!
            );
            Unsafe.WriteUnaligned<T13>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + sizeof(byte)
                ),
                value13!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
            && BlittableMarshalling.IsSimpleBlittable<T10>()
            && BlittableMarshalling.IsSimpleBlittable<T11>()
            && BlittableMarshalling.IsSimpleBlittable<T12>()
            && BlittableMarshalling.IsSimpleBlittable<T13>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>()
                + Unsafe.SizeOf<T13>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value10)!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value11)!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value12)!
            );
            Unsafe.WriteUnaligned<T13>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value13)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9,
        in T10 value10,
        in T11 value11,
        in T12 value12,
        in T13 value13,
        in T14 value14
    )
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
        where T10 : unmanaged
        where T11 : unmanaged
        where T12 : unmanaged
        where T13 : unmanaged
        where T14 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>()
                + Unsafe.SizeOf<T13>()
                + Unsafe.SizeOf<T14>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                value9!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                ),
                value10!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                ),
                value11!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                ),
                value12!
            );
            Unsafe.WriteUnaligned<T13>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                ),
                value13!
            );
            Unsafe.WriteUnaligned<T14>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + Unsafe.SizeOf<T13>()
                ),
                value14!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
            && BlittableMarshalling.IsSimpleBlittable<T10>()
            && BlittableMarshalling.IsSimpleBlittable<T11>()
            && BlittableMarshalling.IsSimpleBlittable<T12>()
            && BlittableMarshalling.IsSimpleBlittable<T13>()
            && BlittableMarshalling.IsSimpleBlittable<T14>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>()
                + Unsafe.SizeOf<T13>()
                + Unsafe.SizeOf<T14>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                ),
                BlittableMarshalling.ReverseEndianness(value10)!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                ),
                BlittableMarshalling.ReverseEndianness(value11)!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                ),
                BlittableMarshalling.ReverseEndianness(value12)!
            );
            Unsafe.WriteUnaligned<T13>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                ),
                BlittableMarshalling.ReverseEndianness(value13)!
            );
            Unsafe.WriteUnaligned<T14>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + Unsafe.SizeOf<T13>()
                ),
                BlittableMarshalling.ReverseEndianness(value14)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittableWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        byte propertyCount,
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9,
        in T10 value10,
        in T11 value11,
        in T12 value12,
        in T13 value13,
        in T14 value14
    )
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
        where T10 : unmanaged
        where T11 : unmanaged
        where T12 : unmanaged
        where T13 : unmanaged
        where T14 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>()
                + Unsafe.SizeOf<T13>()
                + Unsafe.SizeOf<T14>()
                + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                value3!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                value9!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + sizeof(byte)
                ),
                value10!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + sizeof(byte)
                ),
                value11!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + sizeof(byte)
                ),
                value12!
            );
            Unsafe.WriteUnaligned<T13>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + sizeof(byte)
                ),
                value13!
            );
            Unsafe.WriteUnaligned<T14>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + Unsafe.SizeOf<T13>()
                        + sizeof(byte)
                ),
                value14!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
            && BlittableMarshalling.IsSimpleBlittable<T10>()
            && BlittableMarshalling.IsSimpleBlittable<T11>()
            && BlittableMarshalling.IsSimpleBlittable<T12>()
            && BlittableMarshalling.IsSimpleBlittable<T13>()
            && BlittableMarshalling.IsSimpleBlittable<T14>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>()
                + Unsafe.SizeOf<T13>()
                + Unsafe.SizeOf<T14>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value10)!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value11)!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value12)!
            );
            Unsafe.WriteUnaligned<T13>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value13)!
            );
            Unsafe.WriteUnaligned<T14>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + Unsafe.SizeOf<T13>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value14)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9,
        in T10 value10,
        in T11 value11,
        in T12 value12,
        in T13 value13,
        in T14 value14,
        in T15 value15
    )
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
        where T10 : unmanaged
        where T11 : unmanaged
        where T12 : unmanaged
        where T13 : unmanaged
        where T14 : unmanaged
        where T15 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>()
                + Unsafe.SizeOf<T13>()
                + Unsafe.SizeOf<T14>()
                + Unsafe.SizeOf<T15>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                value9!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                ),
                value10!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                ),
                value11!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                ),
                value12!
            );
            Unsafe.WriteUnaligned<T13>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                ),
                value13!
            );
            Unsafe.WriteUnaligned<T14>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + Unsafe.SizeOf<T13>()
                ),
                value14!
            );
            Unsafe.WriteUnaligned<T15>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + Unsafe.SizeOf<T13>()
                        + Unsafe.SizeOf<T14>()
                ),
                value15!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
            && BlittableMarshalling.IsSimpleBlittable<T10>()
            && BlittableMarshalling.IsSimpleBlittable<T11>()
            && BlittableMarshalling.IsSimpleBlittable<T12>()
            && BlittableMarshalling.IsSimpleBlittable<T13>()
            && BlittableMarshalling.IsSimpleBlittable<T14>()
            && BlittableMarshalling.IsSimpleBlittable<T15>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>()
                + Unsafe.SizeOf<T13>()
                + Unsafe.SizeOf<T14>()
                + Unsafe.SizeOf<T15>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                ),
                BlittableMarshalling.ReverseEndianness(value10)!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                ),
                BlittableMarshalling.ReverseEndianness(value11)!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                ),
                BlittableMarshalling.ReverseEndianness(value12)!
            );
            Unsafe.WriteUnaligned<T13>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                ),
                BlittableMarshalling.ReverseEndianness(value13)!
            );
            Unsafe.WriteUnaligned<T14>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + Unsafe.SizeOf<T13>()
                ),
                BlittableMarshalling.ReverseEndianness(value14)!
            );
            Unsafe.WriteUnaligned<T15>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + Unsafe.SizeOf<T13>()
                        + Unsafe.SizeOf<T14>()
                ),
                BlittableMarshalling.ReverseEndianness(value15)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteBlittableWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
        byte propertyCount,
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9,
        in T10 value10,
        in T11 value11,
        in T12 value12,
        in T13 value13,
        in T14 value14,
        in T15 value15
    )
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
        where T5 : unmanaged
        where T6 : unmanaged
        where T7 : unmanaged
        where T8 : unmanaged
        where T9 : unmanaged
        where T10 : unmanaged
        where T11 : unmanaged
        where T12 : unmanaged
        where T13 : unmanaged
        where T14 : unmanaged
        where T15 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>()
                + Unsafe.SizeOf<T13>()
                + Unsafe.SizeOf<T14>()
                + Unsafe.SizeOf<T15>()
                + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                value3!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                value9!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + sizeof(byte)
                ),
                value10!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + sizeof(byte)
                ),
                value11!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + sizeof(byte)
                ),
                value12!
            );
            Unsafe.WriteUnaligned<T13>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + sizeof(byte)
                ),
                value13!
            );
            Unsafe.WriteUnaligned<T14>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + Unsafe.SizeOf<T13>()
                        + sizeof(byte)
                ),
                value14!
            );
            Unsafe.WriteUnaligned<T15>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + Unsafe.SizeOf<T13>()
                        + Unsafe.SizeOf<T14>()
                        + sizeof(byte)
                ),
                value15!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
            && BlittableMarshalling.IsSimpleBlittable<T10>()
            && BlittableMarshalling.IsSimpleBlittable<T11>()
            && BlittableMarshalling.IsSimpleBlittable<T12>()
            && BlittableMarshalling.IsSimpleBlittable<T13>()
            && BlittableMarshalling.IsSimpleBlittable<T14>()
            && BlittableMarshalling.IsSimpleBlittable<T15>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>()
                + Unsafe.SizeOf<T13>()
                + Unsafe.SizeOf<T14>()
                + Unsafe.SizeOf<T15>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value10)!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value11)!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value12)!
            );
            Unsafe.WriteUnaligned<T13>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value13)!
            );
            Unsafe.WriteUnaligned<T14>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + Unsafe.SizeOf<T13>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value14)!
            );
            Unsafe.WriteUnaligned<T15>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + Unsafe.SizeOf<T13>()
                        + Unsafe.SizeOf<T14>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value15)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittable<T1>(in T1 value1)
    {
        if (!IsByteSwapping)
        {
            var size = Unsafe.SizeOf<T1>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Advance(size);
        }
        else if (BlittableMarshalling.IsSimpleBlittable<T1>())
        {
            var size = Unsafe.SizeOf<T1>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Advance(size);
        }
        else
        {
            GetFormatter<T1>().Serialize(ref this, in value1);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittableWithObjectHeader<T1>(byte propertyCount, in T1 value1)
    {
        if (!IsByteSwapping)
        {
            var size = Unsafe.SizeOf<T1>() + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Advance(size);
        }
        else if (BlittableMarshalling.IsSimpleBlittable<T1>())
        {
            var size = Unsafe.SizeOf<T1>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T1>().Serialize(ref this, in value1);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittable<T1, T2>(in T1 value1, in T2 value2)
    {
        if (!IsByteSwapping)
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Advance(size);
        }
        else if (BlittableMarshalling.IsSimpleBlittable<T1>() && BlittableMarshalling.IsSimpleBlittable<T2>())
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T2>().Serialize(ref this, in value2);
            GetFormatter<T2>().Serialize(ref this, in value2);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittableWithObjectHeader<T1, T2>(byte propertyCount, in T1 value1, in T2 value2)
    {
        if (!IsByteSwapping)
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Advance(size);
        }
        else if (BlittableMarshalling.IsSimpleBlittable<T1>() && BlittableMarshalling.IsSimpleBlittable<T2>())
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T2>().Serialize(ref this, in value2);
            GetFormatter<T2>().Serialize(ref this, in value2);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittable<T1, T2, T3>(in T1 value1, in T2 value2, in T3 value3)
    {
        if (!IsByteSwapping)
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
        )
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T3>().Serialize(ref this, in value3);
            GetFormatter<T3>().Serialize(ref this, in value3);
            GetFormatter<T3>().Serialize(ref this, in value3);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittableWithObjectHeader<T1, T2, T3>(
        byte propertyCount,
        in T1 value1,
        in T2 value2,
        in T3 value3
    )
    {
        if (!IsByteSwapping)
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                value3!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
        )
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T3>().Serialize(ref this, in value3);
            GetFormatter<T3>().Serialize(ref this, in value3);
            GetFormatter<T3>().Serialize(ref this, in value3);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittable<T1, T2, T3, T4>(in T1 value1, in T2 value2, in T3 value3, in T4 value4)
    {
        if (!IsByteSwapping)
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                value4!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
        )
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T4>().Serialize(ref this, in value4);
            GetFormatter<T4>().Serialize(ref this, in value4);
            GetFormatter<T4>().Serialize(ref this, in value4);
            GetFormatter<T4>().Serialize(ref this, in value4);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittableWithObjectHeader<T1, T2, T3, T4>(
        byte propertyCount,
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4
    )
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                value3!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                value4!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
        )
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T4>().Serialize(ref this, in value4);
            GetFormatter<T4>().Serialize(ref this, in value4);
            GetFormatter<T4>().Serialize(ref this, in value4);
            GetFormatter<T4>().Serialize(ref this, in value4);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittable<T1, T2, T3, T4, T5>(
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5
    )
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                value5!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T5>().Serialize(ref this, in value5);
            GetFormatter<T5>().Serialize(ref this, in value5);
            GetFormatter<T5>().Serialize(ref this, in value5);
            GetFormatter<T5>().Serialize(ref this, in value5);
            GetFormatter<T5>().Serialize(ref this, in value5);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittableWithObjectHeader<T1, T2, T3, T4, T5>(
        byte propertyCount,
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5
    )
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                value3!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                value5!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T5>().Serialize(ref this, in value5);
            GetFormatter<T5>().Serialize(ref this, in value5);
            GetFormatter<T5>().Serialize(ref this, in value5);
            GetFormatter<T5>().Serialize(ref this, in value5);
            GetFormatter<T5>().Serialize(ref this, in value5);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittable<T1, T2, T3, T4, T5, T6>(
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6
    )
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                value6!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T6>().Serialize(ref this, in value6);
            GetFormatter<T6>().Serialize(ref this, in value6);
            GetFormatter<T6>().Serialize(ref this, in value6);
            GetFormatter<T6>().Serialize(ref this, in value6);
            GetFormatter<T6>().Serialize(ref this, in value6);
            GetFormatter<T6>().Serialize(ref this, in value6);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittableWithObjectHeader<T1, T2, T3, T4, T5, T6>(
        byte propertyCount,
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6
    )
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                value3!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                value6!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T6>().Serialize(ref this, in value6);
            GetFormatter<T6>().Serialize(ref this, in value6);
            GetFormatter<T6>().Serialize(ref this, in value6);
            GetFormatter<T6>().Serialize(ref this, in value6);
            GetFormatter<T6>().Serialize(ref this, in value6);
            GetFormatter<T6>().Serialize(ref this, in value6);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittable<T1, T2, T3, T4, T5, T6, T7>(
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7
    )
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                value7!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T7>().Serialize(ref this, in value7);
            GetFormatter<T7>().Serialize(ref this, in value7);
            GetFormatter<T7>().Serialize(ref this, in value7);
            GetFormatter<T7>().Serialize(ref this, in value7);
            GetFormatter<T7>().Serialize(ref this, in value7);
            GetFormatter<T7>().Serialize(ref this, in value7);
            GetFormatter<T7>().Serialize(ref this, in value7);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittableWithObjectHeader<T1, T2, T3, T4, T5, T6, T7>(
        byte propertyCount,
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7
    )
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                value3!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                value7!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T7>().Serialize(ref this, in value7);
            GetFormatter<T7>().Serialize(ref this, in value7);
            GetFormatter<T7>().Serialize(ref this, in value7);
            GetFormatter<T7>().Serialize(ref this, in value7);
            GetFormatter<T7>().Serialize(ref this, in value7);
            GetFormatter<T7>().Serialize(ref this, in value7);
            GetFormatter<T7>().Serialize(ref this, in value7);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8>(
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8
    )
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                value8!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittableWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8>(
        byte propertyCount,
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8
    )
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                value3!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                value8!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
            GetFormatter<T8>().Serialize(ref this, in value8);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9
    )
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                value9!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittableWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        byte propertyCount,
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9
    )
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                value3!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                value9!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
            GetFormatter<T9>().Serialize(ref this, in value9);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9,
        in T10 value10
    )
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                value9!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                ),
                value10!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
            && BlittableMarshalling.IsSimpleBlittable<T10>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                ),
                BlittableMarshalling.ReverseEndianness(value10)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittableWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        byte propertyCount,
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9,
        in T10 value10
    )
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                value3!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                value9!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + sizeof(byte)
                ),
                value10!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
            && BlittableMarshalling.IsSimpleBlittable<T10>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value10)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
            GetFormatter<T10>().Serialize(ref this, in value10);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9,
        in T10 value10,
        in T11 value11
    )
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                value9!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                ),
                value10!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                ),
                value11!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
            && BlittableMarshalling.IsSimpleBlittable<T10>()
            && BlittableMarshalling.IsSimpleBlittable<T11>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                ),
                BlittableMarshalling.ReverseEndianness(value10)!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                ),
                BlittableMarshalling.ReverseEndianness(value11)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittableWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        byte propertyCount,
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9,
        in T10 value10,
        in T11 value11
    )
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                value3!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                value9!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + sizeof(byte)
                ),
                value10!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + sizeof(byte)
                ),
                value11!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
            && BlittableMarshalling.IsSimpleBlittable<T10>()
            && BlittableMarshalling.IsSimpleBlittable<T11>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value10)!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value11)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
            GetFormatter<T11>().Serialize(ref this, in value11);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9,
        in T10 value10,
        in T11 value11,
        in T12 value12
    )
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                value9!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                ),
                value10!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                ),
                value11!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                ),
                value12!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
            && BlittableMarshalling.IsSimpleBlittable<T10>()
            && BlittableMarshalling.IsSimpleBlittable<T11>()
            && BlittableMarshalling.IsSimpleBlittable<T12>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                ),
                BlittableMarshalling.ReverseEndianness(value10)!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                ),
                BlittableMarshalling.ReverseEndianness(value11)!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                ),
                BlittableMarshalling.ReverseEndianness(value12)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittableWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        byte propertyCount,
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9,
        in T10 value10,
        in T11 value11,
        in T12 value12
    )
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>()
                + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                value3!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                value9!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + sizeof(byte)
                ),
                value10!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + sizeof(byte)
                ),
                value11!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + sizeof(byte)
                ),
                value12!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
            && BlittableMarshalling.IsSimpleBlittable<T10>()
            && BlittableMarshalling.IsSimpleBlittable<T11>()
            && BlittableMarshalling.IsSimpleBlittable<T12>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value10)!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value11)!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value12)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
            GetFormatter<T12>().Serialize(ref this, in value12);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9,
        in T10 value10,
        in T11 value11,
        in T12 value12,
        in T13 value13
    )
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>()
                + Unsafe.SizeOf<T13>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                value9!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                ),
                value10!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                ),
                value11!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                ),
                value12!
            );
            Unsafe.WriteUnaligned<T13>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                ),
                value13!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
            && BlittableMarshalling.IsSimpleBlittable<T10>()
            && BlittableMarshalling.IsSimpleBlittable<T11>()
            && BlittableMarshalling.IsSimpleBlittable<T12>()
            && BlittableMarshalling.IsSimpleBlittable<T13>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>()
                + Unsafe.SizeOf<T13>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                ),
                BlittableMarshalling.ReverseEndianness(value10)!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                ),
                BlittableMarshalling.ReverseEndianness(value11)!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                ),
                BlittableMarshalling.ReverseEndianness(value12)!
            );
            Unsafe.WriteUnaligned<T13>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                ),
                BlittableMarshalling.ReverseEndianness(value13)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittableWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        byte propertyCount,
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9,
        in T10 value10,
        in T11 value11,
        in T12 value12,
        in T13 value13
    )
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>()
                + Unsafe.SizeOf<T13>()
                + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                value3!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                value9!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + sizeof(byte)
                ),
                value10!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + sizeof(byte)
                ),
                value11!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + sizeof(byte)
                ),
                value12!
            );
            Unsafe.WriteUnaligned<T13>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + sizeof(byte)
                ),
                value13!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
            && BlittableMarshalling.IsSimpleBlittable<T10>()
            && BlittableMarshalling.IsSimpleBlittable<T11>()
            && BlittableMarshalling.IsSimpleBlittable<T12>()
            && BlittableMarshalling.IsSimpleBlittable<T13>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>()
                + Unsafe.SizeOf<T13>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value10)!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value11)!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value12)!
            );
            Unsafe.WriteUnaligned<T13>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value13)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
            GetFormatter<T13>().Serialize(ref this, in value13);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9,
        in T10 value10,
        in T11 value11,
        in T12 value12,
        in T13 value13,
        in T14 value14
    )
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>()
                + Unsafe.SizeOf<T13>()
                + Unsafe.SizeOf<T14>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                value9!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                ),
                value10!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                ),
                value11!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                ),
                value12!
            );
            Unsafe.WriteUnaligned<T13>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                ),
                value13!
            );
            Unsafe.WriteUnaligned<T14>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + Unsafe.SizeOf<T13>()
                ),
                value14!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
            && BlittableMarshalling.IsSimpleBlittable<T10>()
            && BlittableMarshalling.IsSimpleBlittable<T11>()
            && BlittableMarshalling.IsSimpleBlittable<T12>()
            && BlittableMarshalling.IsSimpleBlittable<T13>()
            && BlittableMarshalling.IsSimpleBlittable<T14>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>()
                + Unsafe.SizeOf<T13>()
                + Unsafe.SizeOf<T14>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                ),
                BlittableMarshalling.ReverseEndianness(value10)!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                ),
                BlittableMarshalling.ReverseEndianness(value11)!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                ),
                BlittableMarshalling.ReverseEndianness(value12)!
            );
            Unsafe.WriteUnaligned<T13>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                ),
                BlittableMarshalling.ReverseEndianness(value13)!
            );
            Unsafe.WriteUnaligned<T14>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + Unsafe.SizeOf<T13>()
                ),
                BlittableMarshalling.ReverseEndianness(value14)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittableWithObjectHeader<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        byte propertyCount,
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9,
        in T10 value10,
        in T11 value11,
        in T12 value12,
        in T13 value13,
        in T14 value14
    )
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>()
                + Unsafe.SizeOf<T13>()
                + Unsafe.SizeOf<T14>()
                + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                value3!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                value9!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + sizeof(byte)
                ),
                value10!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + sizeof(byte)
                ),
                value11!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + sizeof(byte)
                ),
                value12!
            );
            Unsafe.WriteUnaligned<T13>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + sizeof(byte)
                ),
                value13!
            );
            Unsafe.WriteUnaligned<T14>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + Unsafe.SizeOf<T13>()
                        + sizeof(byte)
                ),
                value14!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
            && BlittableMarshalling.IsSimpleBlittable<T10>()
            && BlittableMarshalling.IsSimpleBlittable<T11>()
            && BlittableMarshalling.IsSimpleBlittable<T12>()
            && BlittableMarshalling.IsSimpleBlittable<T13>()
            && BlittableMarshalling.IsSimpleBlittable<T14>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>()
                + Unsafe.SizeOf<T13>()
                + Unsafe.SizeOf<T14>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value10)!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value11)!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value12)!
            );
            Unsafe.WriteUnaligned<T13>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value13)!
            );
            Unsafe.WriteUnaligned<T14>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + Unsafe.SizeOf<T13>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value14)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
            GetFormatter<T14>().Serialize(ref this, in value14);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9,
        in T10 value10,
        in T11 value11,
        in T12 value12,
        in T13 value13,
        in T14 value14,
        in T15 value15
    )
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>()
                + Unsafe.SizeOf<T13>()
                + Unsafe.SizeOf<T14>()
                + Unsafe.SizeOf<T15>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                value9!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                ),
                value10!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                ),
                value11!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                ),
                value12!
            );
            Unsafe.WriteUnaligned<T13>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                ),
                value13!
            );
            Unsafe.WriteUnaligned<T14>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + Unsafe.SizeOf<T13>()
                ),
                value14!
            );
            Unsafe.WriteUnaligned<T15>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + Unsafe.SizeOf<T13>()
                        + Unsafe.SizeOf<T14>()
                ),
                value15!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
            && BlittableMarshalling.IsSimpleBlittable<T10>()
            && BlittableMarshalling.IsSimpleBlittable<T11>()
            && BlittableMarshalling.IsSimpleBlittable<T12>()
            && BlittableMarshalling.IsSimpleBlittable<T13>()
            && BlittableMarshalling.IsSimpleBlittable<T14>()
            && BlittableMarshalling.IsSimpleBlittable<T15>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>()
                + Unsafe.SizeOf<T13>()
                + Unsafe.SizeOf<T14>()
                + Unsafe.SizeOf<T15>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, BlittableMarshalling.ReverseEndianness(value1)!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                ),
                BlittableMarshalling.ReverseEndianness(value10)!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                ),
                BlittableMarshalling.ReverseEndianness(value11)!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                ),
                BlittableMarshalling.ReverseEndianness(value12)!
            );
            Unsafe.WriteUnaligned<T13>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                ),
                BlittableMarshalling.ReverseEndianness(value13)!
            );
            Unsafe.WriteUnaligned<T14>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + Unsafe.SizeOf<T13>()
                ),
                BlittableMarshalling.ReverseEndianness(value14)!
            );
            Unsafe.WriteUnaligned<T15>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + Unsafe.SizeOf<T13>()
                        + Unsafe.SizeOf<T14>()
                ),
                BlittableMarshalling.ReverseEndianness(value15)!
            );
            Advance(size);
        }
        else
        {
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void UnsafeWriteBlittableWithObjectHeader<
        T1,
        T2,
        T3,
        T4,
        T5,
        T6,
        T7,
        T8,
        T9,
        T10,
        T11,
        T12,
        T13,
        T14,
        T15
    >(
        byte propertyCount,
        in T1 value1,
        in T2 value2,
        in T3 value3,
        in T4 value4,
        in T5 value5,
        in T6 value6,
        in T7 value7,
        in T8 value8,
        in T9 value9,
        in T10 value10,
        in T11 value11,
        in T12 value12,
        in T13 value13,
        in T14 value14,
        in T15 value15
    )
    {
        if (!IsByteSwapping)
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>()
                + Unsafe.SizeOf<T13>()
                + Unsafe.SizeOf<T14>()
                + Unsafe.SizeOf<T15>()
                + sizeof(byte);
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)), value2!);
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                value3!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                value4!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                value5!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                value6!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                value7!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                value8!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                value9!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + sizeof(byte)
                ),
                value10!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + sizeof(byte)
                ),
                value11!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + sizeof(byte)
                ),
                value12!
            );
            Unsafe.WriteUnaligned<T13>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + sizeof(byte)
                ),
                value13!
            );
            Unsafe.WriteUnaligned<T14>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + Unsafe.SizeOf<T13>()
                        + sizeof(byte)
                ),
                value14!
            );
            Unsafe.WriteUnaligned<T15>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + Unsafe.SizeOf<T13>()
                        + Unsafe.SizeOf<T14>()
                        + sizeof(byte)
                ),
                value15!
            );
            Advance(size);
        }
        else if (
            BlittableMarshalling.IsSimpleBlittable<T1>()
            && BlittableMarshalling.IsSimpleBlittable<T2>()
            && BlittableMarshalling.IsSimpleBlittable<T3>()
            && BlittableMarshalling.IsSimpleBlittable<T4>()
            && BlittableMarshalling.IsSimpleBlittable<T5>()
            && BlittableMarshalling.IsSimpleBlittable<T6>()
            && BlittableMarshalling.IsSimpleBlittable<T7>()
            && BlittableMarshalling.IsSimpleBlittable<T8>()
            && BlittableMarshalling.IsSimpleBlittable<T9>()
            && BlittableMarshalling.IsSimpleBlittable<T10>()
            && BlittableMarshalling.IsSimpleBlittable<T11>()
            && BlittableMarshalling.IsSimpleBlittable<T12>()
            && BlittableMarshalling.IsSimpleBlittable<T13>()
            && BlittableMarshalling.IsSimpleBlittable<T14>()
            && BlittableMarshalling.IsSimpleBlittable<T15>()
        )
        {
            var size =
                Unsafe.SizeOf<T1>()
                + Unsafe.SizeOf<T2>()
                + Unsafe.SizeOf<T3>()
                + Unsafe.SizeOf<T4>()
                + Unsafe.SizeOf<T5>()
                + Unsafe.SizeOf<T6>()
                + Unsafe.SizeOf<T7>()
                + Unsafe.SizeOf<T8>()
                + Unsafe.SizeOf<T9>()
                + Unsafe.SizeOf<T10>()
                + Unsafe.SizeOf<T11>()
                + Unsafe.SizeOf<T12>()
                + Unsafe.SizeOf<T13>()
                + Unsafe.SizeOf<T14>()
                + Unsafe.SizeOf<T15>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned(ref spanRef, propertyCount);
            Unsafe.WriteUnaligned<T1>(ref Unsafe.Add(ref spanRef, sizeof(byte)), value1!);
            Unsafe.WriteUnaligned<T2>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value2)!
            );
            Unsafe.WriteUnaligned<T3>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + sizeof(byte)),
                BlittableMarshalling.ReverseEndianness(value3)!
            );
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value4)!
            );
            Unsafe.WriteUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>() + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value5)!
            );
            Unsafe.WriteUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value6)!
            );
            Unsafe.WriteUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value7)!
            );
            Unsafe.WriteUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value8)!
            );
            Unsafe.WriteUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value9)!
            );
            Unsafe.WriteUnaligned<T10>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value10)!
            );
            Unsafe.WriteUnaligned<T11>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value11)!
            );
            Unsafe.WriteUnaligned<T12>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value12)!
            );
            Unsafe.WriteUnaligned<T13>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value13)!
            );
            Unsafe.WriteUnaligned<T14>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + Unsafe.SizeOf<T13>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value14)!
            );
            Unsafe.WriteUnaligned<T15>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                        + Unsafe.SizeOf<T9>()
                        + Unsafe.SizeOf<T10>()
                        + Unsafe.SizeOf<T11>()
                        + Unsafe.SizeOf<T12>()
                        + Unsafe.SizeOf<T13>()
                        + Unsafe.SizeOf<T14>()
                        + sizeof(byte)
                ),
                BlittableMarshalling.ReverseEndianness(value15)!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable(propertyCount);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
            GetFormatter<T15>().Serialize(ref this, in value15);
        }
    }
}
