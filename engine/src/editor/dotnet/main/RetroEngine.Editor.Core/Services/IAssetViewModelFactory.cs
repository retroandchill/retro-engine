// // @file IAssetViewFactory.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using RetroEngine.Assets;
using RetroEngine.Editor.Core.ViewModels;

namespace RetroEngine.Editor.Core.Services;

public interface IAssetViewModelFactory
{
    Type AssetType { get; }

    ValueTask<IAssetViewModel> CreateViewModelAsync(AssetPath assetPath, CancellationToken cancellationToken = default);
}
