// // @file NameYamlConverter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Portable.Strings;
using VYaml.Emitter;
using VYaml.Parser;
using VYaml.Serialization;

namespace RetroEngine.Portable.Serialization.Yaml;

public sealed class NameYamlConverter : IYamlFormatter<Name>
{
    public void Serialize(ref Utf8YamlEmitter emitter, Name value, YamlSerializationContext context)
    {
        Span<byte> stringText = stackalloc byte[Name.MaxRenderedLength];
        var actualLength = value.ToUtf8(stringText);
        emitter.WriteScalar(MemoryMarshal.CreateReadOnlySpan(ref MemoryMarshal.GetReference(stringText), actualLength));
    }

    public Name Deserialize(ref YamlParser parser, YamlDeserializationContext context)
    {
        var stringText = parser.GetScalarAsUtf8();
        return new Name(stringText);
    }
}
