// // @file ArchivableInfo.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;

namespace MagicArchive.SourceGenerator.Model;

[AttributeInfoType<ArchivableAttribute>]
public readonly record struct ArchivableInfo(GenerateType GenerateType, SerializeLayout SerializeLayout)
{
    public ArchivableInfo(GenerateType generateType = GenerateType.Object)
        : this(
            generateType,
            generateType is GenerateType.VersionTolerant or GenerateType.CircularReference
                ? SerializeLayout.Explicit
                : SerializeLayout.Sequential
        ) { }

    public ArchivableInfo(SerializeLayout serializeLayout)
        : this(GenerateType.Object, serializeLayout) { }
}

[AttributeInfoType<ArchivableUnionAttribute>]
public readonly record struct ArchivableUnionInfo(ushort Tag, ITypeSymbol Type);

[AttributeInfoType<ArchiveOrderAttribute>]
public readonly record struct ArchiveOrderInfo(int Order);
