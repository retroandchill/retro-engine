// // @file IStructuredReader.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Portable.Serialization;

public interface IStructuredWriter
{
    void BeginObject();
    void EndObject();
    void BeginProperty(ReadOnlySpan<char> name);
    void EndProperty();

    void BeginArray(int size);
    void EndArray();
    void BeginArrayItem();
    void EndArrayItem();

    void BeginDictionary(int size);
    void EndDictionary();
    void BeginDictionaryItem(ReadOnlySpan<char> key);
    void EndDictionaryItem();

    void Write(bool value);
    void Write(byte value);
    void Write(sbyte value);
    void Write(short value);
    void Write(ushort value);
    void Write(int value);
    void Write(uint value);
    void Write(long value);
    void Write(ulong value);
    void Write(float value);
    void Write(double value);
    void Write(Name value);
    void Write(ReadOnlySpan<char> value);
    void Write(Text value);
    void Write(ReadOnlySpan<byte> value);
}
