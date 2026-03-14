// // @file ProjectDescriptor.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Editor.Core.Model.ProjectStructure;

public record ProjectDescriptor
{
    public int FileVersion { get; init; } = 1;

    public string? Category { get; init; }

    public string? Description { get; init; }
}

public readonly record struct ProjectDescriptorFile(ProjectDescriptor Descriptor, string FilePath);
