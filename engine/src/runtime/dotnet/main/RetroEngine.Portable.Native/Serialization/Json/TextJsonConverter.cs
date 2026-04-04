// // @file TextJsonConverter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Localization.Stringification;

namespace RetroEngine.Portable.Serialization.Json;

public sealed class TextJsonConverter : JsonConverter<Text>
{
    public override Text Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var textString = reader.GetString();
        return TextStringifier.ImportFromString(textString);
    }

    public override void Write(Utf8JsonWriter writer, Text value, JsonSerializerOptions options)
    {
        var builder = new StringBuilder();
        TextStringifier.ExportToString(builder, value);
        writer.WriteStringValue(builder.ToString());
    }
}
