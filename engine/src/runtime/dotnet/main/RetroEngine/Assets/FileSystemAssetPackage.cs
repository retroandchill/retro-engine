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
                .Select(file => ProcessAssetFileInfo(file, string.Empty))
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
        }

        using (_loadTaskLock.EnterWriteScope())
        {
            _loadTask = Task.CompletedTask;
        }
    }

    private FileSystemAssetEntry? ProcessAssetFileInfo(IFileSystemInfo file, ReadOnlySpan<char> parentPath)
    {
        return file switch
        {
            IFileInfo fileInfo => ProcessAssetFile(fileInfo, parentPath),
            IDirectoryInfo directoryInfo => ProcessAssetDirectoryInfoAsync(directoryInfo, parentPath),
            _ => throw new ArgumentException("Unsupported file system info type", nameof(file)),
        };
    }

    private FileSystemAssetFolder ProcessAssetDirectoryInfoAsync(
        IDirectoryInfo directory,
        ReadOnlySpan<char> parentPath
    )
    {
        var currentPath = !parentPath.IsEmpty ? $"{parentPath}/{directory.Name}" : directory.Name;
        return new FileSystemAssetFolder(
            currentPath,
            directory.Name,
            directory.FullName,
            directory
                .EnumerateFileSystemInfos("*")
                .Select(file => ProcessAssetFileInfo(file, currentPath))
                .OfType<FileSystemAssetEntry>()
                .ToAssetPackageEntryList()
        );
    }

    private FileSystemAssetFile? ProcessAssetFile(IFileInfo file, ReadOnlySpan<char> parentPath)
    {
        var assetName = new Name(!parentPath.IsEmpty ? $"{parentPath}/{file.Name}" : file.Name);
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

    private void AddEntry(string path)
    {
        var normalizedPath = _fileSystem.Path.GetFullPath(path);
        if (!normalizedPath.StartsWith(SourcePath, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Asset '{path}' is not in package '{PackageName}'");

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

        var trimLength = SourcePath.EndsWith(_fileSystem.Path.DirectorySeparatorChar)
            ? SourcePath.Length
            : SourcePath.Length + 1;
        var relativeRoute = normalizedPath.AsSpan(trimLength);

        using var scope = _assetEntriesLock.EnterWriteScope();
        var (topEntry, innerEntry) = AddEntry(info, "", relativeRoute);
        if (topEntry is null || innerEntry is null)
            return;
        var fullName = new Name(relativeRoute);
        _topLevelEntries = _topLevelEntries.AddOrReplace(topEntry);
        _assetFileEntries = _assetFileEntries.Add(fullName, innerEntry);
    }

    private (FileSystemAssetEntry? Top, FileSystemAssetEntry? Inner) AddEntry(
        IFileSystemInfo info,
        ReadOnlySpan<char> fullPath,
        ReadOnlySpan<char> path
    )
    {
        throw new NotImplementedException();
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
