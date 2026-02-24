// // @file UnionInfoCollector.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Retro.SourceGeneratorUtilities.Utilities.Attributes;
using RetroEngine.Portable.Utils;

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
                        new TypeName(CreateTypeInfo(y.Type), y.NullableAnnotation == NullableAnnotation.Annotated),
                        ContainsGenericParameters(y.Type)
                    )),
                ]
            ))
            .ToImmutableArray();

        return new UnionInfo(unionTypeSymbol.Name, unionCases, CreateTypeInfo(unionTypeSymbol));
    }

    private static TypeInfo CreateTypeInfo(ITypeSymbol typeSymbol) =>
        new(
            typeSymbol.ContainingNamespace is { IsGlobalNamespace: false }
                ? typeSymbol.ContainingNamespace.ToDisplayString()
                : null,
            typeSymbol.ContainingType != null ? CreateTypeInfo(typeSymbol.ContainingType) : null,
            typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
            typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            typeSymbol switch
            {
                { IsUnmanagedType: true } => TypeInfo.TypeKind.ValueType(true, typeSymbol.IsRecord),
                { IsValueType: true } => TypeInfo.TypeKind.ValueType(false, typeSymbol.IsRecord),
                { IsReferenceType: true, IsRecord: true } => TypeInfo.TypeKind.ReferenceType(
                    TypeInfo.ReferenceTypeKind.Record
                ),
                { IsReferenceType: true, IsRecord: false } => TypeInfo.TypeKind.ReferenceType(
                    typeSymbol.TypeKind == TypeKind.Interface
                        ? TypeInfo.ReferenceTypeKind.Interface
                        : TypeInfo.ReferenceTypeKind.Class
                ),
                _ => TypeInfo.TypeKind.Unknown(),
            }
        );

    private static bool ContainsGenericParameters(ITypeSymbol typeSymbol) =>
        typeSymbol switch
        {
            ITypeParameterSymbol => true,
            INamedTypeSymbol { IsGenericType: true } namedTypeSymbol => namedTypeSymbol.TypeArguments.Any(
                ContainsGenericParameters
            ),
            IArrayTypeSymbol arrayTypeSymbol => ContainsGenericParameters(arrayTypeSymbol.ElementType),
            _ => false,
        };
}
