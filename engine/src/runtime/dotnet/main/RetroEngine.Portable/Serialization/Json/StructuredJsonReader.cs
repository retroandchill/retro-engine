// // @file StructuredJsonReader.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Text;
using System.Text.Json;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Localization.Stringification;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Portable.Serialization.Json;

public ref struct StructuredJsonReader(ReadOnlySpan<byte> bytes, JsonReaderOptions options = default)
    : IStructuredReader
{
    private readonly record struct SavedJsonReaderState(JsonReaderState State, int Position);

    private readonly ReadOnlySpan<byte> _bytes = bytes;
    private Utf8JsonReader _reader = new(bytes, options);
    private readonly List<Dictionary<string, SavedJsonReaderState>> _propertyStack = [];
    private SavedJsonReaderState? _currentProperty;

    public void BeginObject()
    {
        _reader.Read();
        if (_reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of object");
        }

        var propertyCollection = new Dictionary<string, SavedJsonReaderState>();
        while (true)
        {
            _reader.Read();
            if (_reader.TokenType == JsonTokenType.EndObject)
                break;

            if (_reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected property name");

            var propertyName = _reader.GetString()!;
            propertyCollection.Add(
                propertyName,
                new SavedJsonReaderState(_reader.CurrentState, (int)_reader.BytesConsumed)
            );
            _reader.Skip();
        }

        _propertyStack.Add(propertyCollection);
    }

    public void EndObject()
    {
        if (_propertyStack.Count == 0)
            throw new InvalidOperationException("EndObject called without matching BeginObject");

        if (_reader.TokenType != JsonTokenType.EndObject)
        {
            throw new JsonException("Expected end of object");
        }

        _propertyStack.RemoveAt(_propertyStack.Count - 1);
    }

    public void BeginProperty(ReadOnlySpan<char> name)
    {
        if (!TryBeginProperty(name))
            throw new JsonException($"Property '{name}' not found");
    }

    public void EndProperty()
    {
        if (_currentProperty is null)
            throw new InvalidOperationException("EndProperty called without matching BeginProperty");

        _currentProperty = null;
    }

    public bool TryBeginProperty(ReadOnlySpan<char> name)
    {
        if (_propertyStack.Count == 0)
            throw new InvalidOperationException("BeginProperty called without matching BeginObject");

        if (_currentProperty is not null)
            throw new InvalidOperationException("BeginProperty called without matching EndProperty");

        var currentState = _propertyStack[^1];
        var lookup = currentState.GetAlternateLookup<ReadOnlySpan<char>>();
        if (!lookup.TryGetValue(name, out var state))
            return false;

        _currentProperty = state;
        return true;
    }

    public int BeginArray()
    {
        _reader.Read();
        if (_reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected start of array");
        }

        var arrayCount = 0;
        var reader = _reader;
        while (true)
        {
            reader.Read();
            if (reader.TokenType == JsonTokenType.EndArray)
                break;
            arrayCount++;

            reader.Skip();
        }

        return arrayCount;
    }

    public void EndArray()
    {
        if (_reader.TokenType != JsonTokenType.EndArray)
        {
            throw new JsonException("Expected end of array");
        }
    }

    public void BeginArrayItem()
    {
        // No-op
    }

    public void EndArrayItem()
    {
        // No-op
    }

    public int BeginDictionary()
    {
        _reader.Read();
        if (_reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of object");
        }

        var reader = _reader;
        var count = 0;
        while (true)
        {
            reader.Read();
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected property name");

            count++;
            reader.Read();
            reader.Skip();
        }

        return count;
    }

    public void EndDictionary()
    {
        _reader.Read();
        if (_reader.TokenType != JsonTokenType.EndObject)
        {
            throw new JsonException("Expected end of object");
        }
    }

    public string BeginDictionaryItem()
    {
        _reader.Read();
        if (_reader.TokenType != JsonTokenType.PropertyName)
            throw new JsonException("Expected property name");
        return _reader.GetString() ?? throw new JsonException("Expected property name");
    }

    public void EndDictionaryItem()
    {
        // No-op
    }

    private Utf8JsonReader CreateTemporaryReader(SavedJsonReaderState state)
    {
        return new Utf8JsonReader(_bytes[state.Position..], true, state.State);
    }

    public void ReadNull()
    {
        if (_currentProperty is null)
        {
            ReadNull(ref _reader);
            return;
        }

        var reader = CreateTemporaryReader(_currentProperty.Value);
        ReadNull(ref reader);
    }

    private static void ReadNull(ref Utf8JsonReader reader)
    {
        reader.Read();
        if (reader.TokenType != JsonTokenType.Null)
            throw new JsonException("Expected null");
    }

    public bool ReadBoolean()
    {
        if (_currentProperty is null)
        {
            return ReadBoolean(ref _reader);
        }

        var reader = CreateTemporaryReader(_currentProperty.Value);
        return ReadBoolean(ref reader);
    }

    private static bool ReadBoolean(ref Utf8JsonReader reader)
    {
        reader.Read();
        return reader.GetBoolean();
    }

    public char ReadChar()
    {
        return ReadString()[0];
    }

    public Rune ReadRune()
    {
        var str = ReadString();
        var result = Rune.DecodeFromUtf16(str, out var rune, out _);
        return result == OperationStatus.Done ? rune : throw new JsonException("Expected valid UTF-16 rune");
    }

    public byte ReadByte()
    {
        if (_currentProperty is null)
        {
            return ReadByte(ref _reader);
        }

        var reader = CreateTemporaryReader(_currentProperty.Value);
        return ReadByte(ref reader);
    }

    private static byte ReadByte(ref Utf8JsonReader reader)
    {
        reader.Read();
        return reader.GetByte();
    }

    public sbyte ReadSByte()
    {
        if (_currentProperty is null)
        {
            return ReadSByte(ref _reader);
        }

        var reader = CreateTemporaryReader(_currentProperty.Value);
        return ReadSByte(ref reader);
    }

    private static sbyte ReadSByte(ref Utf8JsonReader reader)
    {
        reader.Read();
        return reader.GetSByte();
    }

    public short ReadInt16()
    {
        if (_currentProperty is null)
        {
            return ReadInt16(ref _reader);
        }

        var reader = CreateTemporaryReader(_currentProperty.Value);
        return ReadInt16(ref reader);
    }

    private static short ReadInt16(ref Utf8JsonReader reader)
    {
        reader.Read();
        return reader.GetInt16();
    }

    public ushort ReadUInt16()
    {
        if (_currentProperty is null)
        {
            return ReadUInt16(ref _reader);
        }

        var reader = CreateTemporaryReader(_currentProperty.Value);
        return ReadUInt16(ref reader);
    }

    private static ushort ReadUInt16(ref Utf8JsonReader reader)
    {
        reader.Read();
        return reader.GetUInt16();
    }

    public int ReadInt32()
    {
        if (_currentProperty is null)
        {
            return ReadInt32(ref _reader);
        }

        var reader = CreateTemporaryReader(_currentProperty.Value);
        return ReadInt32(ref reader);
    }

    private static int ReadInt32(ref Utf8JsonReader reader)
    {
        reader.Read();
        return reader.GetInt32();
    }

    public uint ReadUInt32()
    {
        if (_currentProperty is null)
        {
            return ReadUInt32(ref _reader);
        }

        var reader = CreateTemporaryReader(_currentProperty.Value);
        return ReadUInt32(ref reader);
    }

    private static uint ReadUInt32(ref Utf8JsonReader reader)
    {
        reader.Read();
        return reader.GetUInt32();
    }

    public long ReadInt64()
    {
        if (_currentProperty is null)
        {
            return ReadInt64(ref _reader);
        }

        var reader = CreateTemporaryReader(_currentProperty.Value);
        return ReadInt64(ref reader);
    }

    private static long ReadInt64(ref Utf8JsonReader reader)
    {
        reader.Read();
        return reader.GetInt64();
    }

    public ulong ReadUInt64()
    {
        if (_currentProperty is null)
        {
            return ReadUInt64(ref _reader);
        }

        var reader = CreateTemporaryReader(_currentProperty.Value);
        return ReadUInt64(ref reader);
    }

    private static ulong ReadUInt64(ref Utf8JsonReader reader)
    {
        reader.Read();
        return reader.GetUInt64();
    }

    public float ReadSingle()
    {
        if (_currentProperty is null)
        {
            return ReadSingle(ref _reader);
        }

        var reader = CreateTemporaryReader(_currentProperty.Value);
        return ReadSingle(ref reader);
    }

    private static float ReadSingle(ref Utf8JsonReader reader)
    {
        reader.Read();
        return reader.GetSingle();
    }

    public double ReadDouble()
    {
        if (_currentProperty is null)
        {
            return ReadDouble(ref _reader);
        }

        var reader = CreateTemporaryReader(_currentProperty.Value);
        return ReadDouble(ref reader);
    }

    private static double ReadDouble(ref Utf8JsonReader reader)
    {
        reader.Read();
        return reader.GetDouble();
    }

    public Guid ReadGuid()
    {
        if (_currentProperty is null)
        {
            return ReadGuid(ref _reader);
        }

        var reader = CreateTemporaryReader(_currentProperty.Value);
        return ReadGuid(ref reader);
    }

    private static Guid ReadGuid(ref Utf8JsonReader reader)
    {
        reader.Read();
        return reader.GetGuid();
    }

    public DateTime ReadDateTime()
    {
        if (_currentProperty is null)
        {
            return ReadDateTime(ref _reader);
        }

        var reader = CreateTemporaryReader(_currentProperty.Value);
        return ReadDateTime(ref reader);
    }

    private static DateTime ReadDateTime(ref Utf8JsonReader reader)
    {
        reader.Read();
        return reader.GetDateTime();
    }

    public DateTimeOffset ReadDateTimeOffset()
    {
        if (_currentProperty is null)
        {
            return ReadDateTimeOffset(ref _reader);
        }

        var reader = CreateTemporaryReader(_currentProperty.Value);
        return ReadDateTimeOffset(ref reader);
    }

    private static DateTimeOffset ReadDateTimeOffset(ref Utf8JsonReader reader)
    {
        reader.Read();
        return reader.GetDateTimeOffset();
    }

    public Name ReadName()
    {
        return ReadString();
    }

    public string ReadString()
    {
        if (_currentProperty is null)
        {
            return ReadString(ref _reader);
        }

        var reader = CreateTemporaryReader(_currentProperty.Value);
        return ReadString(ref reader);
    }

    private static string ReadString(ref Utf8JsonReader reader)
    {
        reader.Read();
        return reader.GetString() ?? throw new JsonException("Expected string");
    }

    public Text ReadText()
    {
        return TextStringifier.ImportFromString(ReadString());
    }

    public byte[] ReadBytes()
    {
        if (_currentProperty is null)
        {
            return ReadBytes(ref _reader);
        }

        var reader = CreateTemporaryReader(_currentProperty.Value);
        return ReadBytes(ref reader);
    }

    private static byte[] ReadBytes(ref Utf8JsonReader reader)
    {
        reader.Read();
        return reader.GetBytesFromBase64();
    }
}
