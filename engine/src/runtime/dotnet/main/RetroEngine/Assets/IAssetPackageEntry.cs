// // @file IAssetPackageEntry.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Strings;

namespace RetroEngine.Assets;

public interface IAssetPackageEntry
{
    Name Name { get; }

    Name ParentName { get; }

    public string DisplayName { get; }

    bool IsDirectory { get; }
}

public readonly record struct AssetPackageEntryKey(Name Name, bool IsDirectory)
    : IComparable<IAssetPackageEntry>,
        IComparable<AssetPackageEntryKey>
{
    public int CompareTo(IAssetPackageEntry? other)
    {
        if (other is null)
            return 1;
        if (IsDirectory != other.IsDirectory)
            return IsDirectory ? -1 : 1;

        return Name.CompareLexical(other.Name, NameCase.IgnoreCase);
    }

    public int CompareTo(AssetPackageEntryKey other)
    {
        if (IsDirectory != other.IsDirectory)
            return IsDirectory ? -1 : 1;

        return Name.CompareLexical(other.Name, NameCase.IgnoreCase);
    }
}

public interface IAssetPackageFolder : IAssetPackageEntry
{
    IReadOnlyCollection<IAssetPackageEntry> Children { get; }
}

public interface IAssetPackageFile : IAssetPackageEntry
{
    Name AssetType { get; }
}

internal sealed class AssetPackageEntryComparer : IComparer<IAssetPackageEntry>
{
    public static readonly AssetPackageEntryComparer Default = new();

    public int Compare(IAssetPackageEntry? x, IAssetPackageEntry? y)
    {
        if (ReferenceEquals(x, y))
            return 0;
        if (y is null)
            return 1;
        if (x is null)
            return -1;

        if (x.IsDirectory != y.IsDirectory)
            return x.IsDirectory ? -1 : 1;

        return x.Name.CompareLexical(y.Name, NameCase.IgnoreCase);
    }
}

public static class AssetPackageEntryExtensions
{
    extension(IAssetPackageEntry entry)
    {
        public AssetPackageEntryKey Key => new(entry.Name, entry.IsDirectory);
    }
}
