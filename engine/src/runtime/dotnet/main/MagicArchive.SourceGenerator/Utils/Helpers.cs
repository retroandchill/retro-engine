// // @file Helpers.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Text;
using HandlebarsDotNet;
using MagicArchive.SourceGenerator.Model;
using Microsoft.CodeAnalysis;

namespace MagicArchive.SourceGenerator.Utils;

public static class Helpers
{
    public static void Joined(
        EncodedTextWriter output,
        BlockHelperOptions options,
        Context context,
        Arguments arguments
    )
    {
        if (arguments.Length != 2)
        {
            throw new HandlebarsException("Joined helper requires exactly two argument");
        }

        var delimiter = arguments.At<string>(0);
        var items = arguments.At<IEnumerable>(1);

        var i = 0;
        foreach (var item in items)
        {
            if (i > 0)
                output.Write(delimiter);

            options.Template(output, item);
            i++;
        }
    }

    public static void Equals(
        EncodedTextWriter output,
        BlockHelperOptions options,
        Context context,
        Arguments arguments
    )
    {
        if (arguments.Length != 2)
        {
            throw new HandlebarsException("Equals helper requires exactly two argument");
        }

        var left = arguments.At<int>(0);
        var right = arguments.At<int>(1);

        if (left == right)
        {
            options.Template(output, context);
        }
        else
        {
            options.Inverse(output, context);
        }
    }

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

    public static void Indented(
        EncodedTextWriter output,
        BlockHelperOptions options,
        Context context,
        Arguments arguments
    )
    {
        if (arguments.Length != 1)
        {
            throw new HandlebarsException("Intented helper requires exactly one argument");
        }

        var indent = arguments.At<string>(0);
        options.Data.CreateProperty("Indent", indent, out _);
        options.Template(output, context);
    }

    public static void SerializeMembers(
        EncodedTextWriter output,
        BlockHelperOptions options,
        Context context,
        Arguments arguments
    )
    {
        if (arguments.Length != 3)
        {
            throw new HandlebarsException($"{nameof(SerializeMembers)} helper requires exactly three arguments");
        }

        var members = arguments.At<IReadOnlyList<MemberMetadata>>(0);
        var toTempWriter = arguments.At<bool>(1);
        var writeObjectHeader = arguments.At<bool>(2);

        if (members.Count == 0 && writeObjectHeader)
        {
            options.Template(output, "writer.WriteObjectHeader(0);");
            return;
        }

        var writer = toTempWriter ? "tempWriter" : "writer";

        for (var i = 0; i < members.Count; i++)
        {
            if (members[i].Kind is not (MemberKind.Blittable or MemberKind.Enum) || toTempWriter)
            {
                if (i == 0 && writeObjectHeader)
                {
                    options.Template(output, $"{writer}.WriteObjectHeader({members.Count});");
                }

                options.Template(output, members[i].EmitSerialize(writer));
                if (toTempWriter)
                {
                    options.Template(output, $"offsets[{i}] = tempWriter.WrittenBytes;");
                }

                continue;
            }

            var optimizeFrom = i;
            var optimizeTo = i;
            var limit = Math.Min(members.Count, i + 15);
            for (var j = i; j < limit; j++)
            {
                if (members[j].Kind is MemberKind.Blittable or MemberKind.Enum)
                {
                    optimizeTo = j;
                }
                else
                {
                    break;
                }
            }

            var builder = new StringBuilder();
            builder.Append(writer);
            if (optimizeFrom == 0 && writeObjectHeader)
            {
                builder.Append(".WriteBlittableWithObjectHeader(");
                builder.Append(members.Count);
                builder.Append(", ");
            }
            else
            {
                builder.Append(".WriteBlittable(");
            }

            for (var index = optimizeFrom; index <= optimizeTo; index++)
            {
                if (index != i)
                {
                    builder.Append(", ");
                }

                builder.Append("value.@");
                builder.Append(members[index].Name);
            }
            builder.Append(");");
            options.Template(output, builder.ToString());

            i = optimizeTo;
        }
    }

    public static void DeserializeMembers(
        EncodedTextWriter output,
        BlockHelperOptions options,
        Context context,
        Arguments arguments
    )
    {
        if (arguments.Length != 2)
        {
            throw new HandlebarsException($"{nameof(DeserializeMembers)} helper requires exactly two argument");
        }

        var members = arguments.At<IReadOnlyList<MemberMetadata>>(0);
        var isTolerant = arguments.At<bool>(1);
        for (var i = 0; i < members.Count; i++)
        {
            if (members[i].Kind is not (MemberKind.Blittable or MemberKind.Enum) || isTolerant)
            {
                options.Template(output, members[i].EmitReadToDeserialize(i, isTolerant));
                continue;
            }

            var optimizeFrom = i;
            var optimizeTo = i;
            var limit = Math.Min(members.Count, i + 15);
            for (var j = i; j < limit; j++)
            {
                if (members[j].Kind is MemberKind.Blittable or MemberKind.Enum)
                {
                    optimizeTo = j;
                }
                else
                {
                    break;
                }
            }

            var builder = new StringBuilder();
            builder.Append("reader.ReadBlittable(");
            for (var index = optimizeFrom; index <= optimizeTo; index++)
            {
                if (index != i)
                {
                    builder.Append(", ");
                }

                builder.Append("out __");
                builder.Append(members[index].Name);
                builder.Append("__");
            }
            builder.Append(");");
            options.Template(output, builder.ToString());

            i = optimizeTo;
        }
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
        if (arguments.Length != 2)
        {
            throw new HandlebarsException($"{nameof(ConstructorParameters)} helper requires exactly two arguments");
        }

        var constructor = arguments[0] as IMethodSymbol;
        var members = arguments.At<IReadOnlyList<MemberMetadata>>(1);
        if (constructor is not { Parameters.Length: > 0 })
        {
            return;
        }

        var nameDict = members
            .Where(x => x.IsConstructorParameter)
            .ToDictionary(x => x.ConstructorParameterName, x => x.Name, StringComparer.OrdinalIgnoreCase);
        var parameters = constructor
            .Parameters.Select(x => nameDict.TryGetValue(x.Name, out var memberName) ? memberName : null)
            .OfType<string>();

        var i = 0;
        foreach (var parameter in parameters)
        {
            if (i > 0)
                writer.Write(", ");

            writer.Write("__");
            writer.Write(parameter);
            writer.Write("__");
            i++;
        }
    }
}
