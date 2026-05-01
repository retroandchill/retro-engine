// // @file FileSystemChangeSet.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Strings;
using RetroEngine.Utilities.Collections;

namespace RetroEngine.Assets;

internal enum ChangeType
{
    Added,
    Deleted,
    Renamed,
}

internal sealed record FileSystemChange(
    string FullPath,
    Name Path,
    Name Parent,
    ChangeType Type,
    Name OldPath = default,
    Name OldParent = default
);

internal sealed class FileSystemAssetChangeSet
{
    private readonly List<FileSystemChange> _allChanges = [];
    private readonly Dictionary<Name, List<FileSystemChange>> _pendingChanges = [];
    private readonly HashSet<Name> _changedDirectories = [];

    public IReadOnlyList<FileSystemChange> AllChanges => _allChanges;

    public bool FolderHasChanged(Name folder) => _changedDirectories.Contains(folder);

    public IReadOnlyList<FileSystemChange> GetChanges(Name folder)
    {
        return _pendingChanges.TryGetValue(folder, out var changes) ? changes : Array.Empty<FileSystemChange>();
    }

    public void FileAdded(string fullPath, ReadOnlySpan<char> relativePath)
    {
        var directoryName = AddParentDirectoryChanges(relativePath);

        var change = new FileSystemChange(fullPath, new Name(relativePath), directoryName, ChangeType.Added);
        _allChanges.Add(change);
        _pendingChanges.GetOrAdd(directoryName, _ => []).Add(change);
    }

    public void FileDeleted(string fullPath, ReadOnlySpan<char> relativePath)
    {
        var directoryName = AddParentDirectoryChanges(relativePath);

        var change = new FileSystemChange(fullPath, new Name(relativePath), directoryName, ChangeType.Deleted);
        _allChanges.Add(change);
        _pendingChanges.GetOrAdd(directoryName, _ => []).Add(change);
    }

    public void FileRenamed(string fullPath, ReadOnlySpan<char> oldPath, ReadOnlySpan<char> newPath)
    {
        var oldDirectoryName = AddParentDirectoryChanges(oldPath);
        var newDirectoryName = AddParentDirectoryChanges(newPath);

        var change = new FileSystemChange(
            fullPath,
            new Name(newPath),
            newDirectoryName,
            ChangeType.Renamed,
            new Name(oldPath),
            oldDirectoryName
        );
        _allChanges.Add(change);
        _pendingChanges.GetOrAdd(oldDirectoryName, _ => []).Add(change);
        if (oldDirectoryName != newDirectoryName)
        {
            _pendingChanges.GetOrAdd(newDirectoryName, _ => []).Add(change);
        }
    }

    private Name AddParentDirectoryChanges(ReadOnlySpan<char> directPath)
    {
        var directParent = Name.None;
        var i = 0;
        var currentName = directPath;
        do
        {
            currentName = GetParentDirectory(currentName);
            var asName = new Name(currentName);
            if (i == 0)
                directParent = asName;

            _changedDirectories.Add(asName);

            i++;
        } while (!currentName.IsEmpty);

        return directParent;
    }

    private static ReadOnlySpan<char> GetParentDirectory(ReadOnlySpan<char> path)
    {
        var index = path.LastIndexOf('/');
        if (index == -1)
            return "";

        return path[..index];
    }

    public void Clear()
    {
        // Keep the key changes so we don't end up reallocating lists after each change pass
        foreach (var (_, changes) in _pendingChanges)
        {
            changes.Clear();
        }

        _allChanges.Clear();
        _changedDirectories.Clear();
    }
}
