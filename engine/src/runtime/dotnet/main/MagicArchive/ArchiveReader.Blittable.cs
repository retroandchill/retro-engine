using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MagicArchive.Utilities;

namespace MagicArchive;

public ref partial struct ArchiveReader
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ReadBlittable<T1, T2>(out T1 value1, out T2 value2)
        where T1 : unmanaged
        where T2 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>();
            ref var spanRef = ref GetSpanReference(size);
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            Advance(size);
        }
        else
        {
            value1 = ReadBlittable<T1>();
            value2 = ReadBlittable<T2>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ReadBlittable<T1, T2, T3>(out T1 value1, out T2 value2, out T3 value3)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>();
            ref var spanRef = ref GetSpanReference(size);
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            value3 = Unsafe.ReadUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()));
            Advance(size);
        }
        else
        {
            value1 = ReadBlittable<T1>();
            value2 = ReadBlittable<T2>();
            value3 = ReadBlittable<T3>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ReadBlittable<T1, T2, T3, T4>(out T1 value1, out T2 value2, out T3 value3, out T4 value4)
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
    {
        if (!IsByteSwapping)
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>();
            ref var spanRef = ref GetSpanReference(size);
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            value3 = Unsafe.ReadUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()));
            value4 = Unsafe.ReadUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>())
            );
            Advance(size);
        }
        else
        {
            value1 = ReadBlittable<T1>();
            value2 = ReadBlittable<T2>();
            value3 = ReadBlittable<T3>();
            value4 = ReadBlittable<T4>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ReadBlittable<T1, T2, T3, T4, T5>(
        out T1 value1,
        out T2 value2,
        out T3 value3,
        out T4 value4,
        out T5 value5
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
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            value3 = Unsafe.ReadUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()));
            value4 = Unsafe.ReadUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>())
            );
            value5 = Unsafe.ReadUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                )
            );
            Advance(size);
        }
        else
        {
            value1 = ReadBlittable<T1>();
            value2 = ReadBlittable<T2>();
            value3 = ReadBlittable<T3>();
            value4 = ReadBlittable<T4>();
            value5 = ReadBlittable<T5>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ReadBlittable<T1, T2, T3, T4, T5, T6>(
        out T1 value1,
        out T2 value2,
        out T3 value3,
        out T4 value4,
        out T5 value5,
        out T6 value6
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
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            value3 = Unsafe.ReadUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()));
            value4 = Unsafe.ReadUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>())
            );
            value5 = Unsafe.ReadUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                )
            );
            value6 = Unsafe.ReadUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                )
            );
            Advance(size);
        }
        else
        {
            value1 = ReadBlittable<T1>();
            value2 = ReadBlittable<T2>();
            value3 = ReadBlittable<T3>();
            value4 = ReadBlittable<T4>();
            value5 = ReadBlittable<T5>();
            value6 = ReadBlittable<T6>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ReadBlittable<T1, T2, T3, T4, T5, T6, T7>(
        out T1 value1,
        out T2 value2,
        out T3 value3,
        out T4 value4,
        out T5 value5,
        out T6 value6,
        out T7 value7
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
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            value3 = Unsafe.ReadUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()));
            value4 = Unsafe.ReadUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>())
            );
            value5 = Unsafe.ReadUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                )
            );
            value6 = Unsafe.ReadUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                )
            );
            value7 = Unsafe.ReadUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                )
            );
            Advance(size);
        }
        else
        {
            value1 = ReadBlittable<T1>();
            value2 = ReadBlittable<T2>();
            value3 = ReadBlittable<T3>();
            value4 = ReadBlittable<T4>();
            value5 = ReadBlittable<T5>();
            value6 = ReadBlittable<T6>();
            value7 = ReadBlittable<T7>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ReadBlittable<T1, T2, T3, T4, T5, T6, T7, T8>(
        out T1 value1,
        out T2 value2,
        out T3 value3,
        out T4 value4,
        out T5 value5,
        out T6 value6,
        out T7 value7,
        out T8 value8
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
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            value3 = Unsafe.ReadUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()));
            value4 = Unsafe.ReadUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>())
            );
            value5 = Unsafe.ReadUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                )
            );
            value6 = Unsafe.ReadUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                )
            );
            value7 = Unsafe.ReadUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                )
            );
            value8 = Unsafe.ReadUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                )
            );
            Advance(size);
        }
        else
        {
            value1 = ReadBlittable<T1>();
            value2 = ReadBlittable<T2>();
            value3 = ReadBlittable<T3>();
            value4 = ReadBlittable<T4>();
            value5 = ReadBlittable<T5>();
            value6 = ReadBlittable<T6>();
            value7 = ReadBlittable<T7>();
            value8 = ReadBlittable<T8>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ReadBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        out T1 value1,
        out T2 value2,
        out T3 value3,
        out T4 value4,
        out T5 value5,
        out T6 value6,
        out T7 value7,
        out T8 value8,
        out T9 value9
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
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            value3 = Unsafe.ReadUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()));
            value4 = Unsafe.ReadUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>())
            );
            value5 = Unsafe.ReadUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                )
            );
            value6 = Unsafe.ReadUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                )
            );
            value7 = Unsafe.ReadUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                )
            );
            value8 = Unsafe.ReadUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                )
            );
            value9 = Unsafe.ReadUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                )
            );
            Advance(size);
        }
        else
        {
            value1 = ReadBlittable<T1>();
            value2 = ReadBlittable<T2>();
            value3 = ReadBlittable<T3>();
            value4 = ReadBlittable<T4>();
            value5 = ReadBlittable<T5>();
            value6 = ReadBlittable<T6>();
            value7 = ReadBlittable<T7>();
            value8 = ReadBlittable<T8>();
            value9 = ReadBlittable<T9>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ReadBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        out T1 value1,
        out T2 value2,
        out T3 value3,
        out T4 value4,
        out T5 value5,
        out T6 value6,
        out T7 value7,
        out T8 value8,
        out T9 value9,
        out T10 value10
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
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            value3 = Unsafe.ReadUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()));
            value4 = Unsafe.ReadUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>())
            );
            value5 = Unsafe.ReadUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                )
            );
            value6 = Unsafe.ReadUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                )
            );
            value7 = Unsafe.ReadUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                )
            );
            value8 = Unsafe.ReadUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                )
            );
            value9 = Unsafe.ReadUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                )
            );
            value10 = Unsafe.ReadUnaligned<T10>(
                ref Unsafe.Add(
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
                )
            );
            Advance(size);
        }
        else
        {
            value1 = ReadBlittable<T1>();
            value2 = ReadBlittable<T2>();
            value3 = ReadBlittable<T3>();
            value4 = ReadBlittable<T4>();
            value5 = ReadBlittable<T5>();
            value6 = ReadBlittable<T6>();
            value7 = ReadBlittable<T7>();
            value8 = ReadBlittable<T8>();
            value9 = ReadBlittable<T9>();
            value10 = ReadBlittable<T10>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ReadBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        out T1 value1,
        out T2 value2,
        out T3 value3,
        out T4 value4,
        out T5 value5,
        out T6 value6,
        out T7 value7,
        out T8 value8,
        out T9 value9,
        out T10 value10,
        out T11 value11
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
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            value3 = Unsafe.ReadUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()));
            value4 = Unsafe.ReadUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>())
            );
            value5 = Unsafe.ReadUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                )
            );
            value6 = Unsafe.ReadUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                )
            );
            value7 = Unsafe.ReadUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                )
            );
            value8 = Unsafe.ReadUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                )
            );
            value9 = Unsafe.ReadUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                )
            );
            value10 = Unsafe.ReadUnaligned<T10>(
                ref Unsafe.Add(
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
                )
            );
            value11 = Unsafe.ReadUnaligned<T11>(
                ref Unsafe.Add(
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
                )
            );
            Advance(size);
        }
        else
        {
            value1 = ReadBlittable<T1>();
            value2 = ReadBlittable<T2>();
            value3 = ReadBlittable<T3>();
            value4 = ReadBlittable<T4>();
            value5 = ReadBlittable<T5>();
            value6 = ReadBlittable<T6>();
            value7 = ReadBlittable<T7>();
            value8 = ReadBlittable<T8>();
            value9 = ReadBlittable<T9>();
            value10 = ReadBlittable<T10>();
            value11 = ReadBlittable<T11>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ReadBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        out T1 value1,
        out T2 value2,
        out T3 value3,
        out T4 value4,
        out T5 value5,
        out T6 value6,
        out T7 value7,
        out T8 value8,
        out T9 value9,
        out T10 value10,
        out T11 value11,
        out T12 value12
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
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            value3 = Unsafe.ReadUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()));
            value4 = Unsafe.ReadUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>())
            );
            value5 = Unsafe.ReadUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                )
            );
            value6 = Unsafe.ReadUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                )
            );
            value7 = Unsafe.ReadUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                )
            );
            value8 = Unsafe.ReadUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                )
            );
            value9 = Unsafe.ReadUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                )
            );
            value10 = Unsafe.ReadUnaligned<T10>(
                ref Unsafe.Add(
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
                )
            );
            value11 = Unsafe.ReadUnaligned<T11>(
                ref Unsafe.Add(
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
                )
            );
            value12 = Unsafe.ReadUnaligned<T12>(
                ref Unsafe.Add(
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
                )
            );
            Advance(size);
        }
        else
        {
            value1 = ReadBlittable<T1>();
            value2 = ReadBlittable<T2>();
            value3 = ReadBlittable<T3>();
            value4 = ReadBlittable<T4>();
            value5 = ReadBlittable<T5>();
            value6 = ReadBlittable<T6>();
            value7 = ReadBlittable<T7>();
            value8 = ReadBlittable<T8>();
            value9 = ReadBlittable<T9>();
            value10 = ReadBlittable<T10>();
            value11 = ReadBlittable<T11>();
            value12 = ReadBlittable<T12>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ReadBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        out T1 value1,
        out T2 value2,
        out T3 value3,
        out T4 value4,
        out T5 value5,
        out T6 value6,
        out T7 value7,
        out T8 value8,
        out T9 value9,
        out T10 value10,
        out T11 value11,
        out T12 value12,
        out T13 value13
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
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            value3 = Unsafe.ReadUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()));
            value4 = Unsafe.ReadUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>())
            );
            value5 = Unsafe.ReadUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                )
            );
            value6 = Unsafe.ReadUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                )
            );
            value7 = Unsafe.ReadUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                )
            );
            value8 = Unsafe.ReadUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                )
            );
            value9 = Unsafe.ReadUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                )
            );
            value10 = Unsafe.ReadUnaligned<T10>(
                ref Unsafe.Add(
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
                )
            );
            value11 = Unsafe.ReadUnaligned<T11>(
                ref Unsafe.Add(
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
                )
            );
            value12 = Unsafe.ReadUnaligned<T12>(
                ref Unsafe.Add(
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
                )
            );
            value13 = Unsafe.ReadUnaligned<T13>(
                ref Unsafe.Add(
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
                )
            );
            Advance(size);
        }
        else
        {
            value1 = ReadBlittable<T1>();
            value2 = ReadBlittable<T2>();
            value3 = ReadBlittable<T3>();
            value4 = ReadBlittable<T4>();
            value5 = ReadBlittable<T5>();
            value6 = ReadBlittable<T6>();
            value7 = ReadBlittable<T7>();
            value8 = ReadBlittable<T8>();
            value9 = ReadBlittable<T9>();
            value10 = ReadBlittable<T10>();
            value11 = ReadBlittable<T11>();
            value12 = ReadBlittable<T12>();
            value13 = ReadBlittable<T13>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ReadBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        out T1 value1,
        out T2 value2,
        out T3 value3,
        out T4 value4,
        out T5 value5,
        out T6 value6,
        out T7 value7,
        out T8 value8,
        out T9 value9,
        out T10 value10,
        out T11 value11,
        out T12 value12,
        out T13 value13,
        out T14 value14
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
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            value3 = Unsafe.ReadUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()));
            value4 = Unsafe.ReadUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>())
            );
            value5 = Unsafe.ReadUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                )
            );
            value6 = Unsafe.ReadUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                )
            );
            value7 = Unsafe.ReadUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                )
            );
            value8 = Unsafe.ReadUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                )
            );
            value9 = Unsafe.ReadUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                )
            );
            value10 = Unsafe.ReadUnaligned<T10>(
                ref Unsafe.Add(
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
                )
            );
            value11 = Unsafe.ReadUnaligned<T11>(
                ref Unsafe.Add(
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
                )
            );
            value12 = Unsafe.ReadUnaligned<T12>(
                ref Unsafe.Add(
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
                )
            );
            value13 = Unsafe.ReadUnaligned<T13>(
                ref Unsafe.Add(
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
                )
            );
            value14 = Unsafe.ReadUnaligned<T14>(
                ref Unsafe.Add(
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
                )
            );
            Advance(size);
        }
        else
        {
            value1 = ReadBlittable<T1>();
            value2 = ReadBlittable<T2>();
            value3 = ReadBlittable<T3>();
            value4 = ReadBlittable<T4>();
            value5 = ReadBlittable<T5>();
            value6 = ReadBlittable<T6>();
            value7 = ReadBlittable<T7>();
            value8 = ReadBlittable<T8>();
            value9 = ReadBlittable<T9>();
            value10 = ReadBlittable<T10>();
            value11 = ReadBlittable<T11>();
            value12 = ReadBlittable<T12>();
            value13 = ReadBlittable<T13>();
            value14 = ReadBlittable<T14>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ReadBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
        out T1 value1,
        out T2 value2,
        out T3 value3,
        out T4 value4,
        out T5 value5,
        out T6 value6,
        out T7 value7,
        out T8 value8,
        out T9 value9,
        out T10 value10,
        out T11 value11,
        out T12 value12,
        out T13 value13,
        out T14 value14,
        out T15 value15
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
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            value3 = Unsafe.ReadUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()));
            value4 = Unsafe.ReadUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>())
            );
            value5 = Unsafe.ReadUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                )
            );
            value6 = Unsafe.ReadUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                )
            );
            value7 = Unsafe.ReadUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                )
            );
            value8 = Unsafe.ReadUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                )
            );
            value9 = Unsafe.ReadUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                )
            );
            value10 = Unsafe.ReadUnaligned<T10>(
                ref Unsafe.Add(
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
                )
            );
            value11 = Unsafe.ReadUnaligned<T11>(
                ref Unsafe.Add(
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
                )
            );
            value12 = Unsafe.ReadUnaligned<T12>(
                ref Unsafe.Add(
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
                )
            );
            value13 = Unsafe.ReadUnaligned<T13>(
                ref Unsafe.Add(
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
                )
            );
            value14 = Unsafe.ReadUnaligned<T14>(
                ref Unsafe.Add(
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
                )
            );
            value15 = Unsafe.ReadUnaligned<T15>(
                ref Unsafe.Add(
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
                )
            );
            Advance(size);
        }
        else
        {
            value1 = ReadBlittable<T1>();
            value2 = ReadBlittable<T2>();
            value3 = ReadBlittable<T3>();
            value4 = ReadBlittable<T4>();
            value5 = ReadBlittable<T5>();
            value6 = ReadBlittable<T6>();
            value7 = ReadBlittable<T7>();
            value8 = ReadBlittable<T8>();
            value9 = ReadBlittable<T9>();
            value10 = ReadBlittable<T10>();
            value11 = ReadBlittable<T11>();
            value12 = ReadBlittable<T12>();
            value13 = ReadBlittable<T13>();
            value14 = ReadBlittable<T14>();
            value15 = ReadBlittable<T15>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeReadBlittable<T1, T2>(out T1 value1, out T2 value2)
    {
        if (!IsByteSwapping)
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>();
            ref var spanRef = ref GetSpanReference(size);
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            Advance(size);
        }
        else
        {
            value1 = UnsafeReadBlittable<T1>();
            value2 = UnsafeReadBlittable<T2>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeReadBlittable<T1, T2, T3>(out T1 value1, out T2 value2, out T3 value3)
    {
        if (!IsByteSwapping)
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>();
            ref var spanRef = ref GetSpanReference(size);
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            value3 = Unsafe.ReadUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()));
            Advance(size);
        }
        else
        {
            value1 = UnsafeReadBlittable<T1>();
            value2 = UnsafeReadBlittable<T2>();
            value3 = UnsafeReadBlittable<T3>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeReadBlittable<T1, T2, T3, T4>(out T1 value1, out T2 value2, out T3 value3, out T4 value4)
    {
        if (!IsByteSwapping)
        {
            var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>();
            ref var spanRef = ref GetSpanReference(size);
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            value3 = Unsafe.ReadUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()));
            value4 = Unsafe.ReadUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>())
            );
            Advance(size);
        }
        else
        {
            value1 = UnsafeReadBlittable<T1>();
            value2 = UnsafeReadBlittable<T2>();
            value3 = UnsafeReadBlittable<T3>();
            value4 = UnsafeReadBlittable<T4>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeReadBlittable<T1, T2, T3, T4, T5>(
        out T1 value1,
        out T2 value2,
        out T3 value3,
        out T4 value4,
        out T5 value5
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
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            value3 = Unsafe.ReadUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()));
            value4 = Unsafe.ReadUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>())
            );
            value5 = Unsafe.ReadUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                )
            );
            Advance(size);
        }
        else
        {
            value1 = UnsafeReadBlittable<T1>();
            value2 = UnsafeReadBlittable<T2>();
            value3 = UnsafeReadBlittable<T3>();
            value4 = UnsafeReadBlittable<T4>();
            value5 = UnsafeReadBlittable<T5>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeReadBlittable<T1, T2, T3, T4, T5, T6>(
        out T1 value1,
        out T2 value2,
        out T3 value3,
        out T4 value4,
        out T5 value5,
        out T6 value6
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
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            value3 = Unsafe.ReadUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()));
            value4 = Unsafe.ReadUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>())
            );
            value5 = Unsafe.ReadUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                )
            );
            value6 = Unsafe.ReadUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                )
            );
            Advance(size);
        }
        else
        {
            value1 = UnsafeReadBlittable<T1>();
            value2 = UnsafeReadBlittable<T2>();
            value3 = UnsafeReadBlittable<T3>();
            value4 = UnsafeReadBlittable<T4>();
            value5 = UnsafeReadBlittable<T5>();
            value6 = UnsafeReadBlittable<T6>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeReadBlittable<T1, T2, T3, T4, T5, T6, T7>(
        out T1 value1,
        out T2 value2,
        out T3 value3,
        out T4 value4,
        out T5 value5,
        out T6 value6,
        out T7 value7
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
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            value3 = Unsafe.ReadUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()));
            value4 = Unsafe.ReadUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>())
            );
            value5 = Unsafe.ReadUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                )
            );
            value6 = Unsafe.ReadUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                )
            );
            value7 = Unsafe.ReadUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                )
            );
            Advance(size);
        }
        else
        {
            value1 = UnsafeReadBlittable<T1>();
            value2 = UnsafeReadBlittable<T2>();
            value3 = UnsafeReadBlittable<T3>();
            value4 = UnsafeReadBlittable<T4>();
            value5 = UnsafeReadBlittable<T5>();
            value6 = UnsafeReadBlittable<T6>();
            value7 = UnsafeReadBlittable<T7>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeReadBlittable<T1, T2, T3, T4, T5, T6, T7, T8>(
        out T1 value1,
        out T2 value2,
        out T3 value3,
        out T4 value4,
        out T5 value5,
        out T6 value6,
        out T7 value7,
        out T8 value8
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
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            value3 = Unsafe.ReadUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()));
            value4 = Unsafe.ReadUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>())
            );
            value5 = Unsafe.ReadUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                )
            );
            value6 = Unsafe.ReadUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                )
            );
            value7 = Unsafe.ReadUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                )
            );
            value8 = Unsafe.ReadUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                )
            );
            Advance(size);
        }
        else
        {
            value1 = UnsafeReadBlittable<T1>();
            value2 = UnsafeReadBlittable<T2>();
            value3 = UnsafeReadBlittable<T3>();
            value4 = UnsafeReadBlittable<T4>();
            value5 = UnsafeReadBlittable<T5>();
            value6 = UnsafeReadBlittable<T6>();
            value7 = UnsafeReadBlittable<T7>();
            value8 = UnsafeReadBlittable<T8>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeReadBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        out T1 value1,
        out T2 value2,
        out T3 value3,
        out T4 value4,
        out T5 value5,
        out T6 value6,
        out T7 value7,
        out T8 value8,
        out T9 value9
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
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            value3 = Unsafe.ReadUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()));
            value4 = Unsafe.ReadUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>())
            );
            value5 = Unsafe.ReadUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                )
            );
            value6 = Unsafe.ReadUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                )
            );
            value7 = Unsafe.ReadUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                )
            );
            value8 = Unsafe.ReadUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                )
            );
            value9 = Unsafe.ReadUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                )
            );
            Advance(size);
        }
        else
        {
            value1 = UnsafeReadBlittable<T1>();
            value2 = UnsafeReadBlittable<T2>();
            value3 = UnsafeReadBlittable<T3>();
            value4 = UnsafeReadBlittable<T4>();
            value5 = UnsafeReadBlittable<T5>();
            value6 = UnsafeReadBlittable<T6>();
            value7 = UnsafeReadBlittable<T7>();
            value8 = UnsafeReadBlittable<T8>();
            value9 = UnsafeReadBlittable<T9>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeReadBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        out T1 value1,
        out T2 value2,
        out T3 value3,
        out T4 value4,
        out T5 value5,
        out T6 value6,
        out T7 value7,
        out T8 value8,
        out T9 value9,
        out T10 value10
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
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            value3 = Unsafe.ReadUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()));
            value4 = Unsafe.ReadUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>())
            );
            value5 = Unsafe.ReadUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                )
            );
            value6 = Unsafe.ReadUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                )
            );
            value7 = Unsafe.ReadUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                )
            );
            value8 = Unsafe.ReadUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                )
            );
            value9 = Unsafe.ReadUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                )
            );
            value10 = Unsafe.ReadUnaligned<T10>(
                ref Unsafe.Add(
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
                )
            );
            Advance(size);
        }
        else
        {
            value1 = UnsafeReadBlittable<T1>();
            value2 = UnsafeReadBlittable<T2>();
            value3 = UnsafeReadBlittable<T3>();
            value4 = UnsafeReadBlittable<T4>();
            value5 = UnsafeReadBlittable<T5>();
            value6 = UnsafeReadBlittable<T6>();
            value7 = UnsafeReadBlittable<T7>();
            value8 = UnsafeReadBlittable<T8>();
            value9 = UnsafeReadBlittable<T9>();
            value10 = UnsafeReadBlittable<T10>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeReadBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        out T1 value1,
        out T2 value2,
        out T3 value3,
        out T4 value4,
        out T5 value5,
        out T6 value6,
        out T7 value7,
        out T8 value8,
        out T9 value9,
        out T10 value10,
        out T11 value11
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
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            value3 = Unsafe.ReadUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()));
            value4 = Unsafe.ReadUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>())
            );
            value5 = Unsafe.ReadUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                )
            );
            value6 = Unsafe.ReadUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                )
            );
            value7 = Unsafe.ReadUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                )
            );
            value8 = Unsafe.ReadUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                )
            );
            value9 = Unsafe.ReadUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                )
            );
            value10 = Unsafe.ReadUnaligned<T10>(
                ref Unsafe.Add(
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
                )
            );
            value11 = Unsafe.ReadUnaligned<T11>(
                ref Unsafe.Add(
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
                )
            );
            Advance(size);
        }
        else
        {
            value1 = UnsafeReadBlittable<T1>();
            value2 = UnsafeReadBlittable<T2>();
            value3 = UnsafeReadBlittable<T3>();
            value4 = UnsafeReadBlittable<T4>();
            value5 = UnsafeReadBlittable<T5>();
            value6 = UnsafeReadBlittable<T6>();
            value7 = UnsafeReadBlittable<T7>();
            value8 = UnsafeReadBlittable<T8>();
            value9 = UnsafeReadBlittable<T9>();
            value10 = UnsafeReadBlittable<T10>();
            value11 = UnsafeReadBlittable<T11>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeReadBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        out T1 value1,
        out T2 value2,
        out T3 value3,
        out T4 value4,
        out T5 value5,
        out T6 value6,
        out T7 value7,
        out T8 value8,
        out T9 value9,
        out T10 value10,
        out T11 value11,
        out T12 value12
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
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            value3 = Unsafe.ReadUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()));
            value4 = Unsafe.ReadUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>())
            );
            value5 = Unsafe.ReadUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                )
            );
            value6 = Unsafe.ReadUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                )
            );
            value7 = Unsafe.ReadUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                )
            );
            value8 = Unsafe.ReadUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                )
            );
            value9 = Unsafe.ReadUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                )
            );
            value10 = Unsafe.ReadUnaligned<T10>(
                ref Unsafe.Add(
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
                )
            );
            value11 = Unsafe.ReadUnaligned<T11>(
                ref Unsafe.Add(
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
                )
            );
            value12 = Unsafe.ReadUnaligned<T12>(
                ref Unsafe.Add(
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
                )
            );
            Advance(size);
        }
        else
        {
            value1 = UnsafeReadBlittable<T1>();
            value2 = UnsafeReadBlittable<T2>();
            value3 = UnsafeReadBlittable<T3>();
            value4 = UnsafeReadBlittable<T4>();
            value5 = UnsafeReadBlittable<T5>();
            value6 = UnsafeReadBlittable<T6>();
            value7 = UnsafeReadBlittable<T7>();
            value8 = UnsafeReadBlittable<T8>();
            value9 = UnsafeReadBlittable<T9>();
            value10 = UnsafeReadBlittable<T10>();
            value11 = UnsafeReadBlittable<T11>();
            value12 = UnsafeReadBlittable<T12>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeReadBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        out T1 value1,
        out T2 value2,
        out T3 value3,
        out T4 value4,
        out T5 value5,
        out T6 value6,
        out T7 value7,
        out T8 value8,
        out T9 value9,
        out T10 value10,
        out T11 value11,
        out T12 value12,
        out T13 value13
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
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            value3 = Unsafe.ReadUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()));
            value4 = Unsafe.ReadUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>())
            );
            value5 = Unsafe.ReadUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                )
            );
            value6 = Unsafe.ReadUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                )
            );
            value7 = Unsafe.ReadUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                )
            );
            value8 = Unsafe.ReadUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                )
            );
            value9 = Unsafe.ReadUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                )
            );
            value10 = Unsafe.ReadUnaligned<T10>(
                ref Unsafe.Add(
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
                )
            );
            value11 = Unsafe.ReadUnaligned<T11>(
                ref Unsafe.Add(
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
                )
            );
            value12 = Unsafe.ReadUnaligned<T12>(
                ref Unsafe.Add(
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
                )
            );
            value13 = Unsafe.ReadUnaligned<T13>(
                ref Unsafe.Add(
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
                )
            );
            Advance(size);
        }
        else
        {
            value1 = UnsafeReadBlittable<T1>();
            value2 = UnsafeReadBlittable<T2>();
            value3 = UnsafeReadBlittable<T3>();
            value4 = UnsafeReadBlittable<T4>();
            value5 = UnsafeReadBlittable<T5>();
            value6 = UnsafeReadBlittable<T6>();
            value7 = UnsafeReadBlittable<T7>();
            value8 = UnsafeReadBlittable<T8>();
            value9 = UnsafeReadBlittable<T9>();
            value10 = UnsafeReadBlittable<T10>();
            value11 = UnsafeReadBlittable<T11>();
            value12 = UnsafeReadBlittable<T12>();
            value13 = UnsafeReadBlittable<T13>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeReadBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        out T1 value1,
        out T2 value2,
        out T3 value3,
        out T4 value4,
        out T5 value5,
        out T6 value6,
        out T7 value7,
        out T8 value8,
        out T9 value9,
        out T10 value10,
        out T11 value11,
        out T12 value12,
        out T13 value13,
        out T14 value14
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
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            value3 = Unsafe.ReadUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()));
            value4 = Unsafe.ReadUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>())
            );
            value5 = Unsafe.ReadUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                )
            );
            value6 = Unsafe.ReadUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                )
            );
            value7 = Unsafe.ReadUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                )
            );
            value8 = Unsafe.ReadUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                )
            );
            value9 = Unsafe.ReadUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                )
            );
            value10 = Unsafe.ReadUnaligned<T10>(
                ref Unsafe.Add(
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
                )
            );
            value11 = Unsafe.ReadUnaligned<T11>(
                ref Unsafe.Add(
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
                )
            );
            value12 = Unsafe.ReadUnaligned<T12>(
                ref Unsafe.Add(
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
                )
            );
            value13 = Unsafe.ReadUnaligned<T13>(
                ref Unsafe.Add(
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
                )
            );
            value14 = Unsafe.ReadUnaligned<T14>(
                ref Unsafe.Add(
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
                )
            );
            Advance(size);
        }
        else
        {
            value1 = UnsafeReadBlittable<T1>();
            value2 = UnsafeReadBlittable<T2>();
            value3 = UnsafeReadBlittable<T3>();
            value4 = UnsafeReadBlittable<T4>();
            value5 = UnsafeReadBlittable<T5>();
            value6 = UnsafeReadBlittable<T6>();
            value7 = UnsafeReadBlittable<T7>();
            value8 = UnsafeReadBlittable<T8>();
            value9 = UnsafeReadBlittable<T9>();
            value10 = UnsafeReadBlittable<T10>();
            value11 = UnsafeReadBlittable<T11>();
            value12 = UnsafeReadBlittable<T12>();
            value13 = UnsafeReadBlittable<T13>();
            value14 = UnsafeReadBlittable<T14>();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UnsafeReadBlittable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
        out T1 value1,
        out T2 value2,
        out T3 value3,
        out T4 value4,
        out T5 value5,
        out T6 value6,
        out T7 value7,
        out T8 value8,
        out T9 value9,
        out T10 value10,
        out T11 value11,
        out T12 value12,
        out T13 value13,
        out T14 value14,
        out T15 value15
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
            value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
            value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
            value3 = Unsafe.ReadUnaligned<T3>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()));
            value4 = Unsafe.ReadUnaligned<T4>(
                ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>())
            );
            value5 = Unsafe.ReadUnaligned<T5>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                )
            );
            value6 = Unsafe.ReadUnaligned<T6>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                )
            );
            value7 = Unsafe.ReadUnaligned<T7>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                )
            );
            value8 = Unsafe.ReadUnaligned<T8>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                )
            );
            value9 = Unsafe.ReadUnaligned<T9>(
                ref Unsafe.Add(
                    ref spanRef,
                    Unsafe.SizeOf<T1>()
                        + Unsafe.SizeOf<T2>()
                        + Unsafe.SizeOf<T3>()
                        + Unsafe.SizeOf<T4>()
                        + Unsafe.SizeOf<T5>()
                        + Unsafe.SizeOf<T6>()
                        + Unsafe.SizeOf<T7>()
                        + Unsafe.SizeOf<T8>()
                )
            );
            value10 = Unsafe.ReadUnaligned<T10>(
                ref Unsafe.Add(
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
                )
            );
            value11 = Unsafe.ReadUnaligned<T11>(
                ref Unsafe.Add(
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
                )
            );
            value12 = Unsafe.ReadUnaligned<T12>(
                ref Unsafe.Add(
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
                )
            );
            value13 = Unsafe.ReadUnaligned<T13>(
                ref Unsafe.Add(
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
                )
            );
            value14 = Unsafe.ReadUnaligned<T14>(
                ref Unsafe.Add(
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
                )
            );
            value15 = Unsafe.ReadUnaligned<T15>(
                ref Unsafe.Add(
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
                )
            );
            Advance(size);
        }
        else
        {
            value1 = UnsafeReadBlittable<T1>();
            value2 = UnsafeReadBlittable<T2>();
            value3 = UnsafeReadBlittable<T3>();
            value4 = UnsafeReadBlittable<T4>();
            value5 = UnsafeReadBlittable<T5>();
            value6 = UnsafeReadBlittable<T6>();
            value7 = UnsafeReadBlittable<T7>();
            value8 = UnsafeReadBlittable<T8>();
            value9 = UnsafeReadBlittable<T9>();
            value10 = UnsafeReadBlittable<T10>();
            value11 = UnsafeReadBlittable<T11>();
            value12 = UnsafeReadBlittable<T12>();
            value13 = UnsafeReadBlittable<T13>();
            value14 = UnsafeReadBlittable<T14>();
            value15 = UnsafeReadBlittable<T15>();
        }
    }
}
