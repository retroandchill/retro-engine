using System.Numerics;
using System.Runtime.InteropServices;

namespace MagicArchive.Formatters;

public sealed class BigIntegerFormatter : ArchiveFormatter<BigInteger>
{
    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in BigInteger value)
    {
        Span<byte> temp = stackalloc byte[255];
        if (value.TryWriteBytes(temp, out var written))
        {
            writer.WriteSpan(temp[written..]);
            return;
        }

        var byteArray = value.ToByteArray();
        writer.WriteSpan(byteArray);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref BigInteger value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = default;
            return;
        }

        ref var src = ref reader.GetSpanReference(length);
        value = new BigInteger(MemoryMarshal.CreateReadOnlySpan(ref src, length));

        reader.Advance(length);
    }
}
