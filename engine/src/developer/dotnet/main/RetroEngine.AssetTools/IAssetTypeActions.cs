// // @file IAssetTypeActions.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Assets;
using RetroEngine.AssetTools.ViewModels;

namespace RetroEngine.AssetTools;

public interface IAssetTypeActions
{
    Type SupportedType { get; }

    IAssetViewModel CreateViewModel(AssetPath assetPath, object asset)
    {
        return new GenericAssetViewModel(assetPath, asset);
    }
}
