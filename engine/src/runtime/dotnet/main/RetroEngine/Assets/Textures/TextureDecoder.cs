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

    public bool CanCreateFromExtension(ReadOnlySpan<char> extension)
    {
        return extension.Equals("png", StringComparison.OrdinalIgnoreCase);
    }

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
        await stream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
        var bytes = memoryStream.ToArray();
        var nativeTexture = NativeCreate(context.Path, bytes, bytes.Length, out var width, out var height);
        return nativeTexture != IntPtr.Zero
            ? new Texture(context.Path, nativeTexture, width, height)
            : throw new InvalidOperationException("Failed to load texture from stream.");
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
