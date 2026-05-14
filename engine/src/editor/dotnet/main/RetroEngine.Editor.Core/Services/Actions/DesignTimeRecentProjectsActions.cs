// // @file DesignTimeRecentProjectsActions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Editor.Core.Model.ProjectStructure;

namespace RetroEngine.Editor.Core.Services.Actions;

internal sealed class DesignTimeRecentProjectsActions : IRecentProjectsActions
{
    private readonly RecentProjectInfo[] _recentProjects =
    [
        new()
        {
            Descriptor = new ProjectDescriptor(),
            Id = 1,
            Name = "Sample1",
            Path = @"C:\Users\user\Documents\RetroEngine\Projects\Sample1",
        },
        new()
        {
            Descriptor = new ProjectDescriptor(),
            Id = 1,
            Name = "Sample2",
            Path = @"C:\Users\user\Documents\RetroEngine\Projects\Sample2",
        },
        new()
        {
            Descriptor = new ProjectDescriptor(),
            Id = 1,
            Name = "Sample3",
            Path = @"C:\Users\user\Documents\RetroEngine\Projects\Sample3",
        },
        new()
        {
            Descriptor = new ProjectDescriptor(),
            Id = 1,
            Name = "Sample4",
            Path = @"C:\Users\user\Documents\RetroEngine\Projects\Sample4",
        },
    ];

    public event Action<RecentProjectInfo>? OnProjectDeleted;

    public Task<IEnumerable<RecentProjectInfo>> GetRecentProjectsAsync(
        string? searchTerm = null,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(
            _recentProjects.Where(x =>
                searchTerm is null || x.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
            )
        );
    }

    public Task CreateNewProjectAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task OpenProjectAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task OpenRecentProjectAsync(RecentProjectInfo project, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task DeleteRecentProjectAsync(RecentProjectInfo project, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
