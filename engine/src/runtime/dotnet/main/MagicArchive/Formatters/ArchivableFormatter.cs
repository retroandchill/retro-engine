// // @file ArchivableFormatter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace MagicArchive.Formatters;

public sealed class ArchivableFormatter<T> : ArchiveFormatter<T>
    where T : IArchivable<T>
{
    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in T? value)
    {
        T.Serialize(ref writer, in value);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref T? value)
    {
        T.Deserialize(ref reader, ref value);
    }
}
