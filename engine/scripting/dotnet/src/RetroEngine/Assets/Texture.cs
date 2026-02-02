// // @file Texture.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Core.Math;
using RetroEngine.Strings;

namespace RetroEngine.Assets;

public sealed partial class Texture : Asset
{
    private static readonly Name TypeName = new("Texture");

    public int Width { get; }
    public int Height { get; }

    private Texture(IntPtr handle)
        : base(handle)
    {
        (Width, Height) = NativeGetSize(handle);
    }

    internal static void RegisterAssetFactory()
    {
        AssetRegistry.RegisterAssetFactory(TypeName, ptr => new Texture(ptr));
    }

    [LibraryImport("retro_runtime", EntryPoint = "retro_texture_get_size")]
    private static partial Vector2I NativeGetSize(IntPtr texture);
}
