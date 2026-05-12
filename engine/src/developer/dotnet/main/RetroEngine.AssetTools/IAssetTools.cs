// // @file IAssetTools.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Strings;

namespace RetroEngine.AssetTools;

public sealed record AdvancedAssetCategory(AssetTypeCategories Category, Name CategoryKey, Text CategoryName);

public interface IAssetTools
{
    void RegisterAssetTypeActions(IAssetTypeActions actions);

    void UnregisterAssetTypeActions(IAssetTypeActions actions);

    IReadOnlyList<IAssetTypeActions> AssetTypeActions { get; }

    AssetTypeCategories RegisterAdvancedAssetCategory(Name categoryKey, Text categoryDisplayName);

    AssetTypeCategories FindAdvancedAssetCategory(Name categoryKey);

    IEnumerable<AdvancedAssetCategory> AdvancedAssetCategories { get; }

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
            (IAssetFactory)assetType,
            cancellationToken
        );
    }

    Task<object> CreateAssetAsync(
        ReadOnlyMemory<char> assetName,
        ReadOnlyMemory<char> parentPath,
        Name assetPackage,
        IAssetFactory factory,
        CancellationToken cancellationToken = default
    );
}
