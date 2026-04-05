// // @file UnionSourceGenerator.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;

namespace RetroEngine.Utilities.SourceGenerator.Unions;

[Generator]
public sealed class UnionSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context) =>
        UnionSourceGeneratorBootstrapper.Bootstrap(
            context,
            new UnionCodeGenerator(new UnionDefinitionGeneratorFactory())
        );
}
