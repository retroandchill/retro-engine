// // @file MethodMetadata.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MagicArchive.SourceGenerator.Model;

public sealed class MethodMetadata
{
    public IMethodSymbol Symbol { get; }
    public string Name { get; }
    public bool IsStatic { get; }
    public bool IsValueType { get; }
    public bool UseReaderArgument { get; }
    public bool UseWriterArgument { get; }

    public MethodMetadata(IMethodSymbol symbol, bool isValueType, bool isReader)
    {
        Symbol = symbol;
        Name = symbol.Name;
        IsStatic = symbol.IsStatic;
        IsValueType = isValueType;

        if (symbol.Parameters.Length == 0)
            return;
        if (isReader)
        {
            UseReaderArgument = true;
        }
        else
        {
            UseWriterArgument = true;
        }
    }

    public Location GetLocation(TypeDeclarationSyntax fallback)
    {
        return Symbol.Locations.FirstOrDefault() ?? fallback.Identifier.GetLocation();
    }

    public string Emit()
    {
        var instance =
            (IsStatic) ? ""
            : (IsValueType) ? "value."
            : "value?.";

        if (UseReaderArgument)
        {
            return $"{instance}{Name}(ref reader, ref value);";
        }

        return UseWriterArgument ? $"{instance}{Name}(ref writer, ref value);" : $"{instance}{Name}();";
    }
}
