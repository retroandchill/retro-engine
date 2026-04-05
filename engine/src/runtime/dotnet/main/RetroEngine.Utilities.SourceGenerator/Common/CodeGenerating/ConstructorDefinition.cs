// // @file ConstructorDefinition.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using RetroEngine.Utilities.SourceGenerator.Unions;

namespace RetroEngine.Utilities.SourceGenerator.Common.CodeGenerating;

public enum OtherConstructorKind : byte
{
    Base,
    This,
}

public readonly record struct OtherConstructorCall(OtherConstructorKind Kind, ImmutableArray<string> Arguments);

public sealed record ConstructorDefinition
{
    public Accessibility? Accessibility { get; init; }

    public bool IsStatic { get; init; }

    public ImmutableArray<MethodParameter> Parameters { get; init; } = [];

    public OtherConstructorCall? OtherConstructorCall { get; init; }

    public Action<ConstructorDefinition, CodeWriter>? BodyWriter { get; init; }
}
