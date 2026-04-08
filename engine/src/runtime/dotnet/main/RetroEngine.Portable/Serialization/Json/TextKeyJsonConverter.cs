// // @file TextKeyJsonConverter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;
using RetroEngine.Portable.Localization;

namespace RetroEngine.Portable.Serialization.Json;

public sealed class TextKeyJsonConverter : JsonConverter<TextKey>
{
    public override TextKey Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new TextKey(reader.GetString());
    }

    public override TextKey ReadAsPropertyName(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        return Read(ref reader, typeToConvert, options);
    }

    public override void Write(Utf8JsonWriter writer, TextKey value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, TextKey value, JsonSerializerOptions options)
    {
        Write(writer, value, options);
    }
}
