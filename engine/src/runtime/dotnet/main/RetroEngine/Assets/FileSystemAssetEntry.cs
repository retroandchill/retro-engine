// // @file FileSystemAssetEntry.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Assets;

internal abstract record FileSystemAssetEntry(Name Name, Name ParentName, string DisplayName, string Path)
    : IAssetPackageEntry
{
    public abstract bool IsDirectory { get; }
}

internal sealed record FileSystemAssetFolder(
    Name Name,
    Name ParentName,
    string DisplayName,
    string Path,
    AssetPackageEntryList<FileSystemAssetEntry> Children
) : FileSystemAssetEntry(Name, ParentName, DisplayName, Path), IAssetPackageFolder
{
    public override bool IsDirectory => true;

    IReadOnlyCollection<IAssetPackageEntry> IAssetPackageFolder.Children => Children;
}

internal sealed record FileSystemAssetFile(Name Name, Name ParentName, string DisplayName, string Path, Name AssetType)
    : FileSystemAssetEntry(Name, ParentName, DisplayName, Path),
        IAssetPackageFile
{
    public override bool IsDirectory => false;
}
