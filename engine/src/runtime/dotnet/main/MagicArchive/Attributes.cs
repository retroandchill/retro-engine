// // @file Attributes.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace MagicArchive;

public enum GenerateType
{
    Object,
    VersionTolerant,
    CircularReference,
    Collection,
    Custom,
    NoGenerate,
}

public enum SerializeLayout
{
    Sequential,
    Explicit,
}

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface,
    AllowMultiple = false,
    Inherited = false
)]
public sealed class ArchivableAttribute(GenerateType generateType, SerializeLayout serializeLayout) : Attribute
{
    public GenerateType GenerateType { get; } = generateType;

    public SerializeLayout SerializeLayout { get; } = serializeLayout;

    public ArchivableAttribute(GenerateType generateType = GenerateType.Object)
        : this(
            generateType,
            generateType is GenerateType.VersionTolerant or GenerateType.CircularReference
                ? SerializeLayout.Explicit
                : SerializeLayout.Sequential
        ) { }

    public ArchivableAttribute(SerializeLayout serializeLayout)
        : this(GenerateType.Object, serializeLayout) { }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class ArchiveOrderAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}
