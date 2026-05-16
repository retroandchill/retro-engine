// @file TextKeyJsonSerializer.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Portable.Serialization.Json;

public sealed class TextKeyJsonConverter : JsonConverter<TextKey>
{
    public override TextKey Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType != JsonTokenType.String
            ? throw new JsonException($"Expected string token, got {reader.TokenType}")
            : new TextKey(reader.GetString()!);
    }

    /// <inheritdoc />
    public override TextKey ReadAsPropertyName(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        return reader.TokenType != JsonTokenType.PropertyName
            ? throw new JsonException($"Expected string token, got {reader.TokenType}")
            : new TextKey(reader.GetString()!);
    }

    public override void Write(Utf8JsonWriter writer, TextKey value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, TextKey value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(value.ToString());
    }
}
