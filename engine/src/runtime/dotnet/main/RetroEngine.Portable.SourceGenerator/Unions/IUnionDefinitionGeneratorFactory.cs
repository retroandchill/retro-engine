// // @file IUnionDefinitionGeneratorFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.SourceGenerator.Unions.CodeAnalyzing;

namespace RetroEngine.Portable.SourceGenerator.Unions;

public interface IUnionDefinitionGeneratorFactory
{
    IUnionDefinitionGenerator Create(UnionInfo union);
}
