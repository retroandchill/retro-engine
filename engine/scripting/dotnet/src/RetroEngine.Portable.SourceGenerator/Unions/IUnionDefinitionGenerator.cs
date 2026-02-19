// // @file IUnionDefinitionGenerator.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.SourceGenerator.Unions.CodeAnalyzing;
using RetroEngine.Portable.SourceGenerator.Unions.CodeGenerating;

namespace RetroEngine.Portable.SourceGenerator.Unions;

public interface IUnionDefinitionGenerator
{
    TypeKind TypeKind { get; }

    Action<MethodDefinition, CodeWriter> GetUnionCaseMethodBodyWriter(UnionCaseInfo unionCase);

    string GetUnionCaseCheckExpression(UnionCaseInfo unionCase);

    IEnumerable<string> GetUnionCaseParameterAccessors(UnionCaseInfo unionCase);

    MethodDefinition AdjustDefaultEqualsMethod(MethodDefinition equalsMethod);

    MethodDefinition AdjustSpecificEqualsMethod(MethodDefinition equalsMethod);

    Action<MethodDefinition, CodeWriter> GetGetHashCodeMethodBodyWriter();

    Action<OperatorDefinition, CodeWriter> GetEqualityOperatorBodyWriter();

    TypeDefinition AdjustUnionTypeDefinition(TypeDefinition typeDefinition);

    IReadOnlyList<TypeDefinition> GetAdditionalTypes();
}
