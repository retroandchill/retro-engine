// // @file IScriptType.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Scripting.Model;

public interface IScriptType
{
    Type? ClrType { get; }

    string Name { get; }

    string? Namespace { get; }

    string? FullName { get; }

    string? AssemblyQualifiedName { get; }
}
