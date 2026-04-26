// // @file NativeLibraryLoader.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RetroEngine.Interop;

namespace RetroEngine.Portable;

internal static class NativeLibraryLoader
{
#pragma warning disable CA2255
    [ModuleInitializer]
#pragma warning restore CA2255
    public static void Initialize()
    {
        LibraryLoader.RegisterRetroInteropLoader(typeof(NativeLibraryLoader).Assembly);
    }
}
