// // @file FilesystemAssetPackage.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Immutable;
using System.IO.Abstractions;
using RetroEngine.Portable.Strings;
using RetroEngine.Utilities.Concurrency;
using VYaml.Serialization;

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
    public ImmutableArray<AssetPackageEntry> TopLevelEntries { get; private set; } = [];
    public event Action<AssetPackageEntry>? OnEntryAdded;
    public event Action<AssetPackageEntry>? OnEntryRemoved;
    public event Action<AssetPackageEntry, AssetPackageEntry>? OnEntryRenamed;

    private CancellationTokenSource? _loadCancellationSource;
    private Task? _loadTask;
    private readonly ReaderWriterLockSlim _loadTaskLock = new();

    private ImmutableDictionary<Name, AssetPackageEntry>? _assetFileEntries;
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
        var rootDirectory = _fileSystem.DirectoryInfo.New(SourcePath);
        if (!rootDirectory.Exists)
            throw new AssetLoadException($"Asset package directory '{SourcePath}' does not exist");

        var assetFileEntries = new Dictionary<Name, AssetPackageEntry>();
        var topLevelEntries = ImmutableArray.CreateBuilder<AssetPackageEntry>();
        foreach (var file in rootDirectory.EnumerateFileSystemInfos("*"))
        {
            ProcessAssetFileInfo(file, "", assetFileEntries, topLevelEntries);
        }

        using (_loadTaskLock.EnterWriteScope())
        {
            TopLevelEntries = topLevelEntries.DrainToImmutable();
            _assetFileEntries = assetFileEntries.ToImmutableDictionary();
            _loadTask = Task.CompletedTask;
        }
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
                targetTask = Task.Run(Load, linkedToken.Token);
                _loadTask = targetTask;
            }
            else
            {
                targetTask = _loadTask;
            }
        }

        await targetTask;
    }

    private void ProcessAssetFileInfo(
        IFileSystemInfo file,
        string parentPath,
        Dictionary<Name, AssetPackageEntry> assetFileEntries,
        ImmutableArray<AssetPackageEntry>.Builder builder
    )
    {
        switch (file)
        {
            case IFileInfo fileInfo:
                ProcessAssetFile(fileInfo, parentPath, assetFileEntries, builder);
                break;
            case IDirectoryInfo directoryInfo:
                ProcessAssetDirectoryInfoAsync(directoryInfo, parentPath, assetFileEntries, builder);
                break;
        }
    }

    private void ProcessAssetDirectoryInfoAsync(
        IDirectoryInfo directory,
        string parentPath,
        Dictionary<Name, AssetPackageEntry> assetFileEntries,
        ImmutableArray<AssetPackageEntry>.Builder builder
    )
    {
        var currentPath = !string.IsNullOrEmpty(parentPath) ? $"{parentPath}/{directory.Name}" : directory.Name;
        var subBuilder = ImmutableArray.CreateBuilder<AssetPackageEntry>();
        foreach (var file in directory.EnumerateFileSystemInfos("*"))
        {
            ProcessAssetFileInfo(file, currentPath, assetFileEntries, subBuilder);
        }

        builder.Add(new AssetPackageFolder(currentPath, subBuilder.DrainToImmutable()));
    }

    private void ProcessAssetFile(
        IFileInfo file,
        string parentPath,
        Dictionary<Name, AssetPackageEntry> assetFileEntries,
        ImmutableArray<AssetPackageEntry>.Builder builder
    )
    {
        var nameWithoutExtension = file.Name.AsSpan(0, file.Name.Length - file.Extension.Length);
        var assetName = new Name(
            !string.IsNullOrEmpty(parentPath) ? $"{parentPath}/{nameWithoutExtension}" : nameWithoutExtension
        );
        if (assetFileEntries.ContainsKey(assetName))
            return;

        var extension = file.Extension.AsSpan(1);
        AssetPackageFile? entry = null;
        foreach (var decoder in _decoders)
        {
            foreach (var ext in decoder.Extensions)
            {
                if (!ext.Equals(extension, StringComparison.OrdinalIgnoreCase))
                    continue;
                entry = new AssetPackageFile(assetName, decoder.AssetType, file.Extension);
                break;
            }

            if (entry is not null)
                break;
        }

        if (entry is null)
            return;

        assetFileEntries[assetName] = entry;
        builder.Add(entry);
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
    }

    public bool HasAsset(Name assetName)
    {
        using var scope = _loadTaskLock.EnterReadScope();
        if (_assetFileEntries is null)
            throw new InvalidOperationException("Asset package is not loaded");

        return _assetFileEntries.TryGetValue(assetName, out var entry) && entry is AssetPackageFile;
    }

    public Name GetAssetType(Name assetName)
    {
        using var scope = _loadTaskLock.EnterReadScope();
        if (_assetFileEntries is null)
            throw new InvalidOperationException("Asset package is not loaded");

        var entry = _assetFileEntries.GetValueOrDefault(assetName);
        return entry is AssetPackageFile file
            ? file.AssetType
            : throw new InvalidOperationException($"Asset '{assetName}' not found in package '{PackageName}'");
    }

    public Stream OpenAsset(Name assetName)
    {
        using var scope = _loadTaskLock.EnterReadScope();
        if (_assetFileEntries is null)
            throw new InvalidOperationException("Asset package is not loaded");

        if (_assetFileEntries.TryGetValue(assetName, out var entry) && entry is AssetPackageFile file)
        {
            var fullPath = file.GetFullPath(SourcePath);
            if (_fileSystem.File.Exists(fullPath))
            {
                return _fileSystem.File.OpenRead(fullPath);
            }
        }

        throw new FileNotFoundException($"Asset '{assetName}' not found in package '{PackageName}'");
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
