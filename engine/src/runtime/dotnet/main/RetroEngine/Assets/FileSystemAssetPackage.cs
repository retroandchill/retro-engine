// // @file FilesystemAssetPackage.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.IO.Abstractions;
using Cysharp.Text;
using RetroEngine.Portable.Strings;
using RetroEngine.Utilities.Async;
using RetroEngine.Utilities.Concurrency;

namespace RetroEngine.Assets;

public sealed class FileSystemAssetPackage : IEditableAssetPackage, IDisposable
{
    public Name PackageName { get; }

    public AssetPackageLoadState LoadState
    {
        get
        {
            using var scope = _loadTaskLock.EnterReadScope();
            return _loadTask switch
            {
                null => AssetPackageLoadState.Unloaded,
                { IsCompleted: true } => AssetPackageLoadState.Loaded,
                _ => AssetPackageLoadState.Loading,
            };
        }
    }

    public string SourcePath { get; }

    public bool IsReadOnly => false;
    public event Action? OnEntriesRefreshed;
    public event Action<IAssetPackageEntry>? OnEntryAdded;
    public event Action<IAssetPackageEntry>? OnEntryRemoved;
    public event Action<IAssetPackageEntry, IAssetPackageEntry>? OnEntryRenamed;

    private CancellationTokenSource? _loadCancellationSource;
    private Task? _loadTask;
    private readonly ReaderWriterLockSlim _loadTaskLock = new();

    private AssetPackageEntryList<FileSystemAssetEntry> _topLevelEntries =
        AssetPackageEntryList<FileSystemAssetEntry>.Empty;
    public IReadOnlyCollection<IAssetPackageEntry> TopLevelEntries
    {
        get
        {
            using var scope = _assetEntriesLock.EnterReadScope();
            return _topLevelEntries;
        }
    }

    private ImmutableDictionary<Name, FileSystemAssetEntry> _assetFileEntries = ImmutableDictionary<
        Name,
        FileSystemAssetEntry
    >.Empty;
    private readonly ReaderWriterLockSlim _assetEntriesLock = new();

    private readonly IFileSystemWatcher _watcher;
    private readonly IFileSystem _fileSystem;
    private readonly ImmutableArray<IAssetDecoder> _decoders;

    private readonly Lock _activeChangesLock = new();
    private FileSystemAssetChangeSet _activeChanges = new();
    private FileSystemAssetChangeSet _pendingChanges = new();
    private readonly SemaphoreSlim _processingGate = new(1, 1);
    private readonly Debouncer _debouncer = new();

    public FileSystemAssetPackage(
        IFileSystem fileSystem,
        Name packageName,
        string path,
        ImmutableArray<IAssetDecoder> decoders
    )
    {
        _fileSystem = fileSystem;
        _decoders = decoders;
        PackageName = packageName;
        SourcePath = path;

        _watcher = fileSystem.FileSystemWatcher.New(path, "*");
        _watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite;
        _watcher.IncludeSubdirectories = true;
        _watcher.EnableRaisingEvents = true;
    }

    public void Load()
    {
        using (_loadTaskLock.EnterReadScope())
        {
            if (_loadTask is not null)
                throw new InvalidOperationException("Asset package is already loading");
        }

        LoadInternal();
    }

    public async ValueTask LoadAsync(CancellationToken cancellationToken)
    {
        Task targetTask;
        using (_loadTaskLock.EnterWriteScope())
        {
            if (_loadTask is null)
            {
                _loadCancellationSource = new CancellationTokenSource();
                var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(
                    _loadCancellationSource.Token,
                    cancellationToken
                );
                targetTask = Task.Run(LoadInternal, linkedToken.Token);
                _loadTask = targetTask;
            }
            else
            {
                targetTask = _loadTask;
            }
        }

        await targetTask;
    }

    private void LoadInternal()
    {
        var rootDirectory = _fileSystem.DirectoryInfo.New(SourcePath);
        if (!rootDirectory.Exists)
            throw new AssetLoadException($"Asset package directory '{SourcePath}' does not exist");

        var assetFileEntries = ImmutableDictionary.CreateBuilder<Name, FileSystemAssetEntry>();
        var topLevelEntries = AssetPackageEntryList.CreateBuilder<FileSystemAssetEntry>();
        foreach (
            var entry in rootDirectory
                .EnumerateFileSystemInfos("*")
                .Select(x => ProcessAssetFileInfo(x, Name.None))
                .OfType<FileSystemAssetEntry>()
        )
        {
            topLevelEntries.Add(entry);
            foreach (var subEntry in entry.GetSelfAndChildrenBreadthFirst().OfType<FileSystemAssetEntry>())
            {
                assetFileEntries.Add(subEntry.Name, subEntry);
            }
        }

        using (_assetEntriesLock.EnterWriteScope())
        {
            _topLevelEntries = topLevelEntries.DrainToImmutable();
            _assetFileEntries = assetFileEntries.ToImmutable();
            _watcher.Created += OnFileEntryAdded;
            _watcher.Deleted += OnFileEntryRemoved;
            _watcher.Renamed += OnFileEntryRenamed;
        }

        using (_loadTaskLock.EnterWriteScope())
        {
            _loadTask = Task.CompletedTask;
        }
    }

    private ReadOnlySpan<char> GetRelativeAssetPath(ReadOnlySpan<char> path)
    {
        var relativePath = !SourcePath.EndsWith(_fileSystem.Path.DirectorySeparatorChar)
            ? path[(SourcePath.Length + 1)..]
            : path[SourcePath.Length..];

        return NormalizeAssetPath(relativePath);
    }

    private ReadOnlySpan<char> NormalizeAssetPath(ReadOnlySpan<char> path)
    {
        var directorySeparator = _fileSystem.Path.DirectorySeparatorChar;
        var alternateDirectorySeparator = _fileSystem.Path.DirectorySeparatorChar;

        var firstDirectorySeparator = path.IndexOf(directorySeparator);
        var lastDirectorySeparator = path.LastIndexOf(directorySeparator);
        if (
            directorySeparator == '/'
            || firstDirectorySeparator == -1 && (alternateDirectorySeparator == '/' || lastDirectorySeparator == -1)
        )
            return path;

        using var builder = new Utf16ValueStringBuilder(false);
        builder.Grow(path.Length);
        foreach (var c in path)
        {
            if (c == directorySeparator || c == alternateDirectorySeparator)
                builder.Append('/');
            else
                builder.Append(c);
        }
        return builder.ToString();
    }

    private FileSystemAssetEntry? ProcessAssetFileInfo(IFileSystemInfo file, Name parentName)
    {
        return file switch
        {
            IFileInfo fileInfo => ProcessAssetFile(fileInfo, parentName),
            IDirectoryInfo directoryInfo => ProcessAssetDirectoryInfoAsync(directoryInfo, parentName),
            _ => throw new ArgumentException("Unsupported file system info type", nameof(file)),
        };
    }

    private FileSystemAssetFolder ProcessAssetDirectoryInfoAsync(IDirectoryInfo directory, Name parentName)
    {
        var currentPath = GetRelativeAssetPath(directory.FullName);
        var pathName = new Name(currentPath);
        return new FileSystemAssetFolder(
            pathName,
            parentName,
            directory.Name,
            directory.FullName,
            directory
                .EnumerateFileSystemInfos("*")
                .Select(x => ProcessAssetFileInfo(x, pathName))
                .OfType<FileSystemAssetEntry>()
                .ToAssetPackageEntryList()
        );
    }

    private FileSystemAssetFile? ProcessAssetFile(IFileInfo file, Name parentName)
    {
        var assetName = new Name(GetRelativeAssetPath(file.FullName));
        var extension = file.Extension.AsSpan(1);
        FileSystemAssetFile? entry = null;
        foreach (var decoder in _decoders)
        {
            foreach (var ext in decoder.Extensions)
            {
                if (!ext.Equals(extension, StringComparison.OrdinalIgnoreCase))
                    continue;
                entry = new FileSystemAssetFile(assetName, parentName, file.Name, file.FullName, decoder.AssetType);
                break;
            }

            if (entry is not null)
                break;
        }

        return entry;
    }

    public void Unload()
    {
        using (_loadTaskLock.EnterWriteScope())
        {
            if (_loadCancellationSource is null)
                throw new InvalidOperationException("Asset package is not loaded");

            _loadCancellationSource.Cancel();
            _loadCancellationSource = null;
            _loadTask = null;
        }

        using (_assetEntriesLock.EnterWriteScope())
        {
            _topLevelEntries = AssetPackageEntryList<FileSystemAssetEntry>.Empty;
            _assetFileEntries = ImmutableDictionary<Name, FileSystemAssetEntry>.Empty;
            _watcher.Created -= OnFileEntryAdded;
            _watcher.Deleted -= OnFileEntryRemoved;
            _watcher.Renamed -= OnFileEntryRenamed;
        }
    }

    public bool HasAsset(Name assetName)
    {
        using var scope = _assetEntriesLock.EnterReadScope();
        if (_assetFileEntries is null)
            throw new InvalidOperationException("Asset package is not loaded");

        return _assetFileEntries.TryGetValue(assetName, out var entry) && entry is IAssetPackageFile;
    }

    public Name GetAssetType(Name assetName)
    {
        using var scope = _assetEntriesLock.EnterReadScope();
        var entry = _assetFileEntries.GetValueOrDefault(assetName);
        return entry is IAssetPackageFile file
            ? file.AssetType
            : throw new InvalidOperationException($"Asset '{assetName}' not found in package '{PackageName}'");
    }

    public Stream OpenAsset(Name assetName)
    {
        using var scope = _assetEntriesLock.EnterReadScope();
        if (_assetFileEntries.TryGetValue(assetName, out var entry) && entry is FileSystemAssetFile file)
        {
            if (_fileSystem.File.Exists(file.Path))
            {
                return _fileSystem.File.OpenRead(file.Path);
            }
        }

        throw new FileNotFoundException($"Asset '{assetName}' not found in package '{PackageName}'");
    }

    private void OnFileEntryAdded(object? sender, FileSystemEventArgs e)
    {
        var normalizedPath = GetRelativeAssetPath(e.FullPath);
        using (_assetEntriesLock.EnterWriteScope())
        {
            _pendingChanges.FileAdded(e.FullPath, normalizedPath);
        }
        RequestUpdate();
    }

    private void OnFileEntryRemoved(object? sender, FileSystemEventArgs e)
    {
        var normalizedPath = GetRelativeAssetPath(e.FullPath);
        using (_assetEntriesLock.EnterWriteScope())
        {
            _pendingChanges.FileDeleted(e.FullPath, normalizedPath);
        }
        RequestUpdate();
    }

    private void OnFileEntryRenamed(object? sender, RenamedEventArgs e)
    {
        var oldNormalizedPath = GetRelativeAssetPath(e.OldFullPath);
        var newNormalizedPath = GetRelativeAssetPath(e.FullPath);
        using (_assetEntriesLock.EnterWriteScope())
        {
            _pendingChanges.FileRenamed(e.FullPath, oldNormalizedPath, newNormalizedPath);
        }
        RequestUpdate();
    }

    private void RequestUpdate()
    {
        _debouncer.Debounce(
            () => _ = ApplyPendingChangesAsync(_loadCancellationSource?.Token ?? CancellationToken.None),
            TimeSpan.FromMilliseconds(100),
            _loadCancellationSource?.Token ?? CancellationToken.None
        );
    }

    private IFileSystemInfo CreateFileSystemInfo(string path)
    {
        IFileSystemInfo info;
        if (_fileSystem.File.Exists(path))
        {
            info = _fileSystem.FileInfo.New(path);
        }
        else if (_fileSystem.Directory.Exists(path))
        {
            info = _fileSystem.DirectoryInfo.New(path);
        }
        else
        {
            throw new FileNotFoundException($"Asset '{path}' not found in package '{PackageName}'");
        }

        return info;
    }

    private async Task ApplyPendingChangesAsync(CancellationToken cancellationToken = default)
    {
        await _processingGate.WaitAsync(cancellationToken);
        try
        {
            using (_activeChangesLock.EnterScope())
            {
                (_activeChanges, _pendingChanges) = (_pendingChanges, _activeChanges);
            }

            AssetPackageEntryList<FileSystemAssetEntry> topLevelEntries;
            ImmutableDictionary<Name, FileSystemAssetEntry> oldAssetFileEntries;
            ImmutableDictionary<Name, FileSystemAssetEntry>.Builder assetFileEntries;
            using (_assetEntriesLock.EnterReadScope())
            {
                topLevelEntries = _topLevelEntries;
                oldAssetFileEntries = _assetFileEntries;
                assetFileEntries = _assetFileEntries.ToBuilder();
            }

            var newTopLevelEntries = ApplyPendingChanges(Name.None, topLevelEntries, assetFileEntries);
            var newAssetFileEntries = assetFileEntries.ToImmutable();
            using (_assetEntriesLock.EnterWriteScope())
            {
                _topLevelEntries = newTopLevelEntries;
                _assetFileEntries = newAssetFileEntries;
            }

            foreach (var entry in _activeChanges.AllChanges)
            {
                switch (entry.Type)
                {
                    case ChangeType.Added:
                        OnEntryAdded?.Invoke(newAssetFileEntries[entry.Path]);
                        break;
                    case ChangeType.Deleted:
                        OnEntryRemoved?.Invoke(oldAssetFileEntries[entry.Path]);
                        break;
                    case ChangeType.Renamed:
                        OnEntryRenamed?.Invoke(oldAssetFileEntries[entry.OldPath], newAssetFileEntries[entry.Path]);
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported change type '{entry.Type}'");
                }
            }
            OnEntriesRefreshed?.Invoke();

            using (_activeChangesLock.EnterScope())
            {
                _activeChanges.Clear();
            }
        }
        finally
        {
            _processingGate.Release();
        }
    }

    private AssetPackageEntryList<FileSystemAssetEntry> ApplyPendingChanges(
        Name parentName,
        AssetPackageEntryList<FileSystemAssetEntry> currentList,
        ImmutableDictionary<Name, FileSystemAssetEntry>.Builder assetFileEntries
    )
    {
        if (!_activeChanges.FolderHasChanged(parentName))
            return currentList;

        AssetPackageEntryList<FileSystemAssetEntry>.Builder? builder = null;
        foreach (var child in currentList.OfType<FileSystemAssetFolder>())
        {
            var newChildren = ApplyPendingChanges(child.Name, child.Children, assetFileEntries);
            if (ReferenceEquals(newChildren, child.Children))
                continue;

            builder ??= currentList.ToBuilder();
            builder.AddOrReplace(child with { Children = newChildren });
        }

        var activeChanges = _activeChanges.GetChanges(parentName);
        foreach (var change in activeChanges)
        {
            builder ??= currentList.ToBuilder(activeChanges.Count);
            switch (change.Type)
            {
                case ChangeType.Added:
                    AddAssetAndChildren(change, assetFileEntries, builder);
                    break;
                case ChangeType.Deleted:
                    RemoveAssetAndChildren(assetFileEntries, change, builder);
                    break;
                case ChangeType.Renamed when change.OldParent == change.Parent:
                {
                    if (assetFileEntries.Remove(change.OldPath, out var entry))
                    {
                        var oldPath = entry.Key;
                        entry = ReplaceEntry(entry, change);
                        assetFileEntries[change.Path] = entry;
                        builder.Remove(oldPath);
                        builder.AddOrReplace(entry);
                    }

                    break;
                }
                case ChangeType.Renamed when parentName == change.Parent:
                    AddAssetAndChildren(change, assetFileEntries, builder);
                    break;
                case ChangeType.Renamed:
                {
                    if (parentName == change.OldParent)
                    {
                        RemoveAssetAndChildren(assetFileEntries, change, builder);
                    }

                    break;
                }
                default:
                    throw new InvalidOperationException($"Unsupported change type '{change.Type}'");
            }
        }

        return builder is not null ? builder.DrainToImmutable() : currentList;
    }

    private FileSystemAssetEntry ReplaceEntry(FileSystemAssetEntry entry, FileSystemChange change)
    {
        if (
            entry is FileSystemAssetFile file
            && !_fileSystem
                .Path.GetExtension(change.FullPath.AsSpan())
                .Equals(_fileSystem.Path.GetExtension(file.Path.AsSpan()), StringComparison.OrdinalIgnoreCase)
        )
        {
            return file with
            {
                Name = change.Path,
                DisplayName = _fileSystem.Path.GetFileName(change.FullPath),
                Path = change.FullPath,
                AssetType = GetAssetType(change.Path),
            };
        }

        return entry with
        {
            Name = change.Path,
            DisplayName = _fileSystem.Path.GetFileName(change.FullPath),
            Path = change.FullPath,
        };
    }

    private void AddAssetAndChildren(
        FileSystemChange change,
        ImmutableDictionary<Name, FileSystemAssetEntry>.Builder assetFileEntries,
        AssetPackageEntryList<FileSystemAssetEntry>.Builder builder
    )
    {
        var entry = ProcessAssetFileInfo(CreateFileSystemInfo(change.FullPath), change.Parent);
        if (entry is null)
            return;

        foreach (var c in entry.GetSelfAndChildrenBreadthFirst().Cast<FileSystemAssetEntry>())
        {
            assetFileEntries[c.Name] = c;
        }
        builder.Add(entry);
    }

    private static void RemoveAssetAndChildren(
        ImmutableDictionary<Name, FileSystemAssetEntry>.Builder assetFileEntries,
        FileSystemChange change,
        AssetPackageEntryList<FileSystemAssetEntry>.Builder builder
    )
    {
        var entry = assetFileEntries[change.Path];
        foreach (var c in entry.GetSelfAndChildrenBreadthFirst())
        {
            assetFileEntries.Remove(c.Name);
        }
        builder.Remove(entry);
    }

    public async ValueTask RenameAssetAsync(Name oldName, Name newName, CancellationToken cancellationToken = default)
    {
        var oldPath = _fileSystem.Path.Combine(SourcePath, oldName.ToString());
        var newPath = _fileSystem.Path.Combine(SourcePath, newName.ToString());

        var taskCompletionSource = new TaskCompletionSource();
        await using var cancellationTokenRegistration = cancellationToken.Register(() =>
            taskCompletionSource.TrySetCanceled()
        );

        try
        {
            OnEntryRenamed += OnRenameComplete;
            _fileSystem.File.Move(oldPath, newPath);
            await taskCompletionSource.Task;
        }
        finally
        {
            OnEntryRenamed -= OnRenameComplete;
        }
        return;

        void OnRenameComplete(IAssetPackageEntry oldEntry, IAssetPackageEntry newEntry)
        {
            if (oldEntry.Name == oldName && newEntry.Name == newName)
            {
                taskCompletionSource.TrySetResult();
            }
        }
    }

    public async ValueTask DeleteAssetAsync(Name name, CancellationToken cancellationToken = default)
    {
        var path = _fileSystem.Path.Combine(SourcePath, name.ToString());
        var taskCompletionSource = new TaskCompletionSource();
        await using var cancellationTokenRegistration = cancellationToken.Register(() =>
            taskCompletionSource.TrySetCanceled()
        );

        try
        {
            OnEntryRemoved += OnDeleteComplete;
            _fileSystem.File.Delete(path);
            await taskCompletionSource.Task;
        }
        finally
        {
            OnEntryRemoved -= OnDeleteComplete;
        }
        return;

        void OnDeleteComplete(IAssetPackageEntry entry)
        {
            if (entry.Name == name)
            {
                taskCompletionSource.TrySetResult();
            }
        }
    }

    public void Dispose()
    {
        _watcher.Dispose();
    }
}

[RegisterSingleton]
public sealed class FilesystemAssetPackageFactory(IFileSystem fileSystem, IEnumerable<IAssetDecoder> decoders)
    : IAssetPackageFactory
{
    private readonly ImmutableArray<IAssetDecoder> _decoders = [.. decoders.OrderBy(x => x.Priority)];

    public bool CanCreate(Name packageName, string path)
    {
        return fileSystem.Directory.Exists(path);
    }

    public IAssetPackage Create(Name packageName, string path)
    {
        return new FileSystemAssetPackage(fileSystem, packageName, path, _decoders);
    }
}
