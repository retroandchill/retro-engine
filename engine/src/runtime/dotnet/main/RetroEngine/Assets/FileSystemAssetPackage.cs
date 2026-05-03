// // @file FilesystemAssetPackage.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.IO.Abstractions;
using System.Text;
using Cysharp.Text;
using RetroEngine.Portable.Strings;
using RetroEngine.Utilities.Async;
using RetroEngine.Utilities.Collections;
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
    public event AssetPackageChangeEvent? OnEntriesRefreshed;
    public event Action<Exception>? OnRefreshError;

    private CancellationTokenSource? _loadCancellationSource;
    private Task? _loadTask;
    private readonly ReaderWriterLockSlim _loadTaskLock = new();

    private AssetPackageEntryList<FileSystemAssetEntry> _topLevelEntries = [];

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
            IDirectoryInfo directoryInfo => ProcessAssetDirectoryInfo(directoryInfo, parentName),
            _ => throw new ArgumentException("Unsupported file system info type", nameof(file)),
        };
    }

    private FileSystemAssetFolder ProcessAssetDirectoryInfo(IDirectoryInfo directory, Name parentName)
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
                entry = new FileSystemAssetFile(
                    assetName,
                    parentName,
                    file.Name,
                    file.FullName,
                    file.LastWriteTimeUtc,
                    file.Length,
                    decoder.AssetType
                );
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

    public IAssetPackageEntry? GetEntry(Name entryName)
    {
        using var scope = _assetEntriesLock.EnterReadScope();
        return _assetFileEntries.GetValueOrDefault(entryName);
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

    public Task RefreshAllAsync(CancellationToken cancellationToken = default)
    {
        return RefreshAsync(Name.None, cancellationToken);
    }

    public async Task RefreshAsync(Name entryName, CancellationToken cancellationToken = default)
    {
        if (!_assetFileEntries.TryGetValue(entryName, out var entry) && !entryName.IsNone)
        {
            return;
        }
        var isDirectory = entry?.IsDirectory ?? false;

        var taskCompletionSource = new TaskCompletionSource();
        await using var cancellationTokenRegistration = cancellationToken.Register(() =>
            taskCompletionSource.TrySetCanceled()
        );

        try
        {
            OnEntriesRefreshed += OnComplete;
            OnRefreshError += OnRefreshFailed;
            using (_activeChangesLock.EnterScope())
            {
                _pendingChanges.AddSingleFileChange(entryName, isDirectory);
            }

            await taskCompletionSource.Task;
        }
        finally
        {
            OnEntriesRefreshed -= OnComplete;
            OnRefreshError -= OnRefreshFailed;
        }

        return;

        void OnComplete(scoped in AssetPackageChangeManifest manifest)
        {
            taskCompletionSource.TrySetResult();
        }

        void OnRefreshFailed(Exception ex)
        {
            taskCompletionSource.TrySetException(ex);
        }
    }

    public async Task AddFolderAsync(Name name, CancellationToken cancellationToken = default)
    {
        var path = _fileSystem.Path.Combine(SourcePath, name.ToString());
        var taskCompletionSource = new TaskCompletionSource();
        await using var cancellationTokenRegistration = cancellationToken.Register(() =>
            taskCompletionSource.TrySetCanceled()
        );

        try
        {
            OnEntriesRefreshed += OnAddComplete;
            OnRefreshError += OnRefreshFailed;
            _fileSystem.Directory.CreateDirectory(path);
            using (_activeChangesLock.EnterScope())
            {
                _pendingChanges.AddSingleFileChange(name, false);
            }

            await taskCompletionSource.Task;
        }
        finally
        {
            OnEntriesRefreshed -= OnAddComplete;
            OnRefreshError -= OnRefreshFailed;
        }

        return;

        void OnAddComplete(scoped in AssetPackageChangeManifest manifest)
        {
            foreach (var entry in manifest.RemovedEntries)
            {
                if (entry.Name == name)
                {
                    taskCompletionSource.TrySetResult();
                }
            }
        }

        void OnRefreshFailed(Exception ex)
        {
            taskCompletionSource.TrySetException(ex);
        }
    }

    public async Task RenameAssetAsync(Name oldName, Name newName, CancellationToken cancellationToken = default)
    {
        var oldPath = _fileSystem.Path.Combine(SourcePath, oldName.ToString());
        var newPath = _fileSystem.Path.Combine(SourcePath, newName.ToString());

        var taskCompletionSource = new TaskCompletionSource();
        await using var cancellationTokenRegistration = cancellationToken.Register(() =>
            taskCompletionSource.TrySetCanceled()
        );

        try
        {
            OnEntriesRefreshed += OnRenameComplete;
            OnRefreshError += OnRefreshFailed;
            _fileSystem.File.Move(oldPath, newPath);
            using (_activeChangesLock.EnterScope())
            {
                _pendingChanges.AddRename(oldName, newName, false);
            }

            await taskCompletionSource.Task;
        }
        finally
        {
            OnEntriesRefreshed -= OnRenameComplete;
            OnRefreshError -= OnRefreshFailed;
        }

        return;

        void OnRenameComplete(scoped in AssetPackageChangeManifest manifest)
        {
            foreach (var (oldEntry, newEntry) in manifest.RenamedEntries)
            {
                if (oldEntry.Name != oldName || newEntry.Name != newName)
                    continue;
                taskCompletionSource.TrySetResult();
                break;
            }
        }

        void OnRefreshFailed(Exception ex)
        {
            taskCompletionSource.TrySetException(ex);
        }
    }

    public async Task DeleteAssetAsync(Name name, CancellationToken cancellationToken = default)
    {
        var path = _fileSystem.Path.Combine(SourcePath, name.ToString());
        var taskCompletionSource = new TaskCompletionSource();
        await using var cancellationTokenRegistration = cancellationToken.Register(() =>
            taskCompletionSource.TrySetCanceled()
        );

        try
        {
            OnEntriesRefreshed += OnDeleteComplete;
            OnRefreshError += OnRefreshFailed;
            _fileSystem.File.Delete(path);
            using (_activeChangesLock.EnterScope())
            {
                _pendingChanges.AddSingleFileChange(name, false);
            }

            await taskCompletionSource.Task;
        }
        finally
        {
            OnEntriesRefreshed -= OnDeleteComplete;
            OnRefreshError -= OnRefreshFailed;
        }

        return;

        void OnDeleteComplete(scoped in AssetPackageChangeManifest manifest)
        {
            foreach (var entry in manifest.RemovedEntries)
            {
                if (entry.Name == name)
                {
                    taskCompletionSource.TrySetResult();
                }
            }
        }

        void OnRefreshFailed(Exception ex)
        {
            taskCompletionSource.TrySetException(ex);
        }
    }

    public void Dispose()
    {
        _watcher.Dispose();
    }

    private void OnFileEntryAdded(object? sender, FileSystemEventArgs e)
    {
        var normalizedPath = GetRelativeAssetPath(e.FullPath);
        var isDirectory = _fileSystem.Directory.Exists(e.FullPath);
        using (_assetEntriesLock.EnterWriteScope())
        {
            _pendingChanges.AddSingleFileChange(normalizedPath, isDirectory);
        }

        RequestUpdate();
    }

    private void OnFileEntryRemoved(object? sender, FileSystemEventArgs e)
    {
        var normalizedPath = GetRelativeAssetPath(e.FullPath);
        using (_assetEntriesLock.EnterWriteScope())
        {
            _pendingChanges.AddSingleFileChange(normalizedPath, false);
        }

        RequestUpdate();
    }

    private void OnFileEntryRenamed(object? sender, RenamedEventArgs e)
    {
        var oldNormalizedPath = GetRelativeAssetPath(e.OldFullPath);
        var newNormalizedPath = GetRelativeAssetPath(e.FullPath);
        var isDirectory = _fileSystem.Directory.Exists(e.FullPath);
        using (_assetEntriesLock.EnterWriteScope())
        {
            _pendingChanges.AddRename(oldNormalizedPath, newNormalizedPath, isDirectory);
        }

        RequestUpdate();
    }

    private void RequestUpdate()
    {
        const int debounceDelay = 100;
        _debouncer.Debounce(
            () => _ = ApplyPendingChangesAsync(_loadCancellationSource?.Token ?? CancellationToken.None),
            TimeSpan.FromMilliseconds(debounceDelay),
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
        using var scope = await _processingGate.EnterScopeAsync(cancellationToken);
        using (_activeChangesLock.EnterScope())
        {
            (_activeChanges, _pendingChanges) = (_pendingChanges, _activeChanges);
        }

        var normalizedScanRoots = NormalizeScanRoots(_activeChanges.ChangedDirectories);
        var normalizedRenames = NormalizedRenames(_activeChanges.KnownRenames);

        ImmutableDictionary<Name, FileSystemAssetEntry> assetFileEntries;
        ImmutableDictionary<Name, FileSystemAssetEntry>.Builder assetFileEntriesBuilder;
        AssetPackageEntryList<FileSystemAssetEntry> topLevelEntries;
        using (_assetEntriesLock.EnterReadScope())
        {
            assetFileEntries = _assetFileEntries;
            assetFileEntriesBuilder = _assetFileEntries.ToBuilder();
            topLevelEntries = _topLevelEntries;
        }

        try
        {
            topLevelEntries = ProcessFileSystemChanges(normalizedScanRoots, assetFileEntriesBuilder, topLevelEntries);
        }
        catch (Exception ex)
        {
            OnRefreshError?.Invoke(ex);
            return;
        }

        var maxFileLength = Math.Max(assetFileEntriesBuilder.Count, assetFileEntries.Count);
        using var addedEntries = new FixedList<IAssetPackageEntry>(maxFileLength);
        using var removedEntries = new FixedList<IAssetPackageEntry>(maxFileLength);
        using var renamedEntries = new FixedList<(IAssetPackageEntry Old, IAssetPackageEntry New)>(maxFileLength);
        using var modifiedEntries = new FixedList<(IAssetPackageEntry Old, IAssetPackageEntry New)>(maxFileLength);

        var handledNames = new HashSet<Name>();
        foreach (var (oldName, newName) in normalizedRenames)
        {
            var oldEntry = assetFileEntries.GetValueOrDefault(oldName);
            var newEntry = assetFileEntriesBuilder.GetValueOrDefault(newName);
            if (oldEntry is null || newEntry is null)
                continue;

            renamedEntries.Add((oldEntry, newEntry));
            handledNames.Add(oldName);
            handledNames.Add(newName);
        }

        foreach (var key in assetFileEntries.Keys.Concat(assetFileEntriesBuilder.Keys))
        {
            if (!handledNames.Add(key))
                continue;

            var oldEntry = assetFileEntries.GetValueOrDefault(key);
            var newEntry = assetFileEntriesBuilder.GetValueOrDefault(key);
            if (oldEntry is not null && newEntry is not null)
            {
                if (ReferenceEquals(oldEntry, newEntry))
                    continue;

                modifiedEntries.Add((oldEntry, newEntry));
            }
            else if (oldEntry is not null)
            {
                removedEntries.Add(oldEntry);
            }
            else if (newEntry is not null)
            {
                addedEntries.Add(newEntry);
            }
        }

        var delta = new AssetPackageChangeManifest
        {
            AddedEntries = addedEntries.AsSpan(),
            RemovedEntries = removedEntries.AsSpan(),
            ModifiedEntries = modifiedEntries.AsSpan(),
            RenamedEntries = renamedEntries.AsSpan(),
        };

        using (_assetEntriesLock.EnterWriteScope())
        {
            _assetFileEntries = assetFileEntriesBuilder.ToImmutable();
            _topLevelEntries = topLevelEntries;
        }

        OnEntriesRefreshed?.Invoke(delta);

        using (_activeChangesLock.EnterScope())
        {
            _activeChanges.Clear();
        }
    }

    private AssetPackageEntryList<FileSystemAssetEntry> ProcessFileSystemChanges(
        ImmutableArray<Name> normalizedScanRoots,
        ImmutableDictionary<Name, FileSystemAssetEntry>.Builder assetFileEntriesBuilder,
        AssetPackageEntryList<FileSystemAssetEntry> topLevelEntries
    )
    {
        var exploredParents = new HashSet<Name>();
        var parentsQueue = new Queue<Name>();
        var childrenByParent = new Dictionary<Name, HashSet<Name>>();
        foreach (var root in normalizedScanRoots)
        {
            if (root.IsNone)
            {
                var directoryInfo = _fileSystem.DirectoryInfo.New(SourcePath);
                topLevelEntries = GetChildEntries(Name.None, directoryInfo, topLevelEntries, assetFileEntriesBuilder);
                break;
            }

            FileSystemAssetEntry entry;
            if (assetFileEntriesBuilder.TryGetValue(root, out var rootEntry))
            {
                entry = RefreshEntry(rootEntry, assetFileEntriesBuilder);
                if (ReferenceEquals(entry, rootEntry))
                    continue;
            }
            else
            {
                var rootDirectory = _fileSystem.Path.Combine(SourcePath, root.ToString());
                var directoryInfo = _fileSystem.DirectoryInfo.New(rootDirectory);
                entry = ProcessAssetDirectoryInfo(directoryInfo, Name.None);
                foreach (var child in entry.GetSelfAndChildrenBreadthFirst().Cast<FileSystemAssetEntry>())
                {
                    assetFileEntriesBuilder[child.Name] = child;
                }
            }

            assetFileEntriesBuilder[root] = entry;
            var currentEntry = entry;
            if (exploredParents.Add(currentEntry.ParentName))
                parentsQueue.Enqueue(currentEntry.ParentName);
            while (currentEntry.ParentName != Name.None)
            {
                childrenByParent.GetOrAdd(currentEntry.ParentName, _ => []).Add(currentEntry.Name);
                currentEntry = assetFileEntriesBuilder[currentEntry.ParentName];
            }

            childrenByParent.GetOrAdd(Name.None, _ => []).Add(currentEntry.Name);
        }

        while (parentsQueue.TryDequeue(out var parentName))
        {
            var entry = !parentName.IsNone ? (FileSystemAssetFolder)assetFileEntriesBuilder[parentName] : null;
            var children = childrenByParent[parentName];
            var childrenList = entry?.Children ?? topLevelEntries;
            if (children.Count == 1)
            {
                childrenList = childrenList.AddOrReplace(assetFileEntriesBuilder[children.First()]);
            }
            else
            {
                var builder = childrenList.ToBuilder();
                foreach (var childName in children)
                {
                    builder.AddOrReplace(assetFileEntriesBuilder[childName]);
                }

                childrenList = builder.ToImmutable();
            }

            if (entry is not null)
            {
                assetFileEntriesBuilder[parentName] = entry with { Children = childrenList };
                if (exploredParents.Add(entry.ParentName))
                    parentsQueue.Enqueue(entry.ParentName);
            }
            else
            {
                topLevelEntries = childrenList;
            }
        }

        return topLevelEntries;
    }

    private FileSystemAssetEntry RefreshEntry(
        FileSystemAssetEntry entry,
        ImmutableDictionary<Name, FileSystemAssetEntry>.Builder assetFileEntriesBuilder
    )
    {
        var info = CreateFileSystemInfo(entry.Path);
        switch (entry)
        {
            case FileSystemAssetFile file:
            {
                if (info is not IFileInfo fileInfo)
                    return ProcessAssetFileInfo(info, entry.ParentName)
                        ?? throw new InvalidOperationException("Invalid asset file info");

                if (file.LastModified >= info.LastWriteTimeUtc && fileInfo.Length == file.Length)
                    return file;

                var result = file with { LastModified = info.LastWriteTimeUtc, Length = fileInfo.Length };
                assetFileEntriesBuilder[entry.Name] = result;
                return result;
            }
            case FileSystemAssetFolder folder:
            {
                if (info is not IDirectoryInfo directoryInfo)
                    return ProcessAssetFileInfo(info, entry.ParentName)
                        ?? throw new InvalidOperationException("Invalid asset file info");

                var children = GetChildEntries(entry.Name, directoryInfo, folder.Children, assetFileEntriesBuilder);

                if (ReferenceEquals(folder.Children, children))
                    return folder;

                var result = folder with { Children = children };
                assetFileEntriesBuilder[entry.Name] = result;
                return result;
            }
            default:
                throw new InvalidOperationException("Invalid asset file info");
        }
    }

    private AssetPackageEntryList<FileSystemAssetEntry> GetChildEntries(
        Name entry,
        IDirectoryInfo directoryInfo,
        AssetPackageEntryList<FileSystemAssetEntry> children,
        ImmutableDictionary<Name, FileSystemAssetEntry>.Builder assetFileEntriesBuilder
    )
    {
        AssetPackageEntryList<FileSystemAssetEntry>.Builder? builder = null;
        var foundChildren = new HashSet<AssetPackageEntryKey>();
        foreach (var childInfo in directoryInfo.EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly))
        {
            var normalizedName = GetRelativeAssetPath(childInfo.FullName);
            var isDirectory = childInfo is IDirectoryInfo;
            var assetKey = new AssetPackageEntryKey(new Name(normalizedName), isDirectory);
            foundChildren.Add(assetKey);
            FileSystemAssetEntry updatedEntry;
            if (children.GetOrDefault(assetKey) is { } existingEntry)
            {
                updatedEntry = RefreshEntry(existingEntry, assetFileEntriesBuilder);
                if (ReferenceEquals(updatedEntry, existingEntry))
                    continue;
            }
            else
            {
                updatedEntry =
                    ProcessAssetFileInfo(childInfo, entry)
                    ?? throw new InvalidOperationException("Invalid asset file info");
            }

            assetFileEntriesBuilder[updatedEntry.Name] = updatedEntry;
            builder ??= children.ToBuilder();
            builder.AddOrReplace(updatedEntry);
        }

        foreach (var child in children)
        {
            if (foundChildren.Contains(child.Key))
                continue;

            assetFileEntriesBuilder.Remove(child.Name);
        }

        if (builder is not null)
        {
            builder.Intersect(foundChildren);
            children = builder.DrainToImmutable();
        }
        else
        {
            children = children.Intersect(foundChildren);
        }

        return children;
    }

    private static ImmutableArray<Name> NormalizeScanRoots(IReadOnlySet<Name> scanRoots)
    {
        if (scanRoots.Contains(Name.None))
            return [Name.None];

        var normalized = ImmutableArray.CreateBuilder<Name>(scanRoots.Count);

        var maxNameLength = Encoding.UTF8.GetMaxCharCount(Name.MaxRenderedLength);
        Span<char> rootBuffer = stackalloc char[maxNameLength];
        Span<char> ancestorBuffer = stackalloc char[maxNameLength];
        foreach (var root in scanRoots.OrderBy(x => x, NameComparer.CaseInsensitive))
        {
            if (normalized.Count > 0)
            {
                var rootLength = root.WriteUtf16Bytes(rootBuffer);
                var rootSpan = rootBuffer[..rootLength];
                var ancestorFound = false;
                foreach (var normalizedRoot in normalized)
                {
                    var ancestorLength = normalizedRoot.WriteUtf16Bytes(ancestorBuffer);
                    var ancestorSpan = ancestorBuffer[..ancestorLength];
                    if (!IsAncestor(ancestorSpan, rootSpan))
                        continue;

                    ancestorFound = true;
                    break;
                }

                if (ancestorFound)
                    continue;
            }

            normalized.Add(root);
        }

        return normalized.DrainToImmutable();

        static bool IsAncestor(ReadOnlySpan<char> root, ReadOnlySpan<char> ancestor)
        {
            if (root.Length < ancestor.Length)
                return false;

            var targetSpan = root;
            while (!targetSpan.IsEmpty)
            {
                if (targetSpan.Equals(ancestor, StringComparison.OrdinalIgnoreCase))
                    return true;

                var nextSeparator = targetSpan.LastIndexOf('/');
                targetSpan = nextSeparator != -1 ? targetSpan[..nextSeparator] : [];
            }

            return false;
        }
    }

    private static ImmutableDictionary<Name, Name> NormalizedRenames(IReadOnlyDictionary<Name, Name> renames)
    {
        var builder = ImmutableDictionary.CreateBuilder<Name, Name>();

        foreach (var (source, d) in renames)
        {
            var destination = d;

            while (renames.TryGetValue(destination, out var nextDestination))
            {
                destination = nextDestination;

                if (destination == source)
                    break;
            }

            if (source != destination)
                builder[source] = destination;
        }

        return builder.ToImmutable();
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
