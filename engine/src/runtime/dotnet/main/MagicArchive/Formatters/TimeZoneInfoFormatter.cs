namespace MagicArchive.Formatters;

public sealed class TimeZoneInfoFormatter : ArchiveFormatter<TimeZoneInfo>
{
    public override void Serialize<TBufferWriter>(
        ref ArchiveWriter<TBufferWriter> writer,
        scoped in TimeZoneInfo? value
    )
    {
        writer.Write(value?.ToSerializedString());
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref TimeZoneInfo? value)
    {
        var source = reader.ReadString();
        if (source is null)
        {
            value = null;
            return;
        }

        value = TimeZoneInfo.FromSerializedString(source);
    }
}
