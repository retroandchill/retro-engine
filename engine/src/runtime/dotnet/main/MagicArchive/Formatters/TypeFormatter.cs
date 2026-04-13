using System.Text.RegularExpressions;

namespace MagicArchive.Formatters;

public sealed partial class TypeFormatter : ArchiveFormatter<Type>
{
    [GeneratedRegex(@", Version=\d+.\d+.\d+.\d+, Culture=[\w-]+, PublicKeyToken=(?:null|[a-f0-9]{16})")]
    private static partial Regex ShortTypeNameRegex();

    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in Type? value)
    {
        var full = value?.AssemblyQualifiedName;
        if (full is null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var shortName = ShortTypeNameRegex().Replace(full, "");
        writer.WriteString(shortName);
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref Type? value)
    {
        var typeName = reader.ReadString();
        if (typeName is null)
        {
            value = null;
            return;
        }

        value = Type.GetType(typeName, throwOnError: true);
    }
}
