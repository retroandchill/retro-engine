// @file NameMessagePackFormatter.cs.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
using MessagePack;
using MessagePack.Formatters;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Portable.Serialization.MessagePack;

public class NameMessagePackFormatter : IMessagePackFormatter<Name>
{
    public void Serialize(ref MessagePackWriter writer, Name value, MessagePackSerializerOptions options)
    {
        writer.Write(value.ToString());
    }

    public Name Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var readString = reader.ReadString();
        return !string.IsNullOrEmpty(readString) ? new Name(readString) : Name.None;
    }
}
