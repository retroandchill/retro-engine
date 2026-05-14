// // @file IRecentProjectsActions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Editor.Core.Model.ProjectStructure;

namespace RetroEngine.Editor.Core.Services.Actions;

public interface IRecentProjectsActions
{
    event Action<RecentProjectInfo>? OnProjectDeleted;

    Task<IEnumerable<RecentProjectInfo>> GetRecentProjectsAsync(
        string? searchTerm = null,
        CancellationToken cancellationToken = default
    );

    Task CreateNewProjectAsync(CancellationToken cancellationToken = default);

    Task OpenProjectAsync(CancellationToken cancellationToken = default);

    Task OpenRecentProjectAsync(RecentProjectInfo project, CancellationToken cancellationToken = default);

    Task DeleteRecentProjectAsync(RecentProjectInfo project, CancellationToken cancellationToken = default);
}
