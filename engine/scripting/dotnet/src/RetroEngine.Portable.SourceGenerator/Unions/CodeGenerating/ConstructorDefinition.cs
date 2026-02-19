// // @file ConstructorDefinition.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace RetroEngine.Portable.SourceGenerator.Unions.CodeGenerating;

public sealed record ConstructorDefinition
{
    public Accessibility? Accessibility { get; init; }

    public bool IsStatic { get; init; }

    public ImmutableArray<MethodParameter> Parameters { get; init; } = [];

    public required Action<ConstructorDefinition, CodeWriter> BodyWriter { get; init; }
}
