// // @file TypeDefinition.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using TypeInfo = RetroEngine.Portable.SourceGenerator.Unions.CodeAnalyzing.TypeInfo;

namespace RetroEngine.Portable.SourceGenerator.Unions.CodeGenerating;

public sealed record TypeDefinition
{
    public Accessibility? Accessibility { get; init; }

    public bool IsPartial { get; init; }

    public required string Name { get; init; }

    public string NameWithoutGenerics
    {
        get
        {
            if (field != null)
            {
                return field;
            }

            var genericStart = Name.IndexOf('<');
            if (genericStart < 0)
            {
                return field = Name;
            }

            return field = Name[..genericStart];
        }
    }

    public required TypeKind Kind { get; init; }

    public ImmutableArray<string> Attributes { get; init; } = [];

    public ImmutableArray<TypeInfo> InheritedTypes { get; init; } = [];

    public ImmutableArray<FieldDefinition> Fields { get; init; } = [];

    public ImmutableArray<PropertyDefinition> Properties { get; init; } = [];

    public ImmutableArray<ConstructorDefinition> Constructors { get; init; } = [];

    public ImmutableArray<MethodDefinition> Methods { get; init; } = [];

    public ImmutableArray<OperatorDefinition> Operators { get; init; } = [];

    public ImmutableArray<TypeDefinition> NestedTypes { get; init; } = [];
}
