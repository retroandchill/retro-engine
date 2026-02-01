// // @file Texture.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Strings;

namespace RetroEngine.Assets;

public sealed class Texture : Asset
{
    private static readonly Name TypeName = new("Texture");

    private Texture(IntPtr handle)
        : base(handle) { }

    internal static void RegisterAssetFactory()
    {
        AssetRegistry.RegisterAssetFactory(TypeName, ptr => new Texture(ptr));
    }
}
