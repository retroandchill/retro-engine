// // @file TextureDecoder.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers;
using System.Collections.Immutable;
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
public partial class TextureDecoder(RenderBackend renderBackend) : IAssetDecoder
{
    public Name AssetType => Texture.TypeName;

    private static readonly ImmutableArray<string> Extensions = ["png", "jpg", "jpeg", "bmp"];
    ImmutableArray<string> IAssetDecoder.Extensions => Extensions;

    static TextureDecoder()
    {
        Configuration.Default.PreferContiguousImageBuffers = true;
    }

    public Asset Decode(AssetStorageType type, AssetPath assetPath, scoped ReadOnlySpan<byte> source)
    {
        using var image = Image.Load<Rgba32>(source);

        image.DangerousTryGetSinglePixelMemory(out var pixelMemory);

        var buffer = MemoryMarshal.AsBytes(pixelMemory.Span);
        if (buffer.Length != image.Width * image.Height * 4)
            throw new AssetLoadException($"Texture has invalid dimensions");

        return CreateTextureFromBuffer(assetPath, buffer, image.Width, image.Height, TextureFormat.Rgba8);
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
        return new Texture(assetPath, nativeTexture);
    }
}
