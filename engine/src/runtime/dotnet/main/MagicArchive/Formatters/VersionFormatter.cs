// // @file VersionFormatter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MagicArchive.Utilities;

namespace MagicArchive.Formatters;

public sealed class VersionFormatter : ArchiveFormatter<Version>
{
    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in Version? value)
    {
        if (value is null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(4);
        writer.Write(value.Major, value.Minor, value.Build, value.Revision);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref Version? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 4)
            ArchiveSerializationException.ThrowInvalidPropertyCount(4, count);

        reader.Read(out int major, out int minor, out int build, out int revision);

        if (revision == -1)
        {
            value = build == -1 ? new Version(major, minor) : new Version(major, minor, build);
        }
        else
        {
            value = new Version(major, minor, build, revision);
        }
    }
}
