// // @file EnumFormatter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace MagicArchive.Formatters;

public sealed class EnumFormatter<T> : ArchiveFormatter<T>
    where T : unmanaged, Enum
{
    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in T value)
    {
        writer.WriteBlittable(value);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref T value)
    {
        value = reader.ReadBlittable<T>();
    }
}
