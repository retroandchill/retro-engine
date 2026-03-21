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
        var contentFolder = fileSystem.Path.Combine(directoryName, "content");
        if (fileSystem.Directory.Exists(contentFolder))
        {
            browser.Folders.Add(new ContentBrowserFolder(fileSystem) { Path = contentFolder, CanEdit = false });
        }

        return browser;
    }
}
