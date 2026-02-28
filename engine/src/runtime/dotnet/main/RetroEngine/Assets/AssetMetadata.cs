// // @file AssetMetadata.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace RetroEngine.Assets;

public abstract record AssetMetadata
{
    public abstract Type Type { get; }

    public required string FileExtension { get; init; }
}
