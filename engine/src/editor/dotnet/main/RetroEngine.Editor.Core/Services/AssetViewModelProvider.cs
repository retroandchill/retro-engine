// // @file AssetViewProvider.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using RetroEngine.Assets;
using RetroEngine.Editor.Core.ViewModels;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Editor.Core.Services;

[RegisterSingleton]
public sealed class AssetViewModelProvider(AssetManager assetManager, IEnumerable<IAssetViewModelFactory> factories)
{
    private readonly ImmutableDictionary<Name, IAssetViewModelFactory> _factories = factories.ToImmutableDictionary(f =>
        f.AssetType
    );

    public async ValueTask<IAssetViewModel> GetViewModelAsync(
        AssetPath assetPath,
        CancellationToken cancellationToken = default
    )
    {
        var assetType = assetManager.GetAssetType(assetPath);
        if (assetType.IsNone)
            throw new InvalidOperationException($"Failed to determine asset type for {assetPath}.");

        if (!_factories.TryGetValue(assetType, out var factory))
            throw new InvalidOperationException($"No view model factory found for asset type '{assetType}'");

        return await factory.CreateViewModelAsync(assetPath, cancellationToken);
    }
}
