using System.Globalization;

namespace MagicArchive.Formatters;

public sealed class CultureInfoFormatter : ArchiveFormatter<CultureInfo>
{
    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in CultureInfo? value)
    {
        writer.WriteString(value?.Name);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref CultureInfo? value)
    {
        var str = reader.ReadString();
        value = str is not null ? CultureInfo.GetCultureInfo(str) : null;
    }
}
