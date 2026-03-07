// // @file UnionInfoCollector.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;
using RetroEngine.Portable.SourceGenerator.Common.CodeAnalyzing;
using RetroEngine.Portable.Utils;
using TypeInfo = RetroEngine.Portable.SourceGenerator.Common.CodeAnalyzing.TypeInfo;

namespace RetroEngine.Portable.SourceGenerator.Unions.CodeAnalyzing;

public static class UnionInfoCollector
{
    public static UnionInfo Collect(INamedTypeSymbol unionTypeSymbol)
    {
        var unionCases = unionTypeSymbol
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Where(methodSymbol => methodSymbol.HasAttribute<UnionCaseAttribute>())
            .Select(x => new UnionCaseInfo(
                x.Name,
                [
                    .. x.Parameters.Select(y => new UnionCaseParameterInfo(
                        y.Name,
                        new TypeName(
                            TypeInfoCollector.CreateTypeInfo(y.Type),
                            y.NullableAnnotation == NullableAnnotation.Annotated
                        ),
                        TypeInfoCollector.ContainsGenericParameters(y.Type)
                    )),
                ]
            ))
            .ToImmutableArray();

        return new UnionInfo(unionTypeSymbol.Name, unionCases, TypeInfoCollector.CreateTypeInfo(unionTypeSymbol));
    }
}
