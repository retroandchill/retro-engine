// // @file Texture.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Strings;

namespace RetroEngine.Assets.Textures;

public sealed class Texture : NativeAsset
{
    internal static readonly Name TypeName = new("Texture");

    public int Width { get; }
    public int Height { get; }

    internal Texture(AssetPath path, IntPtr handle, int width, int height)
        : base(path, handle)
    {
        Width = width;
        Height = height;
    }
}
