// // @file TextureDecoder.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using MagicArchive;
using RetroEngine.Interop;
using RetroEngine.Portable.Strings;
using VYaml.Annotations;
using Zomp.SyncMethodGenerator;

namespace RetroEngine.Assets.Textures;

[Archivable]
[YamlObject(NamingConvention.SnakeCase)]
internal readonly partial record struct TextureData
{
    [ArchiveIgnore]
    public string? SourcePath { get; private init; }

    [YamlIgnore]
    public ReadOnlyMemory<byte> Data { get; init; }

    [YamlIgnore]
    public int Width { get; init; }

    [YamlIgnore]
    public int Height { get; init; }

    [YamlIgnore]
    public int Channels { get; init; }

    [ArchiveIgnore]
    [YamlIgnore]
    public bool IsLoaded => Data.Length > 0;
}

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public partial class TextureDecoder(IFileSystem fileSystem) : IAssetDecoder
{
    public Name AssetType => Texture.TypeName;

    public bool CanCreateFromExtension(ReadOnlySpan<char> extension)
    {
        return extension.Equals("png", StringComparison.OrdinalIgnoreCase);
    }

    public bool TryLoadFromNativeCache(AssetPath path, [NotNullWhen(true)] out Asset? asset)
    {
        var nativeTexture = NativeLoad(path, out var width, out var height);
        asset = nativeTexture != IntPtr.Zero ? new Texture(path, nativeTexture, width, height) : null;
        return asset is not null;
    }

    [CreateSyncVersion]
    public async ValueTask<Asset> DecodeAsync(
        IAssetPackage package,
        Name assetName,
        ReadOnlySequence<byte> bytes,
        CancellationToken cancellationToken = default
    )
    {
        var textureData = package.Deserialize<TextureData>(bytes);
        if (textureData.IsLoaded)
        {
            // TODO: We need to actually be able to load a texture like this
            throw new NotImplementedException();
        }

        if (string.IsNullOrWhiteSpace(textureData.SourcePath))
            throw new AssetLoadException($"Texture data for {assetName} has no source path specified");

        var textureDirectory = fileSystem.Path.GetDirectoryName(package.SourcePath);
        var absoluteTexturePath = fileSystem.Path.Combine(textureDirectory ?? ".", textureData.SourcePath);

        if (!fileSystem.File.Exists(absoluteTexturePath))
            throw new AssetLoadException($"Texture file {absoluteTexturePath} does not exist");

        await using var stream = fileSystem.File.OpenRead(absoluteTexturePath);
        var buffer = ArrayPool<byte>.Shared.Rent((int)stream.Length);
        try
        {
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(), cancellationToken).ConfigureAwait(false);
            var assetPath = new AssetPath(package.PackageName, assetName);
            var nativeTexture = NativeCreate(assetPath, buffer, bytesRead, out var width, out var height);
            return nativeTexture != IntPtr.Zero
                ? new Texture(assetPath, nativeTexture, width, height)
                : throw new InvalidOperationException("Failed to load texture from stream.");
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_texture_load_existing")]
    private static partial IntPtr NativeLoad(in AssetPath path, out int width, out int height);

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_texture_load")]
    private static partial IntPtr NativeCreate(
        in AssetPath path,
        ReadOnlySpan<byte> bytes,
        int length,
        out int width,
        out int height
    );
}
