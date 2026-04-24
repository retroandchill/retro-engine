// // @file TextureDecoder.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.IO.Abstractions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MagicArchive;
using RetroEngine.Portable.Strings;
using RetroEngine.Rendering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using VYaml.Annotations;
using Zomp.SyncMethodGenerator;

namespace RetroEngine.Assets.Textures;

[Archivable]
[YamlObject(NamingConvention.SnakeCase)]
internal readonly partial record struct TextureData
{
    [ArchiveIgnore]
    public string? SourcePath { get; init; }

    [YamlIgnore]
    public ReadOnlyMemory<byte> Data { get; init; }

    [YamlIgnore]
    public int Width { get; init; }

    [YamlIgnore]
    public int Height { get; init; }

    [YamlIgnore]
    public TextureFormat Format { get; init; }

    [ArchiveIgnore]
    [YamlIgnore]
    public bool IsLoaded => Data.Length > 0;
}

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public partial class TextureDecoder(IFileSystem fileSystem, RenderBackend renderBackend) : IAssetDecoder
{
    public Name AssetType => Texture.TypeName;

    static TextureDecoder()
    {
        Configuration.Default.PreferContiguousImageBuffers = true;
    }

    public bool CanCreateFromExtension(ReadOnlySpan<char> extension)
    {
        return extension.Equals(".png", StringComparison.OrdinalIgnoreCase);
    }

    [CreateSyncVersion]
    public async ValueTask<Asset> DecodeAsync(
        IAssetPackage package,
        Name assetName,
        ReadOnlySequence<byte> bytes,
        CancellationToken cancellationToken = default
    )
    {
        var assetPath = new AssetPath(package.PackageName, assetName);
        var textureData = package.Deserialize<TextureData>(bytes);
        if (textureData.IsLoaded)
        {
            return CreateTextureFromBuffer(
                assetPath,
                textureData.Data.Span,
                textureData.Width,
                textureData.Width,
                textureData.Format
            );
        }

        if (string.IsNullOrWhiteSpace(textureData.SourcePath))
            throw new AssetLoadException($"Texture data for {assetName} has no source path specified");

        var textureDirectory = fileSystem.Path.GetDirectoryName(package.SourcePath);
        var absoluteTexturePath = fileSystem.Path.Combine(textureDirectory ?? ".", textureData.SourcePath);

        if (!fileSystem.File.Exists(absoluteTexturePath))
            throw new AssetLoadException($"Texture file {absoluteTexturePath} does not exist");

        var file = fileSystem.FileInfo.New(absoluteTexturePath);
        return await ImportFromFileAsync(package, assetName, file, cancellationToken);
    }

    public void Encode<TBufferWriter>(IAssetPackage package, Asset asset, in TBufferWriter writer)
        where TBufferWriter : IBufferWriter<byte>
    {
        var texture = (Texture)asset;
        var sizePerPixel = texture.Format switch
        {
            TextureFormat.Rgba8 => Unsafe.SizeOf<Rgba32>(),
            TextureFormat.Rgba16F => Unsafe.SizeOf<Rgba64>(),
            _ => throw new InvalidOperationException($"Unsupported texture format: {texture.Format}"),
        };
        var expectedSize = texture.Width * texture.Height * sizePerPixel;
        var buffer = ArrayPool<byte>.Shared.Rent(expectedSize);
        try
        {
            var bytesWritten = renderBackend.ExportTexture(texture.NativeTexture, buffer);
            var textureData = new TextureData
            {
                SourcePath = texture.FilePath,
                Data = buffer.AsMemory(0, bytesWritten),
                Width = texture.Width,
                Height = texture.Height,
                Format = texture.Format,
            };
            package.Serialize(in writer, textureData);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    [CreateSyncVersion]
    public async ValueTask<Asset> ImportFromFileAsync(
        IAssetPackage package,
        Name assetName,
        IFileInfo sourceFile,
        CancellationToken cancellationToken = default
    )
    {
        await using var stream = sourceFile.OpenRead();
        var assetPath = new AssetPath(package.PackageName, assetName);
        using var image = await Image.LoadAsync<Rgba32>(stream, cancellationToken).ConfigureAwait(false);

        image.DangerousTryGetSinglePixelMemory(out var pixelMemory);

        var buffer = MemoryMarshal.AsBytes(pixelMemory.Span);
        if (buffer.Length != image.Width * image.Height * 4)
            throw new AssetLoadException($"Texture has invalid dimensions");

        return CreateTextureFromBuffer(
            assetPath,
            buffer,
            image.Width,
            image.Height,
            TextureFormat.Rgba8,
            sourceFile.Name
        );
    }

    private Texture CreateTextureFromBuffer(
        AssetPath assetPath,
        ReadOnlySpan<byte> buffer,
        int width,
        int height,
        TextureFormat format,
        string? sourcePath = null
    )
    {
        var nativeTexture = renderBackend.UploadTexture(buffer, width, height, format);
        return new Texture(assetPath, nativeTexture, sourcePath);
    }
}
