// // @file TextureDecoder.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using RetroEngine.Interop;
using RetroEngine.Portable.Strings;
using Zomp.SyncMethodGenerator;

namespace RetroEngine.Assets.Textures;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public partial class TextureDecoder : IAssetDecoder
{
    public Name AssetType => Texture.TypeName;

    public bool TryLoadFromNativeCache(AssetDecodeContext context, [NotNullWhen(true)] out Asset? asset)
    {
        var nativeTexture = NativeLoad(context.Path, out var width, out var height);
        asset = nativeTexture != IntPtr.Zero ? new Texture(context.Path, nativeTexture, width, height) : null;
        return asset is not null;
    }

    [CreateSyncVersion]
    public async ValueTask<Asset> DecodeAsync(
        AssetDecodeContext context,
        Stream stream,
        CancellationToken cancellationToken = default
    )
    {
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);
        var bytes = memoryStream.ToArray();
        var nativeTexture = NativeCreate(
            context.Path,
            bytes,
            bytes.Length,
            out var width,
            out var height,
            out var error
        );
        return nativeTexture != IntPtr.Zero
            ? new Texture(context.Path, nativeTexture, width, height)
            : throw new InvalidOperationException(error);
    }

    [LibraryImport(
        NativeLibraries.RetroEngine,
        EntryPoint = "retro_texture_load_existing",
        StringMarshallingCustomType = typeof(UnownedCharMarshaller)
    )]
    private static partial IntPtr NativeLoad(in AssetPath path, out int width, out int height);

    [LibraryImport(
        NativeLibraries.RetroEngine,
        EntryPoint = "retro_texture_create",
        StringMarshallingCustomType = typeof(UnownedCharMarshaller)
    )]
    private static partial IntPtr NativeCreate(
        in AssetPath path,
        ReadOnlySpan<byte> bytes,
        int length,
        out int width,
        out int height,
        out string? error
    );
}
