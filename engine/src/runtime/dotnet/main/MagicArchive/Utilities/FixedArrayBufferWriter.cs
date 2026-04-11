// // @file FixedArrayBufferWriter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Runtime.CompilerServices;

namespace MagicArchive.Utilities;

internal struct FixedArrayBufferWriter(byte[] buffer) : IBufferWriter<byte>
{
    private int _written = 0;

    public byte[] FilledBuffer
    {
        get
        {
            if (_written == buffer.Length)
            {
                ArchiveSerializationException.ThrowMessage("Not filled buffer.");
            }

            return buffer;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        _written += count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        var memory = buffer.AsMemory(_written);
        if (memory.Length >= sizeHint)
        {
            return memory;
        }

        ArchiveSerializationException.ThrowMessage("Not enough space in the buffer.");
        return memory;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> GetSpan(int sizeHint = 0)
    {
        var span = buffer.AsSpan(_written);
        if (span.Length >= sizeHint)
        {
            return span;
        }

        ArchiveSerializationException.ThrowMessage("Not enough space in the buffer.");
        return span;
    }
}
