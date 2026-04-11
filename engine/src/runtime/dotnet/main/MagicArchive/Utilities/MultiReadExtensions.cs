using System.Runtime.CompilerServices;

namespace MagicArchive.Utilities;

public static class MultiReadExtensions
{
    extension(ref ArchiveReader reader)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T1>(out T1? value1)
        {
            if (!reader.IsByteSwapping && BinaryHandling.IsBlittable<T1>())
            {
                var size = Unsafe.SizeOf<T1>();
                ref var spanRef = ref reader.GetSpanReference(size);
                value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
                reader.Advance(size);
            }
            else
            {
                value1 = reader.Read<T1>();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T1, T2>(out T1? value1, out T2? value2)
        {
            if (!reader.IsByteSwapping && BinaryHandling.IsBlittable<T1>() && BinaryHandling.IsBlittable<T2>())
            {
                var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>();
                ref var spanRef = ref reader.GetSpanReference(size);
                value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
                value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
                reader.Advance(size);
            }
            else
            {
                value1 = reader.Read<T1>();
                value2 = reader.Read<T2>();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T1, T2, T3>(out T1? value1, out T2? value2, out T3? value3)
        {
            if (
                !reader.IsByteSwapping
                && BinaryHandling.IsBlittable<T1>()
                && BinaryHandling.IsBlittable<T2>()
                && BinaryHandling.IsBlittable<T3>()
            )
            {
                var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>();
                ref var spanRef = ref reader.GetSpanReference(size);
                value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
                value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
                value3 = Unsafe.ReadUnaligned<T3>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>())
                );
                reader.Advance(size);
            }
            else
            {
                value1 = reader.Read<T1>();
                value2 = reader.Read<T2>();
                value3 = reader.Read<T3>();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T1, T2, T3, T4>(out T1? value1, out T2? value2, out T3? value3, out T4? value4)
        {
            if (
                !reader.IsByteSwapping
                && BinaryHandling.IsBlittable<T1>()
                && BinaryHandling.IsBlittable<T2>()
                && BinaryHandling.IsBlittable<T3>()
                && BinaryHandling.IsBlittable<T4>()
            )
            {
                var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>();
                ref var spanRef = ref reader.GetSpanReference(size);
                value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
                value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
                value3 = Unsafe.ReadUnaligned<T3>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>())
                );
                value4 = Unsafe.ReadUnaligned<T4>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>())
                );
                reader.Advance(size);
            }
            else
            {
                value1 = reader.Read<T1>();
                value2 = reader.Read<T2>();
                value3 = reader.Read<T3>();
                value4 = reader.Read<T4>();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T1, T2, T3, T4, T5>(
            out T1? value1,
            out T2? value2,
            out T3? value3,
            out T4? value4,
            out T5? value5
        )
        {
            if (
                !reader.IsByteSwapping
                && BinaryHandling.IsBlittable<T1>()
                && BinaryHandling.IsBlittable<T2>()
                && BinaryHandling.IsBlittable<T3>()
                && BinaryHandling.IsBlittable<T4>()
                && BinaryHandling.IsBlittable<T5>()
            )
            {
                var size =
                    Unsafe.SizeOf<T1>()
                    + Unsafe.SizeOf<T2>()
                    + Unsafe.SizeOf<T3>()
                    + Unsafe.SizeOf<T4>()
                    + Unsafe.SizeOf<T5>();
                ref var spanRef = ref reader.GetSpanReference(size);
                value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
                value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
                value3 = Unsafe.ReadUnaligned<T3>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>())
                );
                value4 = Unsafe.ReadUnaligned<T4>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>())
                );
                value5 = Unsafe.ReadUnaligned<T5>(
                    ref Unsafe.Add(
                        ref spanRef,
                        Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>()
                    )
                );
                reader.Advance(size);
            }
            else
            {
                value1 = reader.Read<T1>();
                value2 = reader.Read<T2>();
                value3 = reader.Read<T3>();
                value4 = reader.Read<T4>();
                value5 = reader.Read<T5>();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T1, T2, T3, T4, T5, T6>(
            out T1? value1,
            out T2? value2,
            out T3? value3,
            out T4? value4,
            out T5? value5,
            out T6? value6
        )
        {
            if (
                !reader.IsByteSwapping
                && BinaryHandling.IsBlittable<T1>()
                && BinaryHandling.IsBlittable<T2>()
                && BinaryHandling.IsBlittable<T3>()
                && BinaryHandling.IsBlittable<T4>()
                && BinaryHandling.IsBlittable<T5>()
                && BinaryHandling.IsBlittable<T6>()
            )
            {
                var size =
                    Unsafe.SizeOf<T1>()
                    + Unsafe.SizeOf<T2>()
                    + Unsafe.SizeOf<T3>()
                    + Unsafe.SizeOf<T4>()
                    + Unsafe.SizeOf<T5>()
                    + Unsafe.SizeOf<T6>();
                ref var spanRef = ref reader.GetSpanReference(size);
                value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
                value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
                value3 = Unsafe.ReadUnaligned<T3>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>())
                );
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
                reader.Advance(size);
            }
            else
            {
                value1 = reader.Read<T1>();
                value2 = reader.Read<T2>();
                value3 = reader.Read<T3>();
                value4 = reader.Read<T4>();
                value5 = reader.Read<T5>();
                value6 = reader.Read<T6>();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T1, T2, T3, T4, T5, T6, T7>(
            out T1? value1,
            out T2? value2,
            out T3? value3,
            out T4? value4,
            out T5? value5,
            out T6? value6,
            out T7? value7
        )
        {
            if (
                !reader.IsByteSwapping
                && BinaryHandling.IsBlittable<T1>()
                && BinaryHandling.IsBlittable<T2>()
                && BinaryHandling.IsBlittable<T3>()
                && BinaryHandling.IsBlittable<T4>()
                && BinaryHandling.IsBlittable<T5>()
                && BinaryHandling.IsBlittable<T6>()
                && BinaryHandling.IsBlittable<T7>()
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
                ref var spanRef = ref reader.GetSpanReference(size);
                value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
                value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
                value3 = Unsafe.ReadUnaligned<T3>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>())
                );
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
                reader.Advance(size);
            }
            else
            {
                value1 = reader.Read<T1>();
                value2 = reader.Read<T2>();
                value3 = reader.Read<T3>();
                value4 = reader.Read<T4>();
                value5 = reader.Read<T5>();
                value6 = reader.Read<T6>();
                value7 = reader.Read<T7>();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T1, T2, T3, T4, T5, T6, T7, T8>(
            out T1? value1,
            out T2? value2,
            out T3? value3,
            out T4? value4,
            out T5? value5,
            out T6? value6,
            out T7? value7,
            out T8? value8
        )
        {
            if (
                !reader.IsByteSwapping
                && BinaryHandling.IsBlittable<T1>()
                && BinaryHandling.IsBlittable<T2>()
                && BinaryHandling.IsBlittable<T3>()
                && BinaryHandling.IsBlittable<T4>()
                && BinaryHandling.IsBlittable<T5>()
                && BinaryHandling.IsBlittable<T6>()
                && BinaryHandling.IsBlittable<T7>()
                && BinaryHandling.IsBlittable<T8>()
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
                ref var spanRef = ref reader.GetSpanReference(size);
                value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
                value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
                value3 = Unsafe.ReadUnaligned<T3>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>())
                );
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
                reader.Advance(size);
            }
            else
            {
                value1 = reader.Read<T1>();
                value2 = reader.Read<T2>();
                value3 = reader.Read<T3>();
                value4 = reader.Read<T4>();
                value5 = reader.Read<T5>();
                value6 = reader.Read<T6>();
                value7 = reader.Read<T7>();
                value8 = reader.Read<T8>();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            out T1? value1,
            out T2? value2,
            out T3? value3,
            out T4? value4,
            out T5? value5,
            out T6? value6,
            out T7? value7,
            out T8? value8,
            out T9? value9
        )
        {
            if (
                !reader.IsByteSwapping
                && BinaryHandling.IsBlittable<T1>()
                && BinaryHandling.IsBlittable<T2>()
                && BinaryHandling.IsBlittable<T3>()
                && BinaryHandling.IsBlittable<T4>()
                && BinaryHandling.IsBlittable<T5>()
                && BinaryHandling.IsBlittable<T6>()
                && BinaryHandling.IsBlittable<T7>()
                && BinaryHandling.IsBlittable<T8>()
                && BinaryHandling.IsBlittable<T9>()
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
                ref var spanRef = ref reader.GetSpanReference(size);
                value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
                value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
                value3 = Unsafe.ReadUnaligned<T3>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>())
                );
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
                reader.Advance(size);
            }
            else
            {
                value1 = reader.Read<T1>();
                value2 = reader.Read<T2>();
                value3 = reader.Read<T3>();
                value4 = reader.Read<T4>();
                value5 = reader.Read<T5>();
                value6 = reader.Read<T6>();
                value7 = reader.Read<T7>();
                value8 = reader.Read<T8>();
                value9 = reader.Read<T9>();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            out T1? value1,
            out T2? value2,
            out T3? value3,
            out T4? value4,
            out T5? value5,
            out T6? value6,
            out T7? value7,
            out T8? value8,
            out T9? value9,
            out T10? value10
        )
        {
            if (
                !reader.IsByteSwapping
                && BinaryHandling.IsBlittable<T1>()
                && BinaryHandling.IsBlittable<T2>()
                && BinaryHandling.IsBlittable<T3>()
                && BinaryHandling.IsBlittable<T4>()
                && BinaryHandling.IsBlittable<T5>()
                && BinaryHandling.IsBlittable<T6>()
                && BinaryHandling.IsBlittable<T7>()
                && BinaryHandling.IsBlittable<T8>()
                && BinaryHandling.IsBlittable<T9>()
                && BinaryHandling.IsBlittable<T10>()
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
                ref var spanRef = ref reader.GetSpanReference(size);
                value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
                value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
                value3 = Unsafe.ReadUnaligned<T3>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>())
                );
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
                reader.Advance(size);
            }
            else
            {
                value1 = reader.Read<T1>();
                value2 = reader.Read<T2>();
                value3 = reader.Read<T3>();
                value4 = reader.Read<T4>();
                value5 = reader.Read<T5>();
                value6 = reader.Read<T6>();
                value7 = reader.Read<T7>();
                value8 = reader.Read<T8>();
                value9 = reader.Read<T9>();
                value10 = reader.Read<T10>();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            out T1? value1,
            out T2? value2,
            out T3? value3,
            out T4? value4,
            out T5? value5,
            out T6? value6,
            out T7? value7,
            out T8? value8,
            out T9? value9,
            out T10? value10,
            out T11? value11
        )
        {
            if (
                !reader.IsByteSwapping
                && BinaryHandling.IsBlittable<T1>()
                && BinaryHandling.IsBlittable<T2>()
                && BinaryHandling.IsBlittable<T3>()
                && BinaryHandling.IsBlittable<T4>()
                && BinaryHandling.IsBlittable<T5>()
                && BinaryHandling.IsBlittable<T6>()
                && BinaryHandling.IsBlittable<T7>()
                && BinaryHandling.IsBlittable<T8>()
                && BinaryHandling.IsBlittable<T9>()
                && BinaryHandling.IsBlittable<T10>()
                && BinaryHandling.IsBlittable<T11>()
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
                ref var spanRef = ref reader.GetSpanReference(size);
                value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
                value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
                value3 = Unsafe.ReadUnaligned<T3>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>())
                );
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
                reader.Advance(size);
            }
            else
            {
                value1 = reader.Read<T1>();
                value2 = reader.Read<T2>();
                value3 = reader.Read<T3>();
                value4 = reader.Read<T4>();
                value5 = reader.Read<T5>();
                value6 = reader.Read<T6>();
                value7 = reader.Read<T7>();
                value8 = reader.Read<T8>();
                value9 = reader.Read<T9>();
                value10 = reader.Read<T10>();
                value11 = reader.Read<T11>();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            out T1? value1,
            out T2? value2,
            out T3? value3,
            out T4? value4,
            out T5? value5,
            out T6? value6,
            out T7? value7,
            out T8? value8,
            out T9? value9,
            out T10? value10,
            out T11? value11,
            out T12? value12
        )
        {
            if (
                !reader.IsByteSwapping
                && BinaryHandling.IsBlittable<T1>()
                && BinaryHandling.IsBlittable<T2>()
                && BinaryHandling.IsBlittable<T3>()
                && BinaryHandling.IsBlittable<T4>()
                && BinaryHandling.IsBlittable<T5>()
                && BinaryHandling.IsBlittable<T6>()
                && BinaryHandling.IsBlittable<T7>()
                && BinaryHandling.IsBlittable<T8>()
                && BinaryHandling.IsBlittable<T9>()
                && BinaryHandling.IsBlittable<T10>()
                && BinaryHandling.IsBlittable<T11>()
                && BinaryHandling.IsBlittable<T12>()
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
                ref var spanRef = ref reader.GetSpanReference(size);
                value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
                value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
                value3 = Unsafe.ReadUnaligned<T3>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>())
                );
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
                reader.Advance(size);
            }
            else
            {
                value1 = reader.Read<T1>();
                value2 = reader.Read<T2>();
                value3 = reader.Read<T3>();
                value4 = reader.Read<T4>();
                value5 = reader.Read<T5>();
                value6 = reader.Read<T6>();
                value7 = reader.Read<T7>();
                value8 = reader.Read<T8>();
                value9 = reader.Read<T9>();
                value10 = reader.Read<T10>();
                value11 = reader.Read<T11>();
                value12 = reader.Read<T12>();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            out T1? value1,
            out T2? value2,
            out T3? value3,
            out T4? value4,
            out T5? value5,
            out T6? value6,
            out T7? value7,
            out T8? value8,
            out T9? value9,
            out T10? value10,
            out T11? value11,
            out T12? value12,
            out T13? value13
        )
        {
            if (
                !reader.IsByteSwapping
                && BinaryHandling.IsBlittable<T1>()
                && BinaryHandling.IsBlittable<T2>()
                && BinaryHandling.IsBlittable<T3>()
                && BinaryHandling.IsBlittable<T4>()
                && BinaryHandling.IsBlittable<T5>()
                && BinaryHandling.IsBlittable<T6>()
                && BinaryHandling.IsBlittable<T7>()
                && BinaryHandling.IsBlittable<T8>()
                && BinaryHandling.IsBlittable<T9>()
                && BinaryHandling.IsBlittable<T10>()
                && BinaryHandling.IsBlittable<T11>()
                && BinaryHandling.IsBlittable<T12>()
                && BinaryHandling.IsBlittable<T13>()
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
                ref var spanRef = ref reader.GetSpanReference(size);
                value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
                value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
                value3 = Unsafe.ReadUnaligned<T3>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>())
                );
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
                reader.Advance(size);
            }
            else
            {
                value1 = reader.Read<T1>();
                value2 = reader.Read<T2>();
                value3 = reader.Read<T3>();
                value4 = reader.Read<T4>();
                value5 = reader.Read<T5>();
                value6 = reader.Read<T6>();
                value7 = reader.Read<T7>();
                value8 = reader.Read<T8>();
                value9 = reader.Read<T9>();
                value10 = reader.Read<T10>();
                value11 = reader.Read<T11>();
                value12 = reader.Read<T12>();
                value13 = reader.Read<T13>();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            out T1? value1,
            out T2? value2,
            out T3? value3,
            out T4? value4,
            out T5? value5,
            out T6? value6,
            out T7? value7,
            out T8? value8,
            out T9? value9,
            out T10? value10,
            out T11? value11,
            out T12? value12,
            out T13? value13,
            out T14? value14
        )
        {
            if (
                !reader.IsByteSwapping
                && BinaryHandling.IsBlittable<T1>()
                && BinaryHandling.IsBlittable<T2>()
                && BinaryHandling.IsBlittable<T3>()
                && BinaryHandling.IsBlittable<T4>()
                && BinaryHandling.IsBlittable<T5>()
                && BinaryHandling.IsBlittable<T6>()
                && BinaryHandling.IsBlittable<T7>()
                && BinaryHandling.IsBlittable<T8>()
                && BinaryHandling.IsBlittable<T9>()
                && BinaryHandling.IsBlittable<T10>()
                && BinaryHandling.IsBlittable<T11>()
                && BinaryHandling.IsBlittable<T12>()
                && BinaryHandling.IsBlittable<T13>()
                && BinaryHandling.IsBlittable<T14>()
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
                ref var spanRef = ref reader.GetSpanReference(size);
                value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
                value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
                value3 = Unsafe.ReadUnaligned<T3>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>())
                );
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
                reader.Advance(size);
            }
            else
            {
                value1 = reader.Read<T1>();
                value2 = reader.Read<T2>();
                value3 = reader.Read<T3>();
                value4 = reader.Read<T4>();
                value5 = reader.Read<T5>();
                value6 = reader.Read<T6>();
                value7 = reader.Read<T7>();
                value8 = reader.Read<T8>();
                value9 = reader.Read<T9>();
                value10 = reader.Read<T10>();
                value11 = reader.Read<T11>();
                value12 = reader.Read<T12>();
                value13 = reader.Read<T13>();
                value14 = reader.Read<T14>();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Read<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            out T1? value1,
            out T2? value2,
            out T3? value3,
            out T4? value4,
            out T5? value5,
            out T6? value6,
            out T7? value7,
            out T8? value8,
            out T9? value9,
            out T10? value10,
            out T11? value11,
            out T12? value12,
            out T13? value13,
            out T14? value14,
            out T15? value15
        )
        {
            if (
                !reader.IsByteSwapping
                && BinaryHandling.IsBlittable<T1>()
                && BinaryHandling.IsBlittable<T2>()
                && BinaryHandling.IsBlittable<T3>()
                && BinaryHandling.IsBlittable<T4>()
                && BinaryHandling.IsBlittable<T5>()
                && BinaryHandling.IsBlittable<T6>()
                && BinaryHandling.IsBlittable<T7>()
                && BinaryHandling.IsBlittable<T8>()
                && BinaryHandling.IsBlittable<T9>()
                && BinaryHandling.IsBlittable<T10>()
                && BinaryHandling.IsBlittable<T11>()
                && BinaryHandling.IsBlittable<T12>()
                && BinaryHandling.IsBlittable<T13>()
                && BinaryHandling.IsBlittable<T14>()
                && BinaryHandling.IsBlittable<T15>()
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
                ref var spanRef = ref reader.GetSpanReference(size);
                value1 = Unsafe.ReadUnaligned<T1>(ref spanRef);
                value2 = Unsafe.ReadUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()));
                value3 = Unsafe.ReadUnaligned<T3>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>())
                );
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
                reader.Advance(size);
            }
            else
            {
                value1 = reader.Read<T1>();
                value2 = reader.Read<T2>();
                value3 = reader.Read<T3>();
                value4 = reader.Read<T4>();
                value5 = reader.Read<T5>();
                value6 = reader.Read<T6>();
                value7 = reader.Read<T7>();
                value8 = reader.Read<T8>();
                value9 = reader.Read<T9>();
                value10 = reader.Read<T10>();
                value11 = reader.Read<T11>();
                value12 = reader.Read<T12>();
                value13 = reader.Read<T13>();
                value14 = reader.Read<T14>();
                value15 = reader.Read<T15>();
            }
        }
    }
}
