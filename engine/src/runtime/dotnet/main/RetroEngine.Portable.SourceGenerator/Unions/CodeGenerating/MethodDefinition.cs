// // @file MethodDefinition.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using RetroEngine.Portable.SourceGenerator.Unions.CodeAnalyzing;

namespace RetroEngine.Portable.SourceGenerator.Unions.CodeGenerating;

public enum MethodParameterModifier : byte
{
    In,
    Ref,
    Out,
}

public readonly record struct MethodParameter(TypeName TypeName, string Name, MethodParameterModifier? Modifier = null);

public enum MethodModifier : byte
{
    Static,
    Abstract,
    Virtual,
    Override,
}

public readonly record struct GenericParameter(string Name, ImmutableArray<string> Constraints);

public sealed record MethodDefinition
{
    public Accessibility? Accessibility { get; init; }

    public required TypeName ReturnType { get; init; }

    public required string Name { get; init; }

    public MethodModifier? MethodModifier { get; init; }

    public bool IsPartial { get; init; }

    public ImmutableArray<MethodParameter> Parameters { get; init; } = [];

    public ImmutableArray<GenericParameter> GenericParameters { get; init; } = [];

    public Action<MethodDefinition, CodeWriter>? BodyWriter { get; init; }
}
