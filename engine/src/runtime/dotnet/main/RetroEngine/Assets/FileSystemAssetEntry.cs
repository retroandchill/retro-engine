// // @file FileSystemAssetEntry.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Assets;

internal abstract record FileSystemAssetEntry(Name Name, string Path) : IAssetPackageEntry
{
    public abstract bool IsDirectory { get; }
}

internal sealed record FileSystemAssetFolder(
    Name Name,
    string Path,
    AssetPackageEntryList<FileSystemAssetEntry> Children
) : FileSystemAssetEntry(Name, Path), IAssetPackageFolder
{
    public override bool IsDirectory => true;

    IReadOnlyCollection<IAssetPackageEntry> IAssetPackageFolder.Children => Children;
}

internal sealed record FileSystemAssetFile(Name Name, string Path, Name AssetType)
    : FileSystemAssetEntry(Name, Path),
        IAssetPackageFile
{
    public override bool IsDirectory => false;
}
