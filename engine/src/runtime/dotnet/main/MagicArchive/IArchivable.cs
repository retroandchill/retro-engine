// // @file IArchivable.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;

namespace MagicArchive;

public interface IFixedSizeArchivable
{
    static abstract int Size { get; }
}

public interface IArchivable
{
    static abstract void RegisterFormatters();
}

public interface IArchivable<T> : IArchivable
    where T : IArchivable<T>
{
    static abstract void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in T? value)
        where TBufferWriter : IBufferWriter<byte>;

    static abstract void Deserialize(ref ArchiveReader reader, scoped ref T? value);
}
