// // @file FileSystemChangeSet.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using RetroEngine.Portable.Strings;
using RetroEngine.Utilities.Collections;

namespace RetroEngine.Assets;

internal sealed class FileSystemAssetChangeSet
{
    private readonly HashSet<Name> _changedDirectories = [];
    private readonly Dictionary<Name, Name> _knownRenames = new();

    public IReadOnlySet<Name> ChangedDirectories => _changedDirectories;
    public IReadOnlyDictionary<Name, Name> KnownRenames => _knownRenames;

    public void AddSingleFileChange(ReadOnlySpan<char> path, bool isDirectory)
    {
        if (!isDirectory)
        {
            path = GetDirectoryName(path);
        }
        _changedDirectories.Add(new Name(path));
    }

    public void AddRename(ReadOnlySpan<char> oldName, ReadOnlySpan<char> newName, bool isDirectory)
    {
        ReadOnlySpan<char> oldDirectory;
        ReadOnlySpan<char> newDirectory;
        if (!isDirectory)
        {
            oldDirectory = GetDirectoryName(oldName);
            newDirectory = GetDirectoryName(newName);
        }
        else
        {
            oldDirectory = oldName;
            newDirectory = newName;
        }

        if (!oldDirectory.Equals(newDirectory, StringComparison.OrdinalIgnoreCase))
        {
            _changedDirectories.Add(new Name(oldDirectory));
        }
        _changedDirectories.Add(new Name(newDirectory));

        _knownRenames[new Name(oldName)] = new Name(newName);
    }

    private static ReadOnlySpan<char> GetDirectoryName(ReadOnlySpan<char> path)
    {
        var lastIndex = path.LastIndexOf('/');
        return lastIndex != -1 ? path[..lastIndex] : Name.None;
    }

    public void Clear()
    {
        _changedDirectories.Clear();
        _knownRenames.Clear();
    }
}
