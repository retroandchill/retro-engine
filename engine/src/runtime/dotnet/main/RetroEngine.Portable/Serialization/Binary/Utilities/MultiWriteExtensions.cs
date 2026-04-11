using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RetroEngine.Portable.Serialization.Binary.Utilities;

public static class MultiWriteExtensions
{
    extension<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer)
        where TBufferWriter : IBufferWriter<byte>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T1, T2>(in T1? value1, in T2? value2)
        {
            if (!writer.IsByteSwapping && BinaryHandling.IsBlittable<T1>() && BinaryHandling.IsBlittable<T2>())
            {
                var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>();
                ref var spanRef = ref writer.GetSpanReference(size);
                Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
                Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
                writer.Advance(size);
            }
            else
            {
                writer.Write<T1>(in value1!);
                writer.Write<T2>(in value2!);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T1, T2, T3>(in T1? value1, in T2? value2, in T3? value3)
        {
            if (
                !writer.IsByteSwapping
                && BinaryHandling.IsBlittable<T1>()
                && BinaryHandling.IsBlittable<T2>()
                && BinaryHandling.IsBlittable<T3>()
            )
            {
                var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>();
                ref var spanRef = ref writer.GetSpanReference(size);
                Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
                Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
                Unsafe.WriteUnaligned<T3>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                    value3!
                );
                writer.Advance(size);
            }
            else
            {
                writer.Write<T1>(in value1!);
                writer.Write<T2>(in value2!);
                writer.Write<T3>(in value3!);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T1, T2, T3, T4>(in T1? value1, in T2? value2, in T3? value3, in T4? value4)
        {
            if (
                !writer.IsByteSwapping
                && BinaryHandling.IsBlittable<T1>()
                && BinaryHandling.IsBlittable<T2>()
                && BinaryHandling.IsBlittable<T3>()
                && BinaryHandling.IsBlittable<T4>()
            )
            {
                var size = Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>() + Unsafe.SizeOf<T4>();
                ref var spanRef = ref writer.GetSpanReference(size);
                Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
                Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
                Unsafe.WriteUnaligned<T3>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                    value3!
                );
                Unsafe.WriteUnaligned<T4>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>() + Unsafe.SizeOf<T3>()),
                    value4!
                );
                writer.Advance(size);
            }
            else
            {
                writer.Write<T1>(in value1!);
                writer.Write<T2>(in value2!);
                writer.Write<T3>(in value3!);
                writer.Write<T4>(in value4!);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T1, T2, T3, T4, T5>(in T1? value1, in T2? value2, in T3? value3, in T4? value4, in T5? value5)
        {
            if (
                !writer.IsByteSwapping
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
                ref var spanRef = ref writer.GetSpanReference(size);
                Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
                Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
                Unsafe.WriteUnaligned<T3>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                    value3!
                );
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
                writer.Advance(size);
            }
            else
            {
                writer.Write<T1>(in value1!);
                writer.Write<T2>(in value2!);
                writer.Write<T3>(in value3!);
                writer.Write<T4>(in value4!);
                writer.Write<T5>(in value5!);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T1, T2, T3, T4, T5, T6>(
            in T1? value1,
            in T2? value2,
            in T3? value3,
            in T4? value4,
            in T5? value5,
            in T6? value6
        )
        {
            if (
                !writer.IsByteSwapping
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
                ref var spanRef = ref writer.GetSpanReference(size);
                Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
                Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
                Unsafe.WriteUnaligned<T3>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                    value3!
                );
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
                writer.Advance(size);
            }
            else
            {
                writer.Write<T1>(in value1!);
                writer.Write<T2>(in value2!);
                writer.Write<T3>(in value3!);
                writer.Write<T4>(in value4!);
                writer.Write<T5>(in value5!);
                writer.Write<T6>(in value6!);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T1, T2, T3, T4, T5, T6, T7>(
            in T1? value1,
            in T2? value2,
            in T3? value3,
            in T4? value4,
            in T5? value5,
            in T6? value6,
            in T7? value7
        )
        {
            if (
                !writer.IsByteSwapping
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
                ref var spanRef = ref writer.GetSpanReference(size);
                Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
                Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
                Unsafe.WriteUnaligned<T3>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                    value3!
                );
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
                writer.Advance(size);
            }
            else
            {
                writer.Write<T1>(in value1!);
                writer.Write<T2>(in value2!);
                writer.Write<T3>(in value3!);
                writer.Write<T4>(in value4!);
                writer.Write<T5>(in value5!);
                writer.Write<T6>(in value6!);
                writer.Write<T7>(in value7!);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T1, T2, T3, T4, T5, T6, T7, T8>(
            in T1? value1,
            in T2? value2,
            in T3? value3,
            in T4? value4,
            in T5? value5,
            in T6? value6,
            in T7? value7,
            in T8? value8
        )
        {
            if (
                !writer.IsByteSwapping
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
                ref var spanRef = ref writer.GetSpanReference(size);
                Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
                Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
                Unsafe.WriteUnaligned<T3>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                    value3!
                );
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
                writer.Advance(size);
            }
            else
            {
                writer.Write<T1>(in value1!);
                writer.Write<T2>(in value2!);
                writer.Write<T3>(in value3!);
                writer.Write<T4>(in value4!);
                writer.Write<T5>(in value5!);
                writer.Write<T6>(in value6!);
                writer.Write<T7>(in value7!);
                writer.Write<T8>(in value8!);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            in T1? value1,
            in T2? value2,
            in T3? value3,
            in T4? value4,
            in T5? value5,
            in T6? value6,
            in T7? value7,
            in T8? value8,
            in T9? value9
        )
        {
            if (
                !writer.IsByteSwapping
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
                ref var spanRef = ref writer.GetSpanReference(size);
                Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
                Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
                Unsafe.WriteUnaligned<T3>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                    value3!
                );
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
                writer.Advance(size);
            }
            else
            {
                writer.Write<T1>(in value1!);
                writer.Write<T2>(in value2!);
                writer.Write<T3>(in value3!);
                writer.Write<T4>(in value4!);
                writer.Write<T5>(in value5!);
                writer.Write<T6>(in value6!);
                writer.Write<T7>(in value7!);
                writer.Write<T8>(in value8!);
                writer.Write<T9>(in value9!);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            in T1? value1,
            in T2? value2,
            in T3? value3,
            in T4? value4,
            in T5? value5,
            in T6? value6,
            in T7? value7,
            in T8? value8,
            in T9? value9,
            in T10? value10
        )
        {
            if (
                !writer.IsByteSwapping
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
                ref var spanRef = ref writer.GetSpanReference(size);
                Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
                Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
                Unsafe.WriteUnaligned<T3>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                    value3!
                );
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
                writer.Advance(size);
            }
            else
            {
                writer.Write<T1>(in value1!);
                writer.Write<T2>(in value2!);
                writer.Write<T3>(in value3!);
                writer.Write<T4>(in value4!);
                writer.Write<T5>(in value5!);
                writer.Write<T6>(in value6!);
                writer.Write<T7>(in value7!);
                writer.Write<T8>(in value8!);
                writer.Write<T9>(in value9!);
                writer.Write<T10>(in value10!);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            in T1? value1,
            in T2? value2,
            in T3? value3,
            in T4? value4,
            in T5? value5,
            in T6? value6,
            in T7? value7,
            in T8? value8,
            in T9? value9,
            in T10? value10,
            in T11? value11
        )
        {
            if (
                !writer.IsByteSwapping
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
                ref var spanRef = ref writer.GetSpanReference(size);
                Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
                Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
                Unsafe.WriteUnaligned<T3>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                    value3!
                );
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
                writer.Advance(size);
            }
            else
            {
                writer.Write<T1>(in value1!);
                writer.Write<T2>(in value2!);
                writer.Write<T3>(in value3!);
                writer.Write<T4>(in value4!);
                writer.Write<T5>(in value5!);
                writer.Write<T6>(in value6!);
                writer.Write<T7>(in value7!);
                writer.Write<T8>(in value8!);
                writer.Write<T9>(in value9!);
                writer.Write<T10>(in value10!);
                writer.Write<T11>(in value11!);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            in T1? value1,
            in T2? value2,
            in T3? value3,
            in T4? value4,
            in T5? value5,
            in T6? value6,
            in T7? value7,
            in T8? value8,
            in T9? value9,
            in T10? value10,
            in T11? value11,
            in T12? value12
        )
        {
            if (
                !writer.IsByteSwapping
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
                ref var spanRef = ref writer.GetSpanReference(size);
                Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
                Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
                Unsafe.WriteUnaligned<T3>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                    value3!
                );
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
                writer.Advance(size);
            }
            else
            {
                writer.Write<T1>(in value1!);
                writer.Write<T2>(in value2!);
                writer.Write<T3>(in value3!);
                writer.Write<T4>(in value4!);
                writer.Write<T5>(in value5!);
                writer.Write<T6>(in value6!);
                writer.Write<T7>(in value7!);
                writer.Write<T8>(in value8!);
                writer.Write<T9>(in value9!);
                writer.Write<T10>(in value10!);
                writer.Write<T11>(in value11!);
                writer.Write<T12>(in value12!);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            in T1? value1,
            in T2? value2,
            in T3? value3,
            in T4? value4,
            in T5? value5,
            in T6? value6,
            in T7? value7,
            in T8? value8,
            in T9? value9,
            in T10? value10,
            in T11? value11,
            in T12? value12,
            in T13? value13
        )
        {
            if (
                !writer.IsByteSwapping
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
                ref var spanRef = ref writer.GetSpanReference(size);
                Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
                Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
                Unsafe.WriteUnaligned<T3>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                    value3!
                );
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
                writer.Advance(size);
            }
            else
            {
                writer.Write<T1>(in value1!);
                writer.Write<T2>(in value2!);
                writer.Write<T3>(in value3!);
                writer.Write<T4>(in value4!);
                writer.Write<T5>(in value5!);
                writer.Write<T6>(in value6!);
                writer.Write<T7>(in value7!);
                writer.Write<T8>(in value8!);
                writer.Write<T9>(in value9!);
                writer.Write<T10>(in value10!);
                writer.Write<T11>(in value11!);
                writer.Write<T12>(in value12!);
                writer.Write<T13>(in value13!);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            in T1? value1,
            in T2? value2,
            in T3? value3,
            in T4? value4,
            in T5? value5,
            in T6? value6,
            in T7? value7,
            in T8? value8,
            in T9? value9,
            in T10? value10,
            in T11? value11,
            in T12? value12,
            in T13? value13,
            in T14? value14
        )
        {
            if (
                !writer.IsByteSwapping
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
                ref var spanRef = ref writer.GetSpanReference(size);
                Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
                Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
                Unsafe.WriteUnaligned<T3>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                    value3!
                );
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
                writer.Advance(size);
            }
            else
            {
                writer.Write<T1>(in value1!);
                writer.Write<T2>(in value2!);
                writer.Write<T3>(in value3!);
                writer.Write<T4>(in value4!);
                writer.Write<T5>(in value5!);
                writer.Write<T6>(in value6!);
                writer.Write<T7>(in value7!);
                writer.Write<T8>(in value8!);
                writer.Write<T9>(in value9!);
                writer.Write<T10>(in value10!);
                writer.Write<T11>(in value11!);
                writer.Write<T12>(in value12!);
                writer.Write<T13>(in value13!);
                writer.Write<T14>(in value14!);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            in T1? value1,
            in T2? value2,
            in T3? value3,
            in T4? value4,
            in T5? value5,
            in T6? value6,
            in T7? value7,
            in T8? value8,
            in T9? value9,
            in T10? value10,
            in T11? value11,
            in T12? value12,
            in T13? value13,
            in T14? value14,
            in T15? value15
        )
        {
            if (
                !writer.IsByteSwapping
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
                ref var spanRef = ref writer.GetSpanReference(size);
                Unsafe.WriteUnaligned<T1>(ref spanRef, value1!);
                Unsafe.WriteUnaligned<T2>(ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>()), value2!);
                Unsafe.WriteUnaligned<T3>(
                    ref Unsafe.Add(ref spanRef, Unsafe.SizeOf<T1>() + Unsafe.SizeOf<T2>()),
                    value3!
                );
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
                writer.Advance(size);
            }
            else
            {
                writer.Write<T1>(in value1!);
                writer.Write<T2>(in value2!);
                writer.Write<T3>(in value3!);
                writer.Write<T4>(in value4!);
                writer.Write<T5>(in value5!);
                writer.Write<T6>(in value6!);
                writer.Write<T7>(in value7!);
                writer.Write<T8>(in value8!);
                writer.Write<T9>(in value9!);
                writer.Write<T10>(in value10!);
                writer.Write<T11>(in value11!);
                writer.Write<T12>(in value12!);
                writer.Write<T13>(in value13!);
                writer.Write<T14>(in value14!);
                writer.Write<T15>(in value15!);
            }
        }
    }
}
