using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MagicArchive.Utilities;

namespace MagicArchive;

public ref partial struct ArchiveWriter<TBufferWriter>
    where TBufferWriter : IBufferWriter<byte>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteBlittable<T1, T2>(in T1 value1, in T2 value2)
        where T1 : unmanaged
        where T2 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Advance(size);
        }
        else
        {
            WriteBlittable<T1>(in value1!);
            WriteBlittable<T2>(in value2!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteBlittable<T1, T2, T3>(in T1 value1, in T2 value2, in T3 value3)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Advance(size);
        }
        else
        {
            WriteBlittable<T1>(in value1!);
            WriteBlittable<T2>(in value2!);
            WriteBlittable<T3>(in value3!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteBlittable<T1, T2, T3, T4>(in T1 value1, in T2 value2, in T3 value3, in T4 value4)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                value4!
            );
            Advance(size);
        }
        else
        {
            WriteBlittable<T1>(in value1!);
            WriteBlittable<T2>(in value2!);
            WriteBlittable<T3>(in value3!);
            WriteBlittable<T4>(in value4!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteBlittable<T1, T2, T3, T4, T5>(
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
                + Unsafe.SizeOf<T5>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
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
        else
        {
            WriteBlittable<T1>(in value1!);
            WriteBlittable<T2>(in value2!);
            WriteBlittable<T3>(in value3!);
            WriteBlittable<T4>(in value4!);
            WriteBlittable<T5>(in value5!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteBlittable<T1, T2, T3, T4, T5, T6>(
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
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
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
        else
        {
            WriteBlittable<T1>(in value1!);
            WriteBlittable<T2>(in value2!);
            WriteBlittable<T3>(in value3!);
            WriteBlittable<T4>(in value4!);
            WriteBlittable<T5>(in value5!);
            WriteBlittable<T6>(in value6!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteBlittable<T1, T2, T3, T4, T5, T6, T7>(
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
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
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
        else
        {
            WriteBlittable<T1>(in value1!);
            WriteBlittable<T2>(in value2!);
            WriteBlittable<T3>(in value3!);
            WriteBlittable<T4>(in value4!);
            WriteBlittable<T5>(in value5!);
            WriteBlittable<T6>(in value6!);
            WriteBlittable<T7>(in value7!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8>(
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
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
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
        else
        {
            WriteBlittable<T1>(in value1!);
            WriteBlittable<T2>(in value2!);
            WriteBlittable<T3>(in value3!);
            WriteBlittable<T4>(in value4!);
            WriteBlittable<T5>(in value5!);
            WriteBlittable<T6>(in value6!);
            WriteBlittable<T7>(in value7!);
            WriteBlittable<T8>(in value8!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
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
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
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
        else
        {
            WriteBlittable<T1>(in value1!);
            WriteBlittable<T2>(in value2!);
            WriteBlittable<T3>(in value3!);
            WriteBlittable<T4>(in value4!);
            WriteBlittable<T5>(in value5!);
            WriteBlittable<T6>(in value6!);
            WriteBlittable<T7>(in value7!);
            WriteBlittable<T8>(in value8!);
            WriteBlittable<T9>(in value9!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
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
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
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
        else
        {
            WriteBlittable<T1>(in value1!);
            WriteBlittable<T2>(in value2!);
            WriteBlittable<T3>(in value3!);
            WriteBlittable<T4>(in value4!);
            WriteBlittable<T5>(in value5!);
            WriteBlittable<T6>(in value6!);
            WriteBlittable<T7>(in value7!);
            WriteBlittable<T8>(in value8!);
            WriteBlittable<T9>(in value9!);
            WriteBlittable<T10>(in value10!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
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
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
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
        else
        {
            WriteBlittable<T1>(in value1!);
            WriteBlittable<T2>(in value2!);
            WriteBlittable<T3>(in value3!);
            WriteBlittable<T4>(in value4!);
            WriteBlittable<T5>(in value5!);
            WriteBlittable<T6>(in value6!);
            WriteBlittable<T7>(in value7!);
            WriteBlittable<T8>(in value8!);
            WriteBlittable<T9>(in value9!);
            WriteBlittable<T10>(in value10!);
            WriteBlittable<T11>(in value11!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
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
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
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
        else
        {
            WriteBlittable<T1>(in value1!);
            WriteBlittable<T2>(in value2!);
            WriteBlittable<T3>(in value3!);
            WriteBlittable<T4>(in value4!);
            WriteBlittable<T5>(in value5!);
            WriteBlittable<T6>(in value6!);
            WriteBlittable<T7>(in value7!);
            WriteBlittable<T8>(in value8!);
            WriteBlittable<T9>(in value9!);
            WriteBlittable<T10>(in value10!);
            WriteBlittable<T11>(in value11!);
            WriteBlittable<T12>(in value12!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
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
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
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
        else
        {
            WriteBlittable<T1>(in value1!);
            WriteBlittable<T2>(in value2!);
            WriteBlittable<T3>(in value3!);
            WriteBlittable<T4>(in value4!);
            WriteBlittable<T5>(in value5!);
            WriteBlittable<T6>(in value6!);
            WriteBlittable<T7>(in value7!);
            WriteBlittable<T8>(in value8!);
            WriteBlittable<T9>(in value9!);
            WriteBlittable<T10>(in value10!);
            WriteBlittable<T11>(in value11!);
            WriteBlittable<T12>(in value12!);
            WriteBlittable<T13>(in value13!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
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
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
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
        else
        {
            WriteBlittable<T1>(in value1!);
            WriteBlittable<T2>(in value2!);
            WriteBlittable<T3>(in value3!);
            WriteBlittable<T4>(in value4!);
            WriteBlittable<T5>(in value5!);
            WriteBlittable<T6>(in value6!);
            WriteBlittable<T7>(in value7!);
            WriteBlittable<T8>(in value8!);
            WriteBlittable<T9>(in value9!);
            WriteBlittable<T10>(in value10!);
            WriteBlittable<T11>(in value11!);
            WriteBlittable<T12>(in value12!);
            WriteBlittable<T13>(in value13!);
            WriteBlittable<T14>(in value14!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
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
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
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
        else
        {
            WriteBlittable<T1>(in value1!);
            WriteBlittable<T2>(in value2!);
            WriteBlittable<T3>(in value3!);
            WriteBlittable<T4>(in value4!);
            WriteBlittable<T5>(in value5!);
            WriteBlittable<T6>(in value6!);
            WriteBlittable<T7>(in value7!);
            WriteBlittable<T8>(in value8!);
            WriteBlittable<T9>(in value9!);
            WriteBlittable<T10>(in value10!);
            WriteBlittable<T11>(in value11!);
            WriteBlittable<T12>(in value12!);
            WriteBlittable<T13>(in value13!);
            WriteBlittable<T14>(in value14!);
            WriteBlittable<T15>(in value15!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeWriteBlittable<T1, T2>(in T1 value1, in T2 value2)
    {
        if (!IsByteSwapping)
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Advance(size);
        }
        else
        {
            UnsafeWriteBlittable<T1>(in value1!);
            UnsafeWriteBlittable<T2>(in value2!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeWriteBlittable<T1, T2, T3>(in T1 value1, in T2 value2, in T3 value3)
    {
        if (!IsByteSwapping)
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Advance(size);
        }
        else
        {
            UnsafeWriteBlittable<T1>(in value1!);
            UnsafeWriteBlittable<T2>(in value2!);
            UnsafeWriteBlittable<T3>(in value3!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeWriteBlittable<T1, T2, T3, T4>(in T1 value1, in T2 value2, in T3 value3, in T4 value4)
    {
        if (!IsByteSwapping)
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>();
            ref var spanRef = ref GetSpanReference(size);
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
            Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
            Unsafe.WriteUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()), value3!);
            Unsafe.WriteUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                value4!
            );
            Advance(size);
        }
        else
        {
            UnsafeWriteBlittable<T1>(in value1!);
            UnsafeWriteBlittable<T2>(in value2!);
            UnsafeWriteBlittable<T3>(in value3!);
            UnsafeWriteBlittable<T4>(in value4!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeWriteBlittable<T1, T2, T3, T4, T5>(
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
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
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
        else
        {
            UnsafeWriteBlittable<T1>(in value1!);
            UnsafeWriteBlittable<T2>(in value2!);
            UnsafeWriteBlittable<T3>(in value3!);
            UnsafeWriteBlittable<T4>(in value4!);
            UnsafeWriteBlittable<T5>(in value5!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeWriteBlittable<T1, T2, T3, T4, T5, T6>(
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
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
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
        else
        {
            UnsafeWriteBlittable<T1>(in value1!);
            UnsafeWriteBlittable<T2>(in value2!);
            UnsafeWriteBlittable<T3>(in value3!);
            UnsafeWriteBlittable<T4>(in value4!);
            UnsafeWriteBlittable<T5>(in value5!);
            UnsafeWriteBlittable<T6>(in value6!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeWriteBlittable<T1, T2, T3, T4, T5, T6, T7>(
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
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
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
        else
        {
            UnsafeWriteBlittable<T1>(in value1!);
            UnsafeWriteBlittable<T2>(in value2!);
            UnsafeWriteBlittable<T3>(in value3!);
            UnsafeWriteBlittable<T4>(in value4!);
            UnsafeWriteBlittable<T5>(in value5!);
            UnsafeWriteBlittable<T6>(in value6!);
            UnsafeWriteBlittable<T7>(in value7!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeWriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8>(
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
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
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
        else
        {
            UnsafeWriteBlittable<T1>(in value1!);
            UnsafeWriteBlittable<T2>(in value2!);
            UnsafeWriteBlittable<T3>(in value3!);
            UnsafeWriteBlittable<T4>(in value4!);
            UnsafeWriteBlittable<T5>(in value5!);
            UnsafeWriteBlittable<T6>(in value6!);
            UnsafeWriteBlittable<T7>(in value7!);
            UnsafeWriteBlittable<T8>(in value8!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeWriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
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
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
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
        else
        {
            UnsafeWriteBlittable<T1>(in value1!);
            UnsafeWriteBlittable<T2>(in value2!);
            UnsafeWriteBlittable<T3>(in value3!);
            UnsafeWriteBlittable<T4>(in value4!);
            UnsafeWriteBlittable<T5>(in value5!);
            UnsafeWriteBlittable<T6>(in value6!);
            UnsafeWriteBlittable<T7>(in value7!);
            UnsafeWriteBlittable<T8>(in value8!);
            UnsafeWriteBlittable<T9>(in value9!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeWriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
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
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
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
        else
        {
            UnsafeWriteBlittable<T1>(in value1!);
            UnsafeWriteBlittable<T2>(in value2!);
            UnsafeWriteBlittable<T3>(in value3!);
            UnsafeWriteBlittable<T4>(in value4!);
            UnsafeWriteBlittable<T5>(in value5!);
            UnsafeWriteBlittable<T6>(in value6!);
            UnsafeWriteBlittable<T7>(in value7!);
            UnsafeWriteBlittable<T8>(in value8!);
            UnsafeWriteBlittable<T9>(in value9!);
            UnsafeWriteBlittable<T10>(in value10!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeWriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
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
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
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
        else
        {
            UnsafeWriteBlittable<T1>(in value1!);
            UnsafeWriteBlittable<T2>(in value2!);
            UnsafeWriteBlittable<T3>(in value3!);
            UnsafeWriteBlittable<T4>(in value4!);
            UnsafeWriteBlittable<T5>(in value5!);
            UnsafeWriteBlittable<T6>(in value6!);
            UnsafeWriteBlittable<T7>(in value7!);
            UnsafeWriteBlittable<T8>(in value8!);
            UnsafeWriteBlittable<T9>(in value9!);
            UnsafeWriteBlittable<T10>(in value10!);
            UnsafeWriteBlittable<T11>(in value11!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeWriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
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
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
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
        else
        {
            UnsafeWriteBlittable<T1>(in value1!);
            UnsafeWriteBlittable<T2>(in value2!);
            UnsafeWriteBlittable<T3>(in value3!);
            UnsafeWriteBlittable<T4>(in value4!);
            UnsafeWriteBlittable<T5>(in value5!);
            UnsafeWriteBlittable<T6>(in value6!);
            UnsafeWriteBlittable<T7>(in value7!);
            UnsafeWriteBlittable<T8>(in value8!);
            UnsafeWriteBlittable<T9>(in value9!);
            UnsafeWriteBlittable<T10>(in value10!);
            UnsafeWriteBlittable<T11>(in value11!);
            UnsafeWriteBlittable<T12>(in value12!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeWriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
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
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
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
        else
        {
            UnsafeWriteBlittable<T1>(in value1!);
            UnsafeWriteBlittable<T2>(in value2!);
            UnsafeWriteBlittable<T3>(in value3!);
            UnsafeWriteBlittable<T4>(in value4!);
            UnsafeWriteBlittable<T5>(in value5!);
            UnsafeWriteBlittable<T6>(in value6!);
            UnsafeWriteBlittable<T7>(in value7!);
            UnsafeWriteBlittable<T8>(in value8!);
            UnsafeWriteBlittable<T9>(in value9!);
            UnsafeWriteBlittable<T10>(in value10!);
            UnsafeWriteBlittable<T11>(in value11!);
            UnsafeWriteBlittable<T12>(in value12!);
            UnsafeWriteBlittable<T13>(in value13!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeWriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
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
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
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
        else
        {
            UnsafeWriteBlittable<T1>(in value1!);
            UnsafeWriteBlittable<T2>(in value2!);
            UnsafeWriteBlittable<T3>(in value3!);
            UnsafeWriteBlittable<T4>(in value4!);
            UnsafeWriteBlittable<T5>(in value5!);
            UnsafeWriteBlittable<T6>(in value6!);
            UnsafeWriteBlittable<T7>(in value7!);
            UnsafeWriteBlittable<T8>(in value8!);
            UnsafeWriteBlittable<T9>(in value9!);
            UnsafeWriteBlittable<T10>(in value10!);
            UnsafeWriteBlittable<T11>(in value11!);
            UnsafeWriteBlittable<T12>(in value12!);
            UnsafeWriteBlittable<T13>(in value13!);
            UnsafeWriteBlittable<T14>(in value14!);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeWriteBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
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
            Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
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
        else
        {
            UnsafeWriteBlittable<T1>(in value1!);
            UnsafeWriteBlittable<T2>(in value2!);
            UnsafeWriteBlittable<T3>(in value3!);
            UnsafeWriteBlittable<T4>(in value4!);
            UnsafeWriteBlittable<T5>(in value5!);
            UnsafeWriteBlittable<T6>(in value6!);
            UnsafeWriteBlittable<T7>(in value7!);
            UnsafeWriteBlittable<T8>(in value8!);
            UnsafeWriteBlittable<T9>(in value9!);
            UnsafeWriteBlittable<T10>(in value10!);
            UnsafeWriteBlittable<T11>(in value11!);
            UnsafeWriteBlittable<T12>(in value12!);
            UnsafeWriteBlittable<T13>(in value13!);
            UnsafeWriteBlittable<T14>(in value14!);
            UnsafeWriteBlittable<T15>(in value15!);
        }
    }
}
