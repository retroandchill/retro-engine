// // @file StructuredJsonWriter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Localization.Stringification;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Portable.Serialization.Json;

public readonly struct StructuredJsonWriter(Utf8JsonWriter writer) : IStructuredWriter
{
    public void BeginObject()
    {
        writer.WriteStartObject();
    }

    public void EndObject()
    {
        writer.WriteEndObject();
    }

    public void BeginProperty(ReadOnlySpan<char> name)
    {
        writer.WritePropertyName(name);
    }

    public void EndProperty()
    {
        // No-op
    }

    public void BeginArray(int size)
    {
        writer.WriteStartArray();
    }

    public void EndArray()
    {
        writer.WriteEndArray();
    }

    public void BeginArrayItem()
    {
        // No-op
    }

    public void EndArrayItem()
    {
        // No-op
    }

    public void BeginDictionary(int size)
    {
        writer.WriteStartObject();
    }

    public void EndDictionary()
    {
        writer.WriteEndObject();
    }

    public void BeginDictionaryItem(ReadOnlySpan<char> key)
    {
        writer.WritePropertyName(key);
    }

    public void EndDictionaryItem()
    {
        // No-op
    }

    public void WriteNull()
    {
        writer.WriteNullValue();
    }

    public void Write(bool value)
    {
        writer.WriteBooleanValue(value);
    }

    public void Write(char value)
    {
        writer.WriteStringValue(new ReadOnlySpan<char>(ref value));
    }

    public void Write(Rune value)
    {
        writer.WriteStringValue(value.ToString());
    }

    public void Write(byte value)
    {
        writer.WriteNumberValue(value);
    }

    public void Write(sbyte value)
    {
        writer.WriteNumberValue(value);
    }

    public void Write(short value)
    {
        writer.WriteNumberValue(value);
    }

    public void Write(ushort value)
    {
        writer.WriteNumberValue(value);
    }

    public void Write(int value)
    {
        writer.WriteNumberValue(value);
    }

    public void Write(uint value)
    {
        writer.WriteNumberValue(value);
    }

    public void Write(long value)
    {
        writer.WriteNumberValue(value);
    }

    public void Write(ulong value)
    {
        writer.WriteNumberValue(value);
    }

    public void Write(float value)
    {
        writer.WriteNumberValue(value);
    }

    public void Write(double value)
    {
        writer.WriteNumberValue(value);
    }

    public void Write(Guid value)
    {
        writer.WriteStringValue(value);
    }

    public void Write(DateTime value)
    {
        writer.WriteStringValue(value);
    }

    public void Write(DateTimeOffset value)
    {
        writer.WriteStringValue(value);
    }

    public void Write(Name value)
    {
        Span<byte> buffer = stackalloc byte[Name.MaxRenderedLength];
        var length = value.ToUtf8(buffer);
        writer.WriteStringValue(buffer[..length]);
    }

    public void Write(ReadOnlySpan<char> value)
    {
        writer.WriteStringValue(value);
    }

    public void Write(Text value)
    {
        var builder = new StringBuilder();
        TextStringifier.ExportToString(builder, value);
        writer.WriteStringValue(builder.ToString());
    }

    public void Write(ReadOnlySpan<byte> value)
    {
        writer.WriteBase64StringValue(value);
    }
}
