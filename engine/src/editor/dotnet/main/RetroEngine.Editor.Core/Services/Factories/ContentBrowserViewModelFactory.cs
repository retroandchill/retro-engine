// // @file ContentBrowserViewModelFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Abstractions;
using IconPacks.Avalonia.Codicons;
using RetroEngine.Assets;
using RetroEngine.Editor.Core.ViewModels.Tabs;

namespace RetroEngine.Editor.Core.Services.Factories;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public sealed class ContentBrowserViewModelFactory(IFileSystem fileSystem, AssetManager assetManager)
    : ViewModelFactory<ContentBrowserViewModel>
{
    public override ContentBrowserViewModel CreateViewModel()
    {
        return new ContentBrowserViewModel
        {
            FileSystem = fileSystem,
            Items = [.. assetManager.LoadedPackages.Select(CreateContentFolder)],
        };
    }

    private static ContentBrowserItem CreateContentFolder(IAssetPackage package)
    {
        return new ContentBrowserItem
        {
            Name = package.PackageName,
            Icon = PackIconCodiconsKind.Package,
            CanEdit = false,
            Children = [.. package.TopLevelEntries.Select(CreateContentFolder)],
        };
    }

    private static ContentBrowserItem CreateContentFolder(IAssetPackageEntry entry)
    {
        var children = entry is IAssetPackageFolder folder ? folder.Children.Select(CreateContentFolder) : [];
        return new ContentBrowserItem
        {
            Name = entry.DisplayName,
            Icon = entry.IsDirectory ? PackIconCodiconsKind.Folder : PackIconCodiconsKind.File,
            Children = [.. children],
        };
    }
}
