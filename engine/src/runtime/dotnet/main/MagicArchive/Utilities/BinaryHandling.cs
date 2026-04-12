// // @file BinaryHandlingConverter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

namespace MagicArchive.Utilities;

public static class BinaryHandling
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBlittable<T>()
    {
        return typeof(T).IsEnum
            || typeof(T) == typeof(byte)
            || typeof(T) == typeof(short)
            || typeof(T) == typeof(int)
            || typeof(T) == typeof(long)
            || typeof(T) == typeof(sbyte)
            || typeof(T) == typeof(ushort)
            || typeof(T) == typeof(uint)
            || typeof(T) == typeof(ulong)
            || typeof(T) == typeof(float)
            || typeof(T) == typeof(double)
            || typeof(T) == typeof(char)
            || typeof(T) == typeof(Rune)
            || typeof(T) == typeof(DateTime);
    }

    internal static T ReverseEndianness<T>(T value)
    {
        ReverseEndianness(ref value);
        return value;
    }

    internal static void ReverseEndianness<T>(ref T value)
    {
        switch (value)
        {
            case byte or sbyte:
                // Do nothing
                break;
            case short or ushort or char:
            {
                ref var castValue = ref Unsafe.As<T, ushort>(ref value);
                castValue = BinaryPrimitives.ReverseEndianness(castValue);
                break;
            }
            case int or uint or float:
            {
                ref var castValue = ref Unsafe.As<T, uint>(ref value);
                castValue = BinaryPrimitives.ReverseEndianness(castValue);
                break;
            }
            case long or ulong or double:
            {
                ref var castValue = ref Unsafe.As<T, ulong>(ref value);
                castValue = BinaryPrimitives.ReverseEndianness(castValue);
                break;
            }
        }
    }
}
