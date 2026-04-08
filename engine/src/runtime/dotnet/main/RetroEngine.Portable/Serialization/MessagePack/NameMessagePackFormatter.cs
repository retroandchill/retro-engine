// @file NameMessagePackFormatter.cs.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Runtime.InteropServices;
using MessagePack;
using MessagePack.Formatters;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Portable.Serialization.MessagePack;

public class NameMessagePackFormatter : IMessagePackFormatter<Name>
{
    public void Serialize(ref MessagePackWriter writer, Name value, MessagePackSerializerOptions options)
    {
        Span<byte> buffer = stackalloc byte[Name.MaxRenderedLength];
        var writtenLength = value.ToUtf8(buffer);
        writer.WriteString(MemoryMarshal.CreateReadOnlySpan(ref buffer.GetPinnableReference(), writtenLength));
    }

    public Name Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        if (reader.TryReadStringSpan(out var stringSpan))
        {
            return new Name(stringSpan);
        }

        if (reader.ReadStringSequence() is not { } readString)
            return Name.None;

        var stringArray = ArrayPool<byte>.Shared.Rent((int)readString.Length);
        try
        {
            readString.CopyTo(stringArray);
            return new Name(stringArray);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(stringArray);
        }
    }
}
