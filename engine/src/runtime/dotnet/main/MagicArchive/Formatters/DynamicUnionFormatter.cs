// // @file DynamicUnionFormatter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using ZLinq;

namespace MagicArchive.Formatters;

public sealed class DynamicUnionFormatter<T> : ArchiveFormatter<T>
    where T : class
{
    private readonly Dictionary<Type, ushort> _typeToTag;
    private readonly Dictionary<ushort, Type> _tagToType;

    public DynamicUnionFormatter(params ReadOnlySpan<(ushort Tag, Type Type)> memoryPackUnions)
    {
        _typeToTag = memoryPackUnions.AsValueEnumerable().ToDictionary(x => x.Type, x => x.Tag);
        _tagToType = memoryPackUnions.AsValueEnumerable().ToDictionary(x => x.Tag, x => x.Type);
    }

    public override void Serialize<TBufferWriter>(ref ArchiveWriter<TBufferWriter> writer, scoped in T? value)
    {
        if (value is null)
        {
            writer.WriteNullUnionHeader();
            return;
        }

        var type = value.GetType();
        if (_typeToTag.TryGetValue(type, out var tag))
        {
            writer.WriteUnionHeader(tag);
            writer.WriteValue(type, value);
        }
        else
        {
            ArchiveSerializationException.ThrowNotFoundInUnionType(type, typeof(T));
        }
    }

    public override void Deserialize(ref ArchiveReader reader, scoped ref T? value)
    {
        if (!reader.TryReadUnionHeader(out var tag))
        {
            value = null;
            return;
        }

        if (_tagToType.TryGetValue(tag, out var type))
        {
            object? v = value;
            reader.ReadValue(type, ref v);
            value = (T?)v;
        }
        else
        {
            ArchiveSerializationException.ThrowInvalidTag(tag, typeof(T));
        }
    }
}
