// // @file IAssetTools.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Strings;

namespace RetroEngine.AssetTools;

public readonly record struct AdvancedAssetCategory(AssetTypeCategory Category, Text CategoryName);

public interface IAssetTools
{
    AssetTypeCategory RegisterAdvancedAssetCategory(Name categoryKey, Text categoryDisplayName);

    AssetTypeCategory FindAdvancedAssetCategory(Name categoryKey);

    IEnumerable<AdvancedAssetCategory> AdvancedAssetCategories { get; }

    void AssociateAssetType(Type assetType, AssetTypeCategory category);

    Task<object> CreateAssetAsync(
        string assetName,
        string parentPath,
        Name assetPackage,
        Type assetType,
        CancellationToken cancellationToken = default
    )
    {
        return CreateAssetAsync(
            assetName.AsMemory(),
            parentPath.AsMemory(),
            assetPackage,
            assetType,
            cancellationToken
        );
    }

    Task<object> CreateAssetAsync(
        ReadOnlyMemory<char> assetName,
        ReadOnlyMemory<char> parentPath,
        Name assetPackage,
        Type assetType,
        CancellationToken cancellationToken = default
    );
}
