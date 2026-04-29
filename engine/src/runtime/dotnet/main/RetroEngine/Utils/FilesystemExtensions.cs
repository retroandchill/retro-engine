// // @file FilesystemExtensions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO.Abstractions;
using Testably.Abstractions;

namespace RetroEngine.Utils;

public static class FilesystemExtensions
{
    private static readonly RealFileSystem DefaultFileSystem = new();

    extension(IFileSystem)
    {
        public static IFileSystem Default => DefaultFileSystem;
    }
}
