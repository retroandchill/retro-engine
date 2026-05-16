// @file StringTableAssetActions.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Injectio.Attributes;
using RetroEngine.Assets;
using RetroEngine.AssetTools.ViewModels;
using RetroEngine.Portable.Localization.StringTables;

namespace RetroEngine.AssetTools.Actions;

[RegisterSingleton(Duplicate = DuplicateStrategy.Append)]
public class StringTableAssetActions : IAssetTypeActions
{
    public Type SupportedType => typeof(StringTable);

    public IAssetViewModel CreateViewModel(AssetPath assetPath, object asset)
    {
        if (asset is not StringTable stringTable)
            throw new ArgumentException("Asset must be of type StringTable", nameof(asset));

        return new StringTableEditorViewModel { Path = assetPath, Asset = stringTable };
    }
}
