namespace MagicArchive.Formatters;

public sealed class LazyFormatter<T> : ArchiveFormatter<Lazy<T?>>
{
    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in Lazy<T?>? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(1);
        writer.Write(value.Value);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref Lazy<T?>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 1)
            ArchiveSerializationException.ThrowInvalidPropertyCount(1, count);

        var v = reader.Read<T>();
        value = new Lazy<T?>(v);
    }
}
