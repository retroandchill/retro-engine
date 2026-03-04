// // @file NativeAsset.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.InteropServices;
using RetroEngine.Interop;

namespace RetroEngine.Assets;

public abstract partial class NativeAsset(AssetPath path, IntPtr nativeObject) : Asset(path)
{
    public IntPtr NativeObject { get; } = nativeObject;

    protected override void Dispose(bool disposing)
    {
        NativeRelease(NativeObject);
    }

    [LibraryImport(NativeLibraries.RetroEngine, EntryPoint = "retro_release_asset")]
    private static partial void NativeRelease(IntPtr asset);
}
