// @file $BindMethodInfo.cs.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
using System.Collections.Immutable;

namespace RetroEngine.SourceGenerator.Model;

public record BindMethodParameter
{
    public required string Name { get; init; }
    public required string ManagedType { get; init; }
    public required string SizeofName { get; init; }
    public required string Prefix { get; init; }
    public required string CppType { get; init; }
}

public record BindMethodInfo
{
    public required string Name { get; init; }
    public required string ManagedReturnType { get; init; }
    public required string CppReturnType { get; init; }
    public required bool ReturnsVoid { get; init; }
    public required ImmutableArray<BindMethodParameter> Parameters { get; init; }
}
