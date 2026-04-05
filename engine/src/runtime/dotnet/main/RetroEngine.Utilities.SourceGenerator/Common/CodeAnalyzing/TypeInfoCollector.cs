// // @file TypeInfoCollector.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace RetroEngine.Utilities.SourceGenerator.Common.CodeAnalyzing;

public static class TypeInfoCollector
{
    public static TypeInfo CreateTypeInfo(ITypeSymbol typeSymbol) =>
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

    public static bool ContainsGenericParameters(ITypeSymbol typeSymbol) =>
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
