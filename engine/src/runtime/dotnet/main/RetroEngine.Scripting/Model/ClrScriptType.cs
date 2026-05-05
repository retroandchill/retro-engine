// // @file ClrScriptType.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Scripting.Model;

internal sealed record ClrScriptType : IScriptType
{
    public required Type ClrType { get; init; }
    public required string Name { get; init; }
    public string? Namespace { get; init; }
    public string? FullName { get; init; }
    public string? AssemblyQualifiedName { get; init; }
}
