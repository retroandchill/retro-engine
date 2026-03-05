// // @file FilesystemAssetPackage.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO.Abstractions;
using RetroEngine.Assets.Textures;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Assets;

public sealed class FilesystemAssetPackage(IFileSystem fileSystem, string rootPath, Name packageName) : IAssetPackage
{
    public Name PackageName { get; } = packageName;
    private readonly ConcurrentDictionary<Name, Name> _assetFileTypes = new();
    private static readonly ImmutableDictionary<string, Name> ExtensionToAssetType;

    static FilesystemAssetPackage()
    {
        var builder = ImmutableDictionary.CreateBuilder<string, Name>();
        builder.Add(".png", Texture.TypeName);
        ExtensionToAssetType = builder.ToImmutable();
    }

    public bool HasAsset(Name assetName)
    {
        return fileSystem.File.Exists(Path.Combine(rootPath, assetName));
    }

    public Name GetAssetType(Name assetName)
    {
        // TODO: Eventually we want to actually load a metadata file from the stream,
        // but sniffing the file extension is fine for now
        return _assetFileTypes.GetOrAdd(
            assetName,
            name =>
            {
                var fullPath = Path.Combine(rootPath, name);

                if (!ExtensionToAssetType.TryGetValue(Path.GetExtension(fullPath), out var assetType))
                {
                    throw new InvalidOperationException($"No asset type found for asset '{name}'");
                }

                _assetFileTypes[name] = assetType;
                return assetType;
            }
        );
    }

    public ValueTask<Name> GetAssetTypeAsync(Name assetName, CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(GetAssetType(assetName));
    }

    public Stream OpenAsset(Name assetName)
    {
        var fullPath = Path.Combine(rootPath, assetName);
        return fileSystem.File.OpenRead(fullPath);
    }
}
