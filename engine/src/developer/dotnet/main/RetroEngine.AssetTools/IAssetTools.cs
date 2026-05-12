// // @file IAssetTools.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using RetroEngine.Assets;
using RetroEngine.Portable.Localization;
using RetroEngine.Portable.Strings;

namespace RetroEngine.AssetTools;

public sealed record AdvancedAssetCategory(AssetTypeCategories Category, Name CategoryKey, Text CategoryName);

public interface IAssetTools
{
    ImmutableArray<IAssetTypeActions> AssetTypeActions { get; }

    IAssetTypeActions? FindAssetTypeAction(Type type);

    AssetTypeCategories RegisterAdvancedAssetCategory(Name categoryKey, Text categoryDisplayName);

    AssetTypeCategories FindAdvancedAssetCategory(Name categoryKey);

    IEnumerable<AdvancedAssetCategory> AdvancedAssetCategories { get; }

    IEnumerable<IAssetFactory> Factories { get; }

    string? GetDefaultAssetExtension(Type assetType);

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

    ReadOnlySpan<char> GetAssetNameWithExtension(ReadOnlySpan<char> assetName, Type assetType);
}
