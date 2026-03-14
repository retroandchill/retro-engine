// // @file ProjectManagementService.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;
using RetroEngine.Editor.Core.Data;
using RetroEngine.Editor.Core.Model.ProjectStructure;

namespace RetroEngine.Editor.Core.Services;

[RegisterSingleton]
public sealed class ProjectManagementService(
    IProjectDescriptorSerializer serializer,
    IDbContextFactory<CachedDbContext> dbContextFactory
) : IProjectManagementService
{
    private ProjectDescriptorFile? CurrentProjectFile
    {
        get;
        set
        {
            field = value;
            if (value is not null)
            {
                ProjectOpened?.Invoke(this, new ProjectOpenedEventArgs(value.Value.Descriptor));
            }
            else
            {
                ProjectClosed?.Invoke(this, new ProjectClosedEventArgs());
            }
        }
    }

    public ProjectDescriptor? CurrentProject => CurrentProjectFile?.Descriptor;

    public event EventHandler<ProjectOpenedEventArgs>? ProjectOpened;
    public event EventHandler<ProjectClosedEventArgs>? ProjectClosed;

    public async ValueTask<IEnumerable<RecentProjectInfo>> GetRecentProjectsAsync(
        int maxNumber = 10,
        CancellationToken cancellationToken = default
    )
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
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
        CurrentProjectFile = new ProjectDescriptorFile(projectDescriptor, path);
    }

    public async Task OpenProjectAsync(string path, CancellationToken cancellationToken = default)
    {
        var projectDescriptor = await serializer.OpenProjectFileAsync(path, cancellationToken);
        CurrentProjectFile = new ProjectDescriptorFile(projectDescriptor, path);
    }

    public void CloseProject()
    {
        CurrentProjectFile = null;
    }

    public async Task SaveCurrentProjectAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentProjectFile is null)
        {
            throw new InvalidOperationException("No project is currently open.");
        }

        var (descriptor, path) = CurrentProjectFile.Value;
        await serializer.SaveProjectFileAsync(descriptor, path, cancellationToken);
    }
}
