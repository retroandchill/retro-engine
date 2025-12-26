using MessagePack;
using MessagePack.Formatters;
using RetroEngine.Strings;

namespace DefaultNamespace;

public class NameMessagePackFormatter : IMessagePackFormatter<Name>
{
    public void Serialize(
        ref MessagePackWriter writer,
        Name value,
        MessagePackSerializerOptions options
    )
    {
        writer.Write(value.ToString());
    }

    public Name Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var readString = reader.ReadString();
        return !string.IsNullOrEmpty(readString) ? new Name(readString) : Name.None;
    }
}
