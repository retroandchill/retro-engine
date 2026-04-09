// // @file BinaryHandlingConverter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace RetroEngine.Portable.Serialization.Binary.Utilities;

public static class BinaryHandling
{
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
            || typeof(T) == typeof(Rune);
    }

    internal static T ReverseEndianness<T>(T value)
        where T : unmanaged
    {
        ReverseEndianness(ref value);
        return value;
    }

    internal static void ReverseEndianness<T>(ref T value)
        where T : unmanaged
    {
        var memorySpan = MemoryMarshal.AsBytes(new Span<T>(ref value));
        memorySpan.Reverse();
    }
}
