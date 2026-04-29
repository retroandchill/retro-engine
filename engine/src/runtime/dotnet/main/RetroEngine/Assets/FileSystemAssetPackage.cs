// // @file FilesystemAssetPackage.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.IO.Abstractions;
using RetroEngine.Portable.Strings;
using RetroEngine.Utilities.Concurrency;

namespace RetroEngine.Assets;

public sealed class FileSystemAssetPackage : IAssetPackage, IDisposable
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
                .Select(ProcessAssetFileInfo)
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
            _watcher.Created += AddEntry;
        }

        using (_loadTaskLock.EnterWriteScope())
        {
            _loadTask = Task.CompletedTask;
        }
    }

    private ReadOnlySpan<char> GetRelativeAssetPath(ReadOnlySpan<char> path)
    {
        return !SourcePath.EndsWith(_fileSystem.Path.DirectorySeparatorChar)
            ? path[(SourcePath.Length + 1)..]
            : path[SourcePath.Length..];
    }

    private FileSystemAssetEntry? ProcessAssetFileInfo(IFileSystemInfo file)
    {
        return file switch
        {
            IFileInfo fileInfo => ProcessAssetFile(fileInfo),
            IDirectoryInfo directoryInfo => ProcessAssetDirectoryInfoAsync(directoryInfo),
            _ => throw new ArgumentException("Unsupported file system info type", nameof(file)),
        };
    }

    private FileSystemAssetFolder ProcessAssetDirectoryInfoAsync(IDirectoryInfo directory)
    {
        var currentPath = GetRelativeAssetPath(directory.FullName);
        return new FileSystemAssetFolder(
            new Name(currentPath),
            directory.Name,
            directory.FullName,
            directory
                .EnumerateFileSystemInfos("*")
                .Select(ProcessAssetFileInfo)
                .OfType<FileSystemAssetEntry>()
                .ToAssetPackageEntryList()
        );
    }

    private FileSystemAssetFile? ProcessAssetFile(IFileInfo file)
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
                entry = new FileSystemAssetFile(assetName, file.Name, file.FullName, decoder.AssetType);
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
            _watcher.Created -= AddEntry;
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

    private void AddEntry(object? sender, FileSystemEventArgs e) => AddEntry(e.FullPath);

    private void AddEntry(string path)
    {
        var normalizedPath = _fileSystem.Path.GetFullPath(path);
        if (!normalizedPath.StartsWith(SourcePath, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Asset '{path}' is not in package '{PackageName}'");

        IFileSystemInfo info;
        bool isDirectory;
        if (_fileSystem.File.Exists(path))
        {
            info = _fileSystem.FileInfo.New(path);
            isDirectory = false;
        }
        else if (_fileSystem.Directory.Exists(path))
        {
            info = _fileSystem.DirectoryInfo.New(path);
            isDirectory = true;
        }
        else
        {
            throw new FileNotFoundException($"Asset '{path}' not found in package '{PackageName}'");
        }

        var trimLength = SourcePath.EndsWith(_fileSystem.Path.DirectorySeparatorChar)
            ? SourcePath.Length
            : SourcePath.Length + 1;
        var relativeRoute = normalizedPath.AsSpan(trimLength);
        var startCursor = new AssetPathCursor(relativeRoute, _fileSystem.Path.DirectorySeparatorChar);

        var newEntry = ProcessAssetFileInfo(info);
        if (newEntry is null)
            return;

        using (_assetEntriesLock.EnterWriteScope())
        {
            var builder = _assetFileEntries.ToBuilder();
            var topLevelEntry = InsertAssetEntry(newEntry, startCursor, builder);
            _topLevelEntries = _topLevelEntries.AddOrReplace(topLevelEntry);
            _assetFileEntries = builder.ToImmutable();
        }

        OnEntryAdded?.Invoke(newEntry);
    }

    private static FileSystemAssetEntry InsertAssetEntry(
        FileSystemAssetEntry entry,
        AssetPathCursor cursor,
        ImmutableDictionary<Name, FileSystemAssetEntry>.Builder builder
    )
    {
        if (!cursor.TryGetNextChild(out var nextCursor))
        {
            builder[entry.Name] = entry;
            return entry;
        }

        var parentKey = new Name(cursor.CurrentPath);
        var parent = builder.GetValueOrDefault(parentKey);
        if (parent is not FileSystemAssetFolder parentFolder)
            throw new InvalidOperationException($"Asset '{entry.Name}' is not a folder");

        var newEntry = parentFolder with
        {
            Children = parentFolder.Children.AddOrReplace(InsertAssetEntry(entry, nextCursor, builder)),
        };
        builder[parentKey] = newEntry;
        return newEntry;
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
