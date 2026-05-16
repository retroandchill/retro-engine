// // @file SampleDataAssetEditorFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Assets;
using RetroEngine.Assets.Decoders;
using RetroEngine.AssetTools;
using RetroEngine.AssetTools.ViewModels;
using RetroEngine.Editor.Core.ViewModels.Tabs;

namespace RetroEngine.Editor.Core.Services;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public class SampleDataAssetActions(IAssetManager assetManager) : IAssetTypeActions
{
    public Type SupportedType => typeof(SampleDataAsset);

    public IAssetViewModel CreateViewModel(AssetPath assetPath, object asset)
    {
        if (asset is not SampleDataAsset sampleDataAsset)
            throw new InvalidOperationException($"Asset at {assetPath} is not a SampleDataAsset.");

        return new SampleDataAssetEditorViewModel { Path = assetPath, Target = sampleDataAsset };
    }
}
