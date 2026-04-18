// // @file BitPackFormatter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace MagicArchive.Compression;

public sealed class BitPackFormatter : ArchiveFormatter<bool[]>
{
    public static readonly BitPackFormatter Default = new();

    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in bool[]? value)
    {
        if (value is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        writer.WriteCollectionHeader(value.Length);
        if (value.Length == 0)
            return;

        var data = 0;
        ref var item = ref MemoryMarshal.GetArrayDataReference(value);
        ref var end = ref Unsafe.Add(ref item, value.Length);

        if (value.Length >= 32)
        {
            ref var loopEnd = ref Unsafe.Subtract(ref end, 32);
            if (Vector256.IsHardwareAccelerated)
            {
                while (!Unsafe.IsAddressGreaterThan(ref item, ref loopEnd))
                {
                    var vector = Vector256.LoadUnsafe(ref Unsafe.As<bool, byte>(ref item));
                    data = (int)Vector256.Equals(vector, Vector256<byte>.Zero).ExtractMostSignificantBits();
                    writer.WriteBlittable(~data);
                    item = ref Unsafe.Add(ref item, 32);
                }
            }
            else if (Vector128.IsHardwareAccelerated)
            {
                while (!Unsafe.IsAddressGreaterThan(ref item, ref loopEnd))
                {
                    var bits0 = (ushort)
                        Vector128
                            .Equals(Vector128.LoadUnsafe(ref Unsafe.As<bool, byte>(ref item)), Vector128<byte>.Zero)
                            .ExtractMostSignificantBits();
                    var bits1 = (ushort)
                        Vector128
                            .Equals(Vector128.LoadUnsafe(ref Unsafe.As<bool, byte>(ref item), 16), Vector128<byte>.Zero)
                            .ExtractMostSignificantBits();
                    data = bits0 | (bits1 << 16);
                    writer.WriteBlittable(~data);
                    item = ref Unsafe.Add(ref item, 32);
                }
            }
            else if (Vector64.IsHardwareAccelerated)
            {
                while (!Unsafe.IsAddressGreaterThan(ref item, ref loopEnd))
                {
                    var bits0 = (byte)
                        Vector64
                            .Equals(Vector64.LoadUnsafe(ref Unsafe.As<bool, byte>(ref item)), Vector64<byte>.Zero)
                            .ExtractMostSignificantBits();
                    var bits1 = (byte)
                        Vector64
                            .Equals(Vector64.LoadUnsafe(ref Unsafe.As<bool, byte>(ref item), 8), Vector64<byte>.Zero)
                            .ExtractMostSignificantBits();
                    var bits2 = (byte)
                        Vector64
                            .Equals(Vector64.LoadUnsafe(ref Unsafe.As<bool, byte>(ref item), 16), Vector64<byte>.Zero)
                            .ExtractMostSignificantBits();
                    var bits3 = (byte)
                        Vector64
                            .Equals(Vector64.LoadUnsafe(ref Unsafe.As<bool, byte>(ref item), 24), Vector64<byte>.Zero)
                            .ExtractMostSignificantBits();
                    data = bits0 | (bits1 << 8) | (bits2 << 16) | (bits3 << 24);
                    writer.WriteBlittable(~data);
                    item = ref Unsafe.Add(ref item, 32);
                }
            }

            data = 0;
        }

        var bit = 0;
        while (Unsafe.IsAddressLessThan(ref item, ref end))
        {
            Set(ref data, bit, item);

            item = ref Unsafe.Add(ref item, 1);
            bit++;

            if (bit != 32)
                continue;

            writer.WriteBlittable(~data);
            data = 0;
            bit = 0;
        }

        if (bit != 0)
            writer.WriteBlittable(data);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref bool[]? value)
    {
        if (!reader.UnsafeTryReadCollectionHeader(out var length))
        {
            value = null;
            return;
        }

        if (length == 0)
        {
            value = [];
            return;
        }

        var readCount = (length - 1) / 32 + 1;
        var requiresSize = readCount * 4;
        if (reader.RemainingBytes < requiresSize)
        {
            ArchiveSerializationException.ThrowInsufficientBufferUnless(requiresSize);
        }

        if (value is null || value.Length != length)
        {
            value = new bool[length];
        }

        var bit = 0;
        var data = 0;
        for (var i = 0; i < value.Length; i++)
        {
            if (bit == 0)
            {
                reader.ReadBlittable(out data);
            }

            value[i] = Get(data, bit);
            bit++;

            if (bit != 32)
                continue;

            data = 0;
            bit = 0;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool Get(int data, int index)
    {
        return (data & (1 << index)) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Set(ref int data, int index, bool value)
    {
        var bitMask = 1 << index;
        if (value)
            data |= bitMask;
        else
            data &= ~bitMask;
    }
}
