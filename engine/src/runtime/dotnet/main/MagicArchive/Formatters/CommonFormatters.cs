// // @file BoolFormatter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;

namespace MagicArchive.Formatters;

public sealed class BooleanFormatter : ArchiveFormatter<bool>
{
    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in bool value)
    {
        writer.Write(value);
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

public sealed class DateTimeOffsetFormatter : ArchiveFormatter<DateTimeOffset>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in DateTimeOffset value
    )
    {
        writer.Write(value.ToUnixTimeMilliseconds());
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref DateTimeOffset value)
    {
        var unixTimestamp = reader.ReadInt64();
        value = DateTimeOffset.FromUnixTimeMilliseconds(unixTimestamp);
    }
}

public sealed class StringFormatter : ArchiveFormatter<string>
{
    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in string? value)
    {
        writer.Write(value);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref string? value)
    {
        value = reader.ReadString();
    }
}
