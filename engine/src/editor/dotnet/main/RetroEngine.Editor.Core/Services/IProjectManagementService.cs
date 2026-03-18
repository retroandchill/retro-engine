// // @file IProjectManagementService.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Avalonia.Utilities;
using RetroEngine.Editor.Core.Model.ProjectStructure;

namespace RetroEngine.Editor.Core.Services;

public readonly record struct ProjectOpenedEventArgs(ProjectDescriptor Project);

public readonly record struct ProjectClosedEventArgs;

public interface IProjectManagementService
{
    ProjectDescriptor? CurrentProject { get; }

    event EventHandler<ProjectOpenedEventArgs>? ProjectOpened;

    event EventHandler<ProjectClosedEventArgs>? ProjectClosed;

    ValueTask<IEnumerable<RecentProjectInfo>> GetRecentProjectsAsync(
        int maxNumber = 10,
        CancellationToken cancellationToken = default
    );

    Task RemoveRecentProjectAsync(string path, CancellationToken cancellationToken = default);

    Task CreateNewProjectAsync(string path, CancellationToken cancellationToken = default);

    Task OpenProjectAsync(string path, CancellationToken cancellationToken = default);

    void CloseProject();

    Task SaveCurrentProjectAsync(CancellationToken cancellationToken = default);
}
