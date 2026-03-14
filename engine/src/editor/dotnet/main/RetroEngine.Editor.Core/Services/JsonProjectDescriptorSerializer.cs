// // @file JsonProjectDescriptorSerializer.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Abstractions;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RetroEngine.Editor.Core.Model.ProjectStructure;

namespace RetroEngine.Editor.Core.Services;

[RegisterSingleton]
public sealed class JsonProjectDescriptorSerializer(IFileSystem fileSystem, IOptions<JsonSerializerOptions> options)
    : IProjectDescriptorSerializer
{
    private readonly JsonSerializerOptions _options = options.Value;

    public bool DoesProjectFileExist(string path)
    {
        return fileSystem.File.Exists(path);
    }

    public async Task<ProjectDescriptor> CreateNewProjectAsync(
        string path,
        CancellationToken cancellationToken = default
    )
    {
        await using var stream = fileSystem.FileStream.New(path, FileMode.CreateNew);
        var descriptor = new ProjectDescriptor();
        await JsonSerializer.SerializeAsync(stream, descriptor, _options, cancellationToken);
        return descriptor;
    }

    public async Task<ProjectDescriptor> OpenProjectFileAsync(
        string path,
        CancellationToken cancellationToken = default
    )
    {
        await using var stream = fileSystem.FileStream.New(path, FileMode.Open);
        var projectDescriptor = await JsonSerializer.DeserializeAsync<ProjectDescriptor>(
            stream,
            _options,
            cancellationToken
        );
        return projectDescriptor ?? throw new JsonException($"Failed to deserialize project descriptor from {path}");
    }

    public async Task SaveProjectFileAsync(
        ProjectDescriptor projectDescriptor,
        string path,
        CancellationToken cancellationToken = default
    )
    {
        await using var stream = fileSystem.FileStream.New(path, FileMode.Open);
    }
}
