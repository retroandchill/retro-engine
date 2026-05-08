// // @file SampleDataAssetEditorFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Assets;
using RetroEngine.Assets.Decoders;
using RetroEngine.Editor.Core.ViewModels;
using RetroEngine.Editor.Core.ViewModels.Tabs;
using RetroEngine.Portable.Strings;

namespace RetroEngine.Editor.Core.Services.Factories;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public class SampleDataAssetEditorFactory(AssetManager assetManager) : IAssetViewModelFactory
{
    public Name AssetType => SampleDataAsset.AssetType;

    public async ValueTask<IAssetViewModel> CreateViewModelAsync(
        AssetPath assetPath,
        CancellationToken cancellationToken = default
    )
    {
        var asset = await assetManager.LoadAssetAsync<SampleDataAsset>(assetPath, cancellationToken);
        if (asset is null)
            throw new InvalidOperationException($"Failed to load texture asset at {assetPath}.");

        var nameAsString = assetPath.AssetName.ToString();
        var lastDelimiter = nameAsString.LastIndexOf('/');
        return new SampleDataAssetEditorViewModel
        {
            Title = lastDelimiter >= 0 ? nameAsString[(lastDelimiter + 1)..] : nameAsString,
            Path = assetPath,
            Target = asset,
        };
    }
}
