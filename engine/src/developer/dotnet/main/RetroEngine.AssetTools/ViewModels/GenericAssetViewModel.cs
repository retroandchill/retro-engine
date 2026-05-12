// // @file GenericAssetViewModel.cs
// //
// // @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// // Licensed under the MIT License. See LICENSE file in the project root for full license information.

using AutoViewModel;
using Dock.Model.RetroEngine.Controls;
using RetroEngine.Assets;
using RetroEngine.AssetTools.Views;

namespace RetroEngine.AssetTools.ViewModels;

[ViewModelFor<GenericAssetView>]
public sealed partial class GenericAssetViewModel(AssetPath path, object asset) : Document, IAssetViewModel
{
    public AssetPath Path { get; } = path;
    public object Asset { get; } = asset;
    public bool IsReadOnly => true;
}
