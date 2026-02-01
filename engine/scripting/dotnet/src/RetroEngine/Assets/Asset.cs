// // @file Asset.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using RetroEngine.Logging;
using RetroEngine.Strings;

namespace RetroEngine.Assets;

public abstract class Asset(IntPtr nativeObject)
{
    public IntPtr NativeObject { get; } = nativeObject;

    ~Asset()
    {
        AssetRegistry.NativeRelease(NativeObject);
    }
}
