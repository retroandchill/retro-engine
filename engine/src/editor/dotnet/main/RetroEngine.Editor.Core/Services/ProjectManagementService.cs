// // @file ProjectManagementService.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using RetroEngine.Editor.Core.Data;
using RetroEngine.Editor.Core.Data.Entities;
using RetroEngine.Editor.Core.Model.ProjectStructure;

namespace RetroEngine.Editor.Core.Services;

[RegisterScoped]
public sealed class ProjectManagementService(IProjectDescriptorSerializer serializer, CachedDbContext dbContext)
{
    private ProjectDescriptorFile? _currentProjectFile;

    public ProjectDescriptor? CurrentProject => _currentProjectFile?.Descriptor;

    public async ValueTask<IEnumerable<RecentProjectInfo>> GetRecentProjectsAsync(
        int maxNumber = 10,
        CancellationToken cancellationToken = default
    )
    {
        return await dbContext
            .RecentProjects.OrderByDescending(p => p.LastOpened)
            .Take(maxNumber)
            .ToAsyncEnumerable()
            .Select(
                async (project, c) =>
                {
                    var projectDescriptor = serializer.DoesProjectFileExist(project.Path)
                        ? await serializer.OpenProjectFileAsync(project.Path, c)
                        : null;

                    return new RecentProjectInfo
                    {
                        Id = project.Id,
                        Path = project.Path,
                        Name = project.Name,
                        Descriptor = projectDescriptor,
                    };
                }
            )
            .ToListAsync(cancellationToken);
    }

    public async Task CreateNewProjectAsync(string path, CancellationToken cancellationToken = default)
    {
        var projectDescriptor = await serializer.CreateNewProjectAsync(path, cancellationToken);
        _currentProjectFile = new ProjectDescriptorFile(projectDescriptor, path);
    }

    public async Task OpenProjectAsync(string path, CancellationToken cancellationToken = default)
    {
        var projectDescriptor = await serializer.OpenProjectFileAsync(path, cancellationToken);
        _currentProjectFile = new ProjectDescriptorFile(projectDescriptor, path);
    }

    public void CloseProject()
    {
        _currentProjectFile = null;
    }

    public async Task SaveCurrentProjectAsync(CancellationToken cancellationToken = default)
    {
        if (_currentProjectFile is null)
        {
            throw new InvalidOperationException("No project is currently open.");
        }

        var (descriptor, path) = _currentProjectFile.Value;
        await serializer.SaveProjectFileAsync(descriptor, path, cancellationToken);
    }
}
