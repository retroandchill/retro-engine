// // @file FilesystemAssetPackage.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO.Abstractions;
using Cysharp.IO;
using Cysharp.Text;
using RetroEngine.Portable.Strings;
using RetroEngine.Utilities.Concurrency;
using Serilog;
using VYaml.Annotations;
using VYaml.Serialization;

namespace RetroEngine.Assets;

[YamlObject(NamingConvention.SnakeCase)]
internal readonly partial record struct AssetFileHeader(Name AssetType);

internal readonly record struct AssetFileEntry(Name AssetType, string FullPath, long DataOffset);

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

    private ConcurrentDictionary<Name, AssetFileEntry>? _assetFileEntries;
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
                targetTask = Task.Run(() => LoadInternalAsync(linkedToken.Token), linkedToken.Token);
                _loadTask = targetTask;
            }
            else
            {
                targetTask = _loadTask;
            }
        }

        await targetTask;
    }

    private async Task LoadInternalAsync(CancellationToken cancellationToken)
    {
        var rootDirectory = _fileSystem.DirectoryInfo.New(SourcePath);
        if (!rootDirectory.Exists)
            throw new AssetLoadException($"Asset package directory '{SourcePath}' does not exist");

        var assetFileEntries = new Dictionary<Name, AssetFileEntry>();
        var topLevelEntries = ImmutableArray.CreateBuilder<AssetPackageEntry>();
        foreach (var file in rootDirectory.EnumerateFileSystemInfos("*"))
        {
            await ProcessAssetFileInfoAsync(file, assetFileEntries, topLevelEntries, cancellationToken);
        }

        using (_loadTaskLock.EnterWriteScope())
        {
            TopLevelEntries = topLevelEntries.DrainToImmutable();
            _assetFileEntries = new ConcurrentDictionary<Name, AssetFileEntry>(assetFileEntries);
            _loadTask = Task.CompletedTask;
        }
    }

    private async ValueTask ProcessAssetFileInfoAsync(
        IFileSystemInfo file,
        Dictionary<Name, AssetFileEntry> assetFileEntries,
        ImmutableArray<AssetPackageEntry>.Builder builder,
        CancellationToken cancellationToken
    )
    {
        switch (file)
        {
            case IFileInfo fileInfo:
                await ProcessAssetFileAsync(fileInfo, assetFileEntries, builder, cancellationToken);
                break;
            case IDirectoryInfo directoryInfo:
                await ProcessAssetDirectoryInfoAsync(directoryInfo, assetFileEntries, builder, cancellationToken);
                break;
        }
    }

    private async ValueTask ProcessAssetDirectoryInfoAsync(
        IDirectoryInfo directory,
        Dictionary<Name, AssetFileEntry> assetFileEntries,
        ImmutableArray<AssetPackageEntry>.Builder builder,
        CancellationToken cancellationToken
    )
    {
        var subBuilder = ImmutableArray.CreateBuilder<AssetPackageEntry>();
        foreach (var file in directory.EnumerateFileSystemInfos("*"))
        {
            await ProcessAssetFileInfoAsync(file, assetFileEntries, subBuilder, cancellationToken);
        }

        builder.Add(new AssetPackageFolder(directory.Name, subBuilder.DrainToImmutable()));
    }

    private async ValueTask ProcessAssetFileAsync(
        IFileInfo file,
        Dictionary<Name, AssetFileEntry> assetFileEntries,
        ImmutableArray<AssetPackageEntry>.Builder builder,
        CancellationToken cancellationToken
    )
    {
        var nameWithoutExtension = file.FullName.AsSpan(0, file.FullName.Length - file.Extension.Length);
        var assetName = new Name(nameWithoutExtension);
        if (assetFileEntries.ContainsKey(assetName))
            return;

        IFileInfo assetFileInfo;
        if (file.Extension == ".asset")
        {
            assetFileInfo = file;
        }
        else
        {
            var assetFileName = $"{nameWithoutExtension}.asset";
            assetFileInfo = _fileSystem.FileInfo.New(assetFileName);
        }

        AssetFileEntry? entry = null;
        if (assetFileInfo.Exists)
        {
            await using var stream = file.OpenRead();
            entry = await ReadAssetFileEntryAsync(assetFileInfo.FullName, stream, cancellationToken);
        }
        else
        {
            foreach (var decoder in _decoders.Where(decoder => decoder.CanCreateFromExtension(file.Extension)))
            {
                entry = await WriteAssetFileEntryAsync(
                    assetFileInfo.FullName,
                    decoder,
                    assetName,
                    file,
                    cancellationToken
                );
            }
        }

        if (entry is not null)
        {
            assetFileEntries[assetName] = entry.Value;
            builder.Add(new AssetPackageFile(assetName, entry.Value.AssetType));
        }
    }

    private static async ValueTask<AssetFileEntry> ReadAssetFileEntryAsync(
        string fullPath,
        Stream stream,
        CancellationToken cancellationToken = default
    )
    {
        await using var reader = new Utf8StreamReader(stream);

        using var builder = ZString.CreateUtf8StringBuilder();

        await foreach (var line in reader.ReadAllLinesAsync(cancellationToken))
        {
            if (line.Span.StartsWith("---"u8))
                break;

            builder.AppendLiteral(line.Span);
            builder.AppendLine();
        }

        var header = YamlSerializer.Deserialize<AssetFileHeader>(builder.AsMemory());

        return new AssetFileEntry(header.AssetType, fullPath, stream.Position);
    }

    private async ValueTask<AssetFileEntry> WriteAssetFileEntryAsync(
        string fullPath,
        IAssetDecoder decoder,
        Name assetName,
        IFileInfo sourceFile,
        CancellationToken cancellationToken = default
    )
    {
        var fileHeader = new AssetFileHeader(decoder.AssetType);
        var bufferWriter = new ArrayBufferWriter<byte>();
        YamlSerializer.Serialize(bufferWriter, fileHeader);
        bufferWriter.Write("---\n"u8);
        var offsetStart = bufferWriter.WrittenCount;

        using var asset = await decoder.ImportFromFileAsync(this, assetName, sourceFile, cancellationToken);
        decoder.Encode(this, asset, bufferWriter);

        await using var fileStream = _fileSystem.File.OpenWrite(fullPath);
        await fileStream.WriteAsync(bufferWriter.WrittenMemory, cancellationToken);
        return new AssetFileEntry(decoder.AssetType, fullPath, offsetStart);
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
        return _assetFileEntries?.ContainsKey(assetName)
            ?? throw new InvalidOperationException("Asset package is not loaded");
    }

    public Name GetAssetType(Name assetName)
    {
        using var scope = _loadTaskLock.EnterReadScope();
        return _assetFileEntries?.GetValueOrDefault(assetName).AssetType
            ?? throw new InvalidOperationException("Asset package is not loaded");
    }

    public Stream OpenAsset(Name assetName)
    {
        using var scope = _loadTaskLock.EnterReadScope();
        if (_assetFileEntries is null)
            throw new InvalidOperationException("Asset package is not loaded");

        if (_assetFileEntries.TryGetValue(assetName, out var entry) && _fileSystem.File.Exists(entry.FullPath))
        {
            var stream = _fileSystem.File.OpenRead(entry.FullPath);
            stream.Seek(entry.DataOffset, SeekOrigin.Begin);
            return stream;
        }

        throw new FileNotFoundException($"Asset '{assetName}' not found in package '{PackageName}'");
    }

    public void Serialize<T, TBufferWriter>(in TBufferWriter writer, T? data)
        where TBufferWriter : IBufferWriter<byte>
    {
        YamlSerializer.Serialize(writer, data);
    }

    public T Deserialize<T>(in ReadOnlySequence<byte> sequence)
    {
        return YamlSerializer.Deserialize<T>(sequence);
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
    private readonly ImmutableArray<IAssetDecoder> _decoders = [.. decoders];

    public bool CanCreate(Name packageName, string path)
    {
        return fileSystem.Directory.Exists(path);
    }

    public IAssetPackage Create(Name packageName, string path)
    {
        return new FileSystemAssetPackage(fileSystem, packageName, path, _decoders);
    }
}
