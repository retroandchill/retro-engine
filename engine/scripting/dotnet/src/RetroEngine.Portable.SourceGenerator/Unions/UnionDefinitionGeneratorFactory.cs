// // @file UnionDefinitionGeneratorFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.SourceGenerator.Unions.CodeAnalyzing;

namespace RetroEngine.Portable.SourceGenerator.Unions;

public sealed class UnionDefinitionGeneratorFactory : IUnionDefinitionGeneratorFactory
{
    public IUnionDefinitionGenerator Create(UnionInfo union) =>
        union.TypeInfo.Kind.Match(
            IUnionDefinitionGenerator (_) => new ClassUnionDefinitionGenerator(union),
            (_, _) => new StructUnionDefinitionGenerator(union),
            () => throw new ArgumentException("Can't create generator for unknown union type kind", nameof(union))
        );
}
