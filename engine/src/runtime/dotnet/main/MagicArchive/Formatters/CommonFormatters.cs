// // @file BoolFormatter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MagicArchive.Utilities;

namespace MagicArchive.Formatters;

public sealed class BooleanFormatter : ArchiveFormatter<bool>
{
    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in bool value)
    {
        writer.WriteBool(value);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref bool value)
    {
        value = reader.ReadBool();
    }
}

public sealed class GuidFormatter : ArchiveFormatter<Guid>
{
    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in Guid value)
    {
        const int size = 16;
        ref var spanRef = ref writer.GetSpanReference(size);
        value.TryWriteBytes(
            MemoryMarshal.CreateSpan(ref spanRef, size),
            writer.Options.ByteOrder == ByteOrder.BigEndian,
            out _
        );
        writer.Advance(size);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref Guid value)
    {
        const int guidSize = 16;
        ref var spanRef = ref reader.GetSpanReference(guidSize);
        var span = MemoryMarshal.CreateReadOnlySpan(ref spanRef, guidSize);
        value = new Guid(span, reader.Options.ByteOrder == ByteOrder.BigEndian);
        reader.Advance(guidSize);
    }
}

public sealed class DecimalFormatter : ArchiveFormatter<decimal>
{
    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in decimal value)
    {
        Span<int> bits = stackalloc int[4];
        decimal.GetBits(value, bits);
        writer.WriteBlittable(bits[0], bits[1], bits[2], bits[3]);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref decimal value)
    {
        Span<int> bits = stackalloc int[4];
        reader.ReadBlittable(out bits[0], out bits[1], out bits[2], out bits[3]);
        value = new decimal(bits);
    }
}

public sealed class DateTimeOffsetFormatter : ArchiveFormatter<DateTimeOffset>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in DateTimeOffset value
    )
    {
        writer.WriteBlittable(value.UtcTicks, value.TotalOffsetMinutes);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref DateTimeOffset value)
    {
        reader.ReadBlittable(out long utcTicks, out int offsetMinutes);
        value = new DateTimeOffset(utcTicks, TimeSpan.FromMinutes(offsetMinutes));
    }
}
