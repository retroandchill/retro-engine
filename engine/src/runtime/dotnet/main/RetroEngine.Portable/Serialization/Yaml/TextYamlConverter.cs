// // @file TextYamlConverter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Localization.Stringification;
using VYaml.Emitter;
using VYaml.Parser;
using VYaml.Serialization;

namespace RetroEngine.Portable.Serialization.Yaml;

public sealed class TextYamlConverter : IYamlFormatter<Text>
{
    public void Serialize(ref Utf8YamlEmitter emitter, Text value, YamlSerializationContext context)
    {
        var builder = new StringBuilder();
        TextStringifier.ExportToString(builder, value);
        emitter.WriteString(builder.ToString());
    }

    public Text Deserialize(ref YamlParser parser, YamlDeserializationContext context)
    {
        var text = parser.ReadScalarAsString();
        return TextStringifier.ImportFromString(text);
    }
}
