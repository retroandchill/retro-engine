// // @file ClassTypeFormatter.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using HandlebarsDotNet;
using HandlebarsDotNet.IO;
using MagicArchive.SourceGenerator.Model;

namespace MagicArchive.SourceGenerator.Formatters;

public sealed class ClassTypeFormatter : IFormatter, IFormatterProvider
{
    public void Format<T>(T value, in EncodedTextWriter writer)
    {
        if (value is not ClassType classType)
            throw new ArgumentException("Value must be of type ClassType", nameof(value));

        switch (classType)
        {
            case ClassType.Class:
                writer.Write("class");
                break;
            case ClassType.Struct:
                writer.Write("struct");
                break;
            case ClassType.Record:
                writer.Write("record");
                break;
            case ClassType.RecordStruct:
                writer.Write("record struct");
                break;
            case ClassType.Interface:
                writer.Write("interface");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public bool TryCreateFormatter(Type type, out IFormatter? formatter)
    {
        if (type != typeof(ClassType))
        {
            formatter = null;
            return false;
        }

        formatter = this;
        return true;
    }
}
