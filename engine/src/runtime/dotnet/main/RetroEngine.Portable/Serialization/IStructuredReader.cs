// // @file IStructuredReader.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Portable.Serialization;

public interface IStructuredReader
{
    void BeginObject();
    void EndObject();
    void BeginProperty(ReadOnlySpan<char> name);
    void EndProperty();
    bool TryBeginProperty(ReadOnlySpan<char> name);

    int BeginArray();
    void EndArray();
    void BeginArrayItem();
    void EndArrayItem();

    int BeginDictionary();
    void EndDictionary();
    string BeginDictionaryItem();
    void EndDictionaryItem();

    void ReadNull();
    bool ReadBoolean();
    char ReadChar();
    Rune ReadRune();
    byte ReadByte();
    sbyte ReadSByte();
    short ReadInt16();
    ushort ReadUInt16();
    int ReadInt32();
    uint ReadUInt32();
    long ReadInt64();
    ulong ReadUInt64();
    float ReadSingle();
    double ReadDouble();
    Guid ReadGuid();
    DateTime ReadDateTime();
    DateTimeOffset ReadDateTimeOffset();
    Name ReadName();
    string ReadString();
    Text ReadText();
    byte[] ReadBytes();
}
