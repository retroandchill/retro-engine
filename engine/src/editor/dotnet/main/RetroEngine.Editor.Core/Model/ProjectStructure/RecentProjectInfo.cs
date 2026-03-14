// // @file RecentProject.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace RetroEngine.Editor.Core.Model.ProjectStructure;

public sealed record RecentProjectInfo
{
    public required long Id { get; init; }

    public required string Path { get; init; } = "";

    public required string Name { get; init; } = "";

    public ProjectDescriptor? Descriptor { get; init; }

    [MemberNotNullWhen(true, nameof(Descriptor))]
    public bool Exists => Descriptor is not null;
}
