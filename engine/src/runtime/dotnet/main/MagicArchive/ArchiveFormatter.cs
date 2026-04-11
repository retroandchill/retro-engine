// // @file IArchiveFormatter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;

namespace MagicArchive;

public interface IArchiveFormatter
{
    void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in object? value)
        where TBufferWriter : IBufferWriter<byte>;

    void Deserialize(ref ArchiveReader reader, scoped ref object? value);
}

public interface IArchiveFormatter<T> : IArchiveFormatter
{
    void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in T? value)
        where TBufferWriter : IBufferWriter<byte>;

    void Deserialize(ref ArchiveReader reader, scoped ref T? value);
}

public abstract class ArchiveFormatter<T> : IArchiveFormatter<T>
{
    public abstract void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in T? value)
        where TBufferWriter : IBufferWriter<byte>;

    void IArchiveFormatter.Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in object? value)
    {
        var v = value is not null ? (T)value : default;
        Serialize(ref writer, in v);
    }

    public abstract void Deserialize(ref ArchiveReader reader, scoped ref T? value);

    void IArchiveFormatter.Deserialize(ref ArchiveReader reader, scoped ref object? value)
    {
        var v = value is not null ? (T)value : default;
        Deserialize(ref reader, ref v);
    }
}
