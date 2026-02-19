// // @file UnionInfo.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace RetroEngine.Portable.SourceGenerator.Unions.CodeAnalyzing;

public readonly record struct UnionCaseParameterInfo(string Name, TypeName TypeName, bool ContainsGenericParameters);

public readonly record struct UnionCaseInfo(string Name, ImmutableArray<UnionCaseParameterInfo> Parameters)
{
    public bool HasParameters => Parameters.Length > 0;
}

public record UnionInfo(string Name, ImmutableArray<UnionCaseInfo> Cases, TypeInfo TypeInfo);
