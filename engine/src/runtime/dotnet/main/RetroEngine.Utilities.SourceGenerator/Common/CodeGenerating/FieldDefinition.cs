// // @file FieldDefinition.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using RetroEngine.Utilities.SourceGenerator.Common.CodeAnalyzing;

namespace RetroEngine.Utilities.SourceGenerator.Common.CodeGenerating;

public sealed record FieldDefinition
{
    public Accessibility? Accessibility { get; init; }

    public bool IsStatic { get; init; }

    public bool IsReadOnly { get; init; }

    public required TypeName TypeName { get; init; }

    public required string Name { get; init; }

    public string? Initializer { get; init; }

    public ImmutableArray<string> Attributes { get; init; } = [];
}
