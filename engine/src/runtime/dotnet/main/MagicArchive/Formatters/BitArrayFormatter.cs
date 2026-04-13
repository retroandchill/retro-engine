using System.Collections;
using System.Runtime.CompilerServices;

namespace MagicArchive.Formatters;

public sealed class BitArrayFormatter : ArchiveFormatter<BitArray>
{
    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in BitArray? value)
    {
        if (value is null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        ref var view = ref Unsafe.As<BitArray, BitArrayView>(ref Unsafe.AsRef(in value));

        writer.WriteObjectHeader(2);
        writer.WriteBlittable(view.Length);
        writer.WriteArray(view.Array);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref BitArray? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 2)
            ArchiveSerializationException.ThrowInvalidPropertyCount(2, count);

        var length = reader.ReadBlittable<int>();

        var bitArray = new BitArray(length, false); // create internal int[] and set m_length to length

        ref var view = ref Unsafe.As<BitArray, BitArrayView>(ref bitArray);
        reader.ReadValue(ref view.Array!);

        value = bitArray;
    }
}

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

internal class BitArrayView
{
    public int[] Array;
    public int Length;
    public int Version;
}
