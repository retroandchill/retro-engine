// @file NameMessagePackFormatter.cs.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using MessagePack;
using MessagePack.Formatters;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Portable.Serialization.MessagePack;

public class TextMessagePackFormatter : IMessagePackFormatter<Text>
{
    public void Serialize(ref MessagePackWriter writer, Text value, MessagePackSerializerOptions options)
    {
        var builder = new StringBuilder();
        TextStringHelper.WriteToBuffer(builder, value);
        writer.Write(builder.ToString());
    }

    public Text Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var readString = reader.ReadString();
        return TextStringHelper.ReadFromBuffer(readString);
    }
}
