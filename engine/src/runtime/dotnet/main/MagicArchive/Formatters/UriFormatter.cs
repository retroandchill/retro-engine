namespace MagicArchive.Formatters;

public sealed class UriFormatter : ArchiveFormatter<Uri>
{
    // treat as a string(OriginalString).

    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in Uri? value)
    {
        writer.Write(value?.OriginalString);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref Uri? value)
    {
        var str = reader.ReadString();
        value = str is not null ? new Uri(str, UriKind.RelativeOrAbsolute) : null;
    }
}
