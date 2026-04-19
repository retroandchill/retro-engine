// // @file AssetLoadException.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Assets;

public sealed class AssetLoadException : Exception
{
    public AssetLoadException(string message)
        : base(message) { }

    public AssetLoadException(string message, Exception innerException)
        : base(message, innerException) { }
}
