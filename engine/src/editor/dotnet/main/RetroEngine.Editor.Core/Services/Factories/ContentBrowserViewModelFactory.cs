// // @file ContentBrowserViewModelFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Abstractions;
using RetroEngine.Editor.Core.ViewModels.Tabs;

namespace RetroEngine.Editor.Core.Services.Factories;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public sealed class ContentBrowserViewModelFactory(
    IFileSystem fileSystem,
    IProjectManagementService projectManagementService
) : ViewModelFactory<ContentBrowserViewModel>
{
    public override ContentBrowserViewModel CreateViewModel()
    {
        var browser = new ContentBrowserViewModel { FileSystem = fileSystem };

        if (projectManagementService.CurrentProjectPath is null)
            return browser;

        var directoryName = fileSystem.Path.GetDirectoryName(projectManagementService.CurrentProjectPath)!;
        foreach (var folder in GetContentBrowserFolders(directoryName))
        {
            browser.Folders.Add(folder);
        }

        return browser;
    }

    private IEnumerable<ContentBrowserFolder> GetContentBrowserFolders(string path)
    {
        return fileSystem.Directory.GetDirectories(path).Select(CreateContentBrowserFolder);
    }

    private ContentBrowserFolder CreateContentBrowserFolder(string directory)
    {
        return new ContentBrowserFolder(fileSystem) { Path = directory };
    }
}
