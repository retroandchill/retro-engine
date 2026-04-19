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
using VYaml.Annotations;
using VYaml.Serialization;
using Zomp.SyncMethodGenerator;

namespace RetroEngine.Assets;

[YamlObject(NamingConvention.SnakeCase)]
public readonly partial record struct AssetFileHeader(Name AssetType);

public readonly record struct AssetFileEntry(Name AssetType, string FullPath, long DataOffset);

public sealed class FilesystemAssetPackage(
    IFileSystem fileSystem,
    Name packageName,
    string path,
    ImmutableArray<IAssetDecoder> decoders
) : IAssetPackage
{
    public Name PackageName { get; } = packageName;

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

    private CancellationTokenSource? _loadCancellationSource;
    private Task? _loadTask;
    private readonly ReaderWriterLockSlim _loadTaskLock = new();

    private ConcurrentDictionary<Name, AssetFileEntry>? _assetFileEntries;

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
        var rootDirectory = fileSystem.DirectoryInfo.New(path);
        if (!rootDirectory.Exists)
            throw new AssetLoadException($"Asset package directory '{path}' does not exist");

        var assetFileEntries = new Dictionary<Name, AssetFileEntry>();
        foreach (var file in rootDirectory.EnumerateFiles("*", SearchOption.AllDirectories))
        {
            var nameWithoutExtension = file.FullName.AsSpan(0, file.Name.Length - file.Extension.Length);
            var assetName = new Name(nameWithoutExtension);
            if (assetFileEntries.ContainsKey(assetName))
                continue;

            IFileInfo assetFileInfo;
            if (file.Extension == ".asset")
            {
                assetFileInfo = file;
            }
            else
            {
                var assetFileName = $"{nameWithoutExtension}.asset";
                assetFileInfo = fileSystem.FileInfo.New(assetFileName);
            }

            AssetFileEntry? entry = null;
            if (assetFileInfo.Exists)
            {
                await using var stream = file.OpenRead();
                entry = await ReadAssetFileEntryAsync(assetFileInfo.FullName, stream, cancellationToken);
            }
            else
            {
                foreach (var decoder in decoders.Where(decoder => decoder.CanCreateFromExtension(file.Extension)))
                {
                    entry = await WriteAssetFileEntryAsync(
                        assetFileInfo.FullName,
                        decoder,
                        file.OpenWrite(),
                        cancellationToken
                    );
                }
            }

            if (entry is not null)
                assetFileEntries[assetName] = entry.Value;
        }

        using (_loadTaskLock.EnterWriteScope())
        {
            _assetFileEntries = new ConcurrentDictionary<Name, AssetFileEntry>(assetFileEntries);
            _loadTask = Task.CompletedTask;
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

            builder.AppendLine(line);
        }

        var header = YamlSerializer.Deserialize<AssetFileHeader>(builder.AsMemory());

        return new AssetFileEntry(header.AssetType, fullPath, stream.Position);
    }

    private static async ValueTask<AssetFileEntry> WriteAssetFileEntryAsync(
        string fullPath,
        IAssetDecoder decoder,
        Stream stream,
        CancellationToken cancellationToken = default
    )
    {
        var fileHeader = new AssetFileHeader(decoder.AssetType);
        var bufferWriter = new ArrayBufferWriter<byte>();
        YamlSerializer.Serialize(bufferWriter, fileHeader);
        bufferWriter.Write("\n---\n"u8);
        await stream.WriteAsync(bufferWriter.WrittenMemory, cancellationToken);
        return new AssetFileEntry(decoder.AssetType, fullPath, stream.Position);
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

        if (_assetFileEntries.TryGetValue(assetName, out var entry) && fileSystem.File.Exists(entry.FullPath))
        {
            var stream = fileSystem.File.OpenRead(entry.FullPath);
            stream.Seek(entry.DataOffset, SeekOrigin.Begin);
            return stream;
        }

        throw new FileNotFoundException($"Asset '{assetName}' not found in package '{PackageName}'");
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
        return new FilesystemAssetPackage(fileSystem, packageName, path, _decoders);
    }
}
