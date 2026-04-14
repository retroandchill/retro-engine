// // @file Helpers.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using HandlebarsDotNet;
using MagicArchive.SourceGenerator.Model;
using Microsoft.CodeAnalysis;

namespace MagicArchive.SourceGenerator.Formatters;

public static class Helpers
{
    public static void Escaped(EncodedTextWriter writer, Context context, Arguments arguments)
    {
        var argument = arguments.At<string>(0);

        var xmlDocument = context["XmlDocument"] as bool? ?? false;
        var argValue = context[argument];
        if (argValue is not ISymbol symbol)
        {
            writer.Write(argValue);
            return;
        }

        var str = symbol.FullyQualifiedToString().Replace("global::", "");
        writer.Write(xmlDocument ? str.Replace("<", "&lt;").Replace(">", "&gt;") : str);
    }

    public static void MemberWriter(EncodedTextWriter writer, Context context, Arguments arguments)
    {
        if (context.Value is not MemberMetadata member)
            return;

        var writerName = arguments.At<string>(0);
        writer.Write(member.EmitSerialize(writerName));
    }

    public static void MemberReader(EncodedTextWriter writer, Context context, Arguments arguments)
    {
        if (context.Value is not MemberMetadata member)
            return;

        var readerName = arguments.At<string>(0);
        writer.Write(member.EmitDeserialize(readerName));
    }

    public static void MemberRefReader(EncodedTextWriter writer, Context context, Arguments arguments)
    {
        if (context.Value is not MemberMetadata member)
            return;

        var readerName = arguments.At<string>(0);
        writer.Write(member.EmitRefDeserialize(readerName));
    }

    public static void ConstructorParameters(EncodedTextWriter writer, Context context, Arguments arguments)
    {
        if (context.Value is not TypeMetadata type)
            return;

        writer.Write(type.EmitConstructorParameters());
    }
}
