// // @file IUnionCodeGenerator.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using RetroEngine.Portable.SourceGenerator.Unions.CodeAnalyzing;

namespace RetroEngine.Portable.SourceGenerator.Unions;

public interface IUnionCodeGenerator
{
    public string Name { get; }

    public string? GenerateCode(UnionInfo unionInfo, INamedTypeSymbol unionTypeSymbol);
}
