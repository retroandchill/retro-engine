// // @file Helpers.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using HandlebarsDotNet;
using MagicArchive.SourceGenerator.Model;

namespace MagicArchive.SourceGenerator.Formatters;

public static class Helpers
{
    public static void MemberWriter(EncodedTextWriter writer, Context context, Arguments arguments)
    {
        if (context.Value is not MemberMetadata member)
            return;

        var writeEnding = true;
        switch (member.Kind)
        {
            case MemberKind.Archivable:
            case MemberKind.ArchivableArray:
            case MemberKind.ArchivableList:
            case MemberKind.ArchivableCollection:
            case MemberKind.ArchivableUnion:
                writer.Write("writer.WriteArchivable(");
                break;
            case MemberKind.Nullable:
            case MemberKind.Bool:
            case MemberKind.Char:
            case MemberKind.Rune:
            case MemberKind.Byte:
            case MemberKind.SByte:
            case MemberKind.Int16:
            case MemberKind.UInt16:
            case MemberKind.Int32:
            case MemberKind.UInt32:
            case MemberKind.Int64:
            case MemberKind.UInt64:
            case MemberKind.Single:
            case MemberKind.Double:
            case MemberKind.Guid:
            case MemberKind.DateTimeOffset:
            case MemberKind.String:
            case MemberKind.Array:
            case MemberKind.Enum:
            case MemberKind.List:
                writer.Write("writer.Write(");
                break;
            case MemberKind.RefLike:
            case MemberKind.NonSerializable:
            case MemberKind.ArchivableNoGenerate:
            case MemberKind.Blank:
                writer.Write("writer.Write(");
                writeEnding = false;
                break;
            case MemberKind.AllowSerialize:
                break;
            case MemberKind.Union:
                break;
            case MemberKind.Object:
                break;
            case MemberKind.CustomFormatter:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (member is { IsField: true, MemberType.IsValueType: true })
        {
            writer.Write("in ");
        }

        writer.Write("value.");
        writer.Write(member.Name);
        writer.Write(");");
    }
}
