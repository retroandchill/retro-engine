// // @file BoolFormatter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

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
        writer.Write(value);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref Guid value)
    {
        value = reader.ReadGuid();
    }
}

public sealed class DateTimeOffsetFormatter : ArchiveFormatter<DateTimeOffset>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in DateTimeOffset value
    )
    {
        writer.Write(value);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref DateTimeOffset value)
    {
        value = reader.ReadDateTimeOffset();
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
