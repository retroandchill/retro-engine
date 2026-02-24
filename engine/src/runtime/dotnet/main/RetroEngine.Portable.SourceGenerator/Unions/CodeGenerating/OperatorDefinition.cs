// // @file OperatorDefinition.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using RetroEngine.Portable.SourceGenerator.Unions.CodeAnalyzing;

namespace RetroEngine.Portable.SourceGenerator.Unions.CodeGenerating;

public sealed class OperatorDefinition
{
    public required TypeName ReturnType { get; init; }

    public required string Name { get; init; }

    public ImmutableArray<MethodParameter> Parameters { get; init; } = [];

    public required Action<OperatorDefinition, CodeWriter> BodyWriter { get; init; }
}
