// // @file TextureDecoder.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Runtime.InteropServices;
using RetroEngine.Portable.Strings;
using RetroEngine.Rendering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace RetroEngine.Assets;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public class TextureDecoder(RenderBackend renderBackend) : IAssetDecoder
{
    private static readonly Name TypeName = "Texture";
    public Name AssetType => TypeName;

    private static readonly ImmutableArray<string> Extensions = ["png", "jpg", "jpeg", "bmp"];
    ImmutableArray<string> IAssetDecoder.Extensions => Extensions;

    static TextureDecoder()
    {
        Configuration.Default.PreferContiguousImageBuffers = true;
    }

    public object Decode(AssetStorageType type, scoped ReadOnlySpan<byte> source)
    {
        using var image = Image.Load<Rgba32>(source);

        image.DangerousTryGetSinglePixelMemory(out var pixelMemory);

        var buffer = MemoryMarshal.AsBytes(pixelMemory.Span);
        if (buffer.Length != image.Width * image.Height * 4)
            throw new AssetLoadException($"Texture has invalid dimensions");

        return CreateTextureFromBuffer(buffer, image.Width, image.Height, TextureFormat.Rgba8);
    }

    private Texture CreateTextureFromBuffer(ReadOnlySpan<byte> buffer, int width, int height, TextureFormat format)
    {
        return renderBackend.UploadTexture(buffer, width, height, format);
    }
}
