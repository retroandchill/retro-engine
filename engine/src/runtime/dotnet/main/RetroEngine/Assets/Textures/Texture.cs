// // @file Texture.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Strings;
using RetroEngine.Rendering;

namespace RetroEngine.Assets.Textures;

public sealed class Texture : Asset
{
    internal static readonly Name TypeName = new("Texture");

    internal NativeTexture NativeTexture { get; }
    public int Width => NativeTexture.Width;
    public int Height => NativeTexture.Height;
    public TextureFormat Format => NativeTexture.Format;
    public string? FilePath { get; }

    internal Texture(AssetPath path, NativeTexture nativeTexture, string? filePath)
        : base(path)
    {
        NativeTexture = nativeTexture;
        FilePath = filePath;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            NativeTexture.Dispose();
        }
    }
}
