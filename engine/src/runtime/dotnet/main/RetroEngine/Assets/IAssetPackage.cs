// // @file IAssetPackage.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Strings;

namespace RetroEngine.Assets;

public interface IAssetPackage
{
    Name PackageName { get; }

    bool HasAsset(Name assetName);

    Name GetAssetType(Name assetName);

    ValueTask<Name> GetAssetTypeAsync(Name assetName, CancellationToken cancellationToken = default);

    Stream OpenAsset(Name assetName);
}
