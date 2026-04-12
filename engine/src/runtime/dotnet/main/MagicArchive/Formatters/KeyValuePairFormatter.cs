// // @file KeyValuePairFormatter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Runtime.CompilerServices;

namespace MagicArchive.Formatters;

public static class KeyValuePairFormatter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize<TKey, TValue, TBufferWriter>(
        IArchiveFormatter<TKey> keyFormatter,
        IArchiveFormatter<TValue> valueFormatter,
        ref ArchiveWriter<TBufferWriter> writer,
        KeyValuePair<TKey?, TValue?> value
    )
        where TBufferWriter : IBufferWriter<byte>
    {
        keyFormatter.Serialize(ref writer, value.Key);
        valueFormatter.Serialize(ref writer, value.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Deserialize<TKey, TValue>(
        IArchiveFormatter<TKey> keyFormatter,
        IArchiveFormatter<TValue> valueFormatter,
        ref ArchiveReader reader,
        out TKey? key,
        out TValue? value
    )
    {
        key = default;
        value = default;
        keyFormatter.Deserialize(ref reader, ref key);
        valueFormatter.Deserialize(ref reader, ref value);
    }
}

public sealed class KeyValuePairFormatter<TKey, TValue> : ArchiveFormatter<KeyValuePair<TKey?, TValue?>>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in KeyValuePair<TKey?, TValue?> value
    )
    {
        writer.Write(value.Key);
        writer.Write(value.Value);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref KeyValuePair<TKey?, TValue?> value)
    {
        value = new KeyValuePair<TKey?, TValue?>(reader.Read<TKey>(), reader.Read<TValue>());
    }
}
