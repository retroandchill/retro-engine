// // @file TestStructuredJsonReader.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Text.Json;
using RetroEngine.Portable.Serialization;
using RetroEngine.Portable.Serialization.Json;

namespace RetroEngine.Portable.Test.Serialization;

public class JsonSerializationTest
{
    private sealed record ManualTestObject(int Id, string Name, bool IsActive) { }

    private static void Serialize<TWriter>(ref TWriter writer, ManualTestObject obj)
        where TWriter : IStructuredWriter, allows ref struct
    {
        writer.BeginObject();
        writer.BeginProperty(nameof(obj.Id));
        writer.Write(obj.Id);
        writer.EndProperty();
        writer.BeginProperty(nameof(obj.Name));
        writer.Write(obj.Name);
        writer.EndProperty();
        writer.BeginProperty(nameof(obj.IsActive));
        writer.Write(obj.IsActive);
        writer.EndProperty();
        writer.EndObject();
    }

    private static ManualTestObject Deserialize<TReader>(ref TReader reader)
        where TReader : IStructuredReader, allows ref struct
    {
        reader.BeginObject();
        reader.BeginProperty(nameof(ManualTestObject.Id));
        var id = reader.ReadInt32();
        reader.EndProperty();
        reader.BeginProperty(nameof(ManualTestObject.Name));
        var name = reader.ReadString();
        reader.EndProperty();
        reader.BeginProperty(nameof(ManualTestObject.IsActive));
        var isActive = reader.ReadBoolean();
        reader.EndProperty();
        reader.EndObject();

        return new ManualTestObject(id, name, isActive);
    }

    [Test]
    public void CanSerializeAndDeserialize()
    {
        var obj = new ManualTestObject(123, "Test", true);
        var binaryWriter = new ArrayBufferWriter<byte>();
        var jsonWriter = new Utf8JsonWriter(binaryWriter);
        var structuredWriter = new StructuredJsonWriter(jsonWriter);
        Serialize(ref structuredWriter, obj);
        jsonWriter.Flush();

        var structuredReader = new StructuredJsonReader(binaryWriter.WrittenSpan);
        var deserializedObj = Deserialize(ref structuredReader);

        Assert.AreEqual(obj.Id, deserializedObj.Id);
        Assert.AreEqual(obj.Name, deserializedObj.Name);
        Assert.AreEqual(obj.IsActive, deserializedObj.IsActive);
    }
}
