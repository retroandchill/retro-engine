// @file IAssetViewModel.cs
//
// @copyright Copyright (c) 2026 Retro & Chill. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using Dock.Model.Controls;
using RetroEngine.Assets;

namespace RetroEngine.AssetTools.ViewModels;

public interface IAssetViewModel : IDocument
{
    AssetPath Path { get; }

    object Asset { get; }

    bool IsReadOnly { get; }
}

public interface IAssetViewModel<out T> : IAssetViewModel
    where T : class
{
    new T Asset { get; }

    object IAssetViewModel.Asset => Asset;
}
