using System.Text.Json;
using System.Text.Json.Serialization;

namespace RetroEngine.Strings.Serialization.Json;

public sealed class NameJsonConverter : JsonConverter<Name>
{
    /// <inheritdoc />
    public override Name Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        try
        {
            var foundString = reader.GetString();
            return foundString is not null
                ? new Name(foundString)
                : throw new JsonException("Name cannot be null.");
        }
        catch (InvalidOperationException ex)
        {
            throw new JsonException(ex.Message, ex);
        }
    }

    /// <inheritdoc />
    public override Name ReadAsPropertyName(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        return Read(ref reader, typeToConvert, options);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Name value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }

    public override void WriteAsPropertyName(
        Utf8JsonWriter writer,
        Name value,
        JsonSerializerOptions options
    )
    {
        writer.WritePropertyName(value.ToString());
    }
}
