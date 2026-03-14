// // @file IProjectDescriptorSerializer.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Editor.Core.Model.ProjectStructure;

namespace RetroEngine.Editor.Core.Services;

public interface IProjectDescriptorSerializer
{
    bool DoesProjectFileExist(string path);

    Task<ProjectDescriptor> CreateNewProjectAsync(string path, CancellationToken cancellationToken = default);

    Task<ProjectDescriptor> OpenProjectFileAsync(string path, CancellationToken cancellationToken = default);

    Task SaveProjectFileAsync(
        ProjectDescriptor projectDescriptor,
        string path,
        CancellationToken cancellationToken = default
    );
}
